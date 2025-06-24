#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System.Collections;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Http.Client;
using Newtonsoft.Json;

namespace Agenix.Http.Message;

public class HttpMessageConverter(CookieConverter cookieConverter)
    : IMessageConverter<HttpResponseMessage, HttpRequestMessage, HttpEndpointConfiguration>
{
    private static readonly Dictionary<Version, string> VersionMap = new()
    {
        { HttpVersion.Version20, "HTTP/2.0" },
        { HttpVersion.Version10, "HTTP/1.0" },
        { HttpVersion.Version30, "HTTP/3.0" }
    };

    public HttpMessageConverter() : this(new CookieConverter())
    {
    }

    /// <summary>
    ///     Converts an internal message into an outbound HTTP request, preparing it to be sent to an external endpoint.
    /// </summary>
    /// <param name="internalMessage">The internal message that needs to be converted into an HTTP request.</param>
    /// <param name="endpointConfiguration">
    ///     The configuration settings for the HTTP endpoint, which may affect the conversion
    ///     process.
    /// </param>
    /// <param name="context">The test context used to potentially resolve dynamic values within the message, such as cookies.</param>
    /// <returns>An HttpRequestMessage that represents the outbound HTTP request constructed from the internal message.</returns>
    public HttpRequestMessage ConvertOutbound(IMessage internalMessage, HttpEndpointConfiguration endpointConfiguration,
        TestContext context)
    {
        var httpMessage = ConvertOutboundMessage(internalMessage);

        var httpHeaders = CreateHttpHeaders(httpMessage);

        foreach (var cookie in httpMessage.GetCookies())
        {
            httpHeaders.Add("Cookie", cookie.Name + "=" + ResolveCookieValue(context, cookie));
        }

        var method = DetermineRequestMethod(endpointConfiguration, httpMessage);

        return CreateHttpRequest(httpHeaders, httpMessage, method, endpointConfiguration);
    }

    public void ConvertOutbound(HttpRequestMessage externalMessage, IMessage internalMessage,
        HttpEndpointConfiguration endpointConfiguration, TestContext context)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Converts an inbound <see cref="HttpResponseMessage" /> to an internal <see cref="IMessage" /> representation using
    ///     the given endpoint configuration and test context.
    /// </summary>
    /// <param name="externalMessage">The HTTP response message to be converted.</param>
    /// <param name="endpointConfiguration">
    ///     The configuration settings for the HTTP endpoint, which dictate how the message
    ///     should be processed, including whether to handle cookies.
    /// </param>
    /// <param name="context">
    ///     The test context that might be utilized during the conversion process for dynamic content
    ///     handling.
    /// </param>
    /// <returns>
    ///     An <see cref="IMessage" /> instance representing the internal format of the HTTP response, containing headers,
    ///     body, status, and cookies if applicable.
    /// </returns>
    public IMessage ConvertInbound(HttpResponseMessage externalMessage, HttpEndpointConfiguration endpointConfiguration,
        TestContext context)
    {
        var responseHeaders = GetHeaders(externalMessage.Headers);

        var httpMessage = new HttpMessage(ExtractMessageBody(externalMessage), responseHeaders);
        httpMessage.Status(externalMessage.StatusCode);


        SetHttpMessageVersion(externalMessage, httpMessage);

        if (endpointConfiguration.HandleCookies)
        {
            httpMessage.SetCookies(cookieConverter.ConvertCookies(externalMessage));
        }

        return httpMessage;
    }

    /// <summary>
    ///     Resolves the value of a cookie, potentially replacing dynamic content using the provided test context.
    /// </summary>
    /// <param name="context">
    ///     The test context used to replace dynamic content in the cookie value. If null, the cookie value
    ///     is returned as is.
    /// </param>
    /// <param name="cookie">The cookie whose value is to be resolved.</param>
    /// <returns>
    ///     A string representing the resolved value of the cookie, with dynamic content replaced if a context is
    ///     provided.
    /// </returns>
    private static string ResolveCookieValue(TestContext? context, Cookie cookie)
    {
        return context == null ? cookie.Value : context.ReplaceDynamicContentInString(cookie.Value);
    }

    /// <summary>
    ///     Converts the types of header values in the provided dictionary. If a value is an enumerable (except strings),
    ///     it is converted into a string by joining the elements with commas.
    /// </summary>
    /// <param name="headers">
    ///     A dictionary representing the headers to be processed, where the key is the header name
    ///     and the value is the header value.
    /// </param>
    /// <returns>
    ///     A dictionary with header values converted to strings if they were originally enumerables, maintaining
    ///     the original structure for other types.
    /// </returns>
    private Dictionary<string, object> ConvertHeaderTypes(Dictionary<string, object> headers)
    {
        var convertedHeaders = new Dictionary<string, object>();

        foreach (var header in headers)
        {
            if (header.Value is IEnumerable enumerable and not string)
            {
                // Convert each element to a string and join them with commas
                var joinedValues = string.Join(",", enumerable.Cast<object>().Select(v => v.ToString()));
                convertedHeaders[header.Key] = joinedValues;
            }
            else
            {
                convertedHeaders[header.Key] = header.Value;
            }
        }

        return convertedHeaders;
    }

    /// <summary>
    ///     Creates <see cref="HttpRequestHeaders" /> based on the provided HttpMessage and the endpoint configuration's header
    ///     mapper.
    /// </summary>
    /// <param name="httpMessage">The HttpMessage instance from which headers are to be copied.</param>
    /// <returns>
    ///     A set of <see cref="HttpRequestHeaders" /> representing the headers constructed from the given HttpMessage and
    ///     endpoint configuration.
    /// </returns>
    private HttpRequestHeaders CreateHttpHeaders(HttpMessage httpMessage)
    {
        var httpHeaders = new HttpRequestMessage().Headers;

        var messageHeaders = httpMessage.GetHeaders();
        foreach (var header in messageHeaders)
        {
            if (!header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase) &&
                !header.Key.StartsWith(MessageHeaders.Prefix) &&
                !MessageHeaderUtils.IsSpringInternalHeader(header.Key) &&
                !httpHeaders.TryGetValues(header.Key, out _))
            {
                httpHeaders.Add(header.Key, header.Value.ToString());
            }
        }

        return httpHeaders;
    }

    /// <summary>
    ///     Sets the HTTP version of the provided <see cref="HttpMessage" /> based on the version of the given
    ///     <see cref="HttpResponseMessage" />.
    /// </summary>
    /// <param name="externalMessage">The <see cref="HttpResponseMessage" /> containing the HTTP version information.</param>
    /// <param name="httpMessage">The <see cref="HttpMessage" /> whose version is to be set.</param>
    private void SetHttpMessageVersion(HttpResponseMessage externalMessage, HttpMessage httpMessage)
    {
        var version = VersionMap.GetValueOrDefault(externalMessage.Version, "HTTP/1.1");
        // Default version
        httpMessage.Version(version);
    }

    private Dictionary<string, object> GetHeadersFromHttpResponseHeaders(HttpResponseHeaders httpHeaders)
    {
        var customHeaders = new Dictionary<string, object>();

        foreach (var header in httpHeaders)
        {
            customHeaders[header.Key] = string.Join(",", header.Value);
        }

        return customHeaders;
    }

    private Dictionary<string, object> GetHeaders(HttpHeaders httpHeaders)
    {
        var customHeaders = new Dictionary<string, object>();

        foreach (var header in httpHeaders)
        {
            customHeaders[header.Key] = string.Join(",", header.Value);
        }

        return customHeaders;
    }

    /// <summary>
    ///     Determines the HTTP request method to be used for the outbound message.
    /// </summary>
    /// <param name="endpointConfiguration">
    ///     The configuration settings for the HTTP endpoint, including the default request
    ///     method.
    /// </param>
    /// <param name="httpMessage">
    ///     The HTTP message which may contain a specific request method that overrides the default
    ///     method.
    /// </param>
    /// <returns>
    ///     The HTTP request method, either from the HTTP message if specified, or the default method from the endpoint
    ///     configuration.
    /// </returns>
    private HttpMethod? DetermineRequestMethod(HttpEndpointConfiguration endpointConfiguration, HttpMessage httpMessage)
    {
        var method = endpointConfiguration.RequestMethod;

        if (httpMessage.GetRequestMethod() != null)
        {
            method = httpMessage.GetRequestMethod();
        }

        return method;
    }

    /// <summary>
    ///     Converts an outbound IMessage object into an HttpMessage.
    /// </summary>
    /// <param name="message">The IMessage object to be converted.</param>
    /// <returns>
    ///     An HttpMessage that represents the converted IMessage. If the input message is already an HttpMessage, it is
    ///     returned as is; otherwise, a new HttpMessage is created based on the input.
    /// </returns>
    private HttpMessage ConvertOutboundMessage(IMessage message)
    {
        if (message is HttpMessage httpMessage)
        {
            return httpMessage;
        }

        return new HttpMessage(message);
    }

    /// <summary>
    ///     Determines whether the given HTTP method supports a message body.
    /// </summary>
    /// <param name="method">The HttpMethod to evaluate.</param>
    /// <returns>A boolean indicating whether a message body is supported for the specified HTTP method.</returns>
    private bool HttpMethodSupportsBody(HttpMethod? method)
    {
        return method == HttpMethod.Post || method == HttpMethod.Put
                                         || method == HttpMethod.Delete || method == HttpMethod.Patch;
    }

    /// <summary>
    ///     Composes a Content-Type header value by appending the charset if it's not already included
    ///     in the Content-Type and if the charset is specified in the endpoint configuration.
    /// </summary>
    /// <param name="endpointConfiguration">
    ///     The endpoint configuration containing the Content-Type and charset information to
    ///     construct the header value.
    /// </param>
    /// <returns>A string representing the composed Content-Type header value, including the charset if applicable.</returns>
    private string ComposeContentTypeHeaderValue(HttpEndpointConfiguration endpointConfiguration)
    {
        return endpointConfiguration.ContentType.Contains("charset") ||
               !StringUtils.HasText(endpointConfiguration.Charset)
            ? endpointConfiguration.ContentType
            : $"{endpointConfiguration.ContentType};charset={endpointConfiguration.Charset}";
    }

    private MediaTypeHeaderValue ComposeMediaTypeHeaderValue(HttpMessage httpMessage,
        HttpEndpointConfiguration endpointConfiguration)
    {
        if (StringUtils.HasText(httpMessage.GetContentType()))
        {
            var contentType = httpMessage.GetContentType();
            var charsetIndex = contentType.IndexOf("charset=", StringComparison.OrdinalIgnoreCase);

            if (charsetIndex > -1)
            {
                var mediaType = contentType[..charsetIndex].TrimEnd(';').Trim();
                var charset = contentType[(charsetIndex + 8)..];
                return new MediaTypeHeaderValue(mediaType) { CharSet = charset };
            }
            else
            {
                var mediaType = contentType.TrimEnd(';').Trim();
                return new MediaTypeHeaderValue(mediaType);
            }
        }

        if (StringUtils.HasText(endpointConfiguration.Charset))
        {
            return new MediaTypeHeaderValue(endpointConfiguration.ContentType)
            {
                CharSet = endpointConfiguration.Charset
            };
        }

        return new MediaTypeHeaderValue(endpointConfiguration.ContentType);
    }

    /// <summary>
    ///     Creates an <see cref="HttpRequestMessage" /> with the specified HTTP headers, payload, HTTP method, and
    ///     endpoint configuration. Prepares a request to be sent to an HTTP endpoint.
    /// </summary>
    /// <param name="httpHeaders">
    ///     The collection of headers to be added to the HTTP request. Headers may include request-level
    ///     or content-level configurations depending on their type.
    /// </param>
    /// <param name="httpMessage">
    ///     The HTTP message that contains the payload data to be included in the request.
    /// </param>
    /// <param name="method">
    ///     The HTTP method that defines the action to be performed, such as GET, POST, or PUT. Defaults
    ///     to GET if null.
    /// </param>
    /// <param name="endpointConfiguration">
    ///     The configuration for the HTTP endpoint which may include authentication, base URL, and other
    ///     settings influencing the request construction.
    /// </param>
    /// <returns>An <see cref="HttpRequestMessage" /> that is fully configured and ready to be sent to the target endpoint.</returns>
    private HttpRequestMessage CreateHttpRequest(HttpRequestHeaders httpHeaders, HttpMessage httpMessage,
        HttpMethod? method,
        HttpEndpointConfiguration endpointConfiguration)
    {
        var payload = httpMessage.Payload;
        var httpRequestMessage = new HttpRequestMessage { Method = method ?? HttpMethod.Get };

        // Set headers
        foreach (var header in httpHeaders)
        {
            if (!httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
            {
                // If header can't be added to HttpRequestMessage.Headers, try adding to Content.Headers
                httpRequestMessage.Content ??= new StringContent(string.Empty);
                httpRequestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        if (!HttpMethodSupportsBody(method))
        {
            return httpRequestMessage;
        }


        // Set content if payload is provided
        // Handle payload based on content type
        if (payload != null)
        {
            if (payload is string stringPayload)
            {
                httpRequestMessage.Content = new StringContent(stringPayload);
                httpRequestMessage.Content.Headers.ContentType =
                    ComposeMediaTypeHeaderValue(httpMessage, endpointConfiguration);
            }
            else if ((endpointConfiguration.ContentType.Contains(MediaTypeNames.Application.Octet,
                          StringComparison.OrdinalIgnoreCase) || endpointConfiguration.ContentType.Contains(
                          MediaTypeNames.Application.Pdf,
                          StringComparison.OrdinalIgnoreCase) || endpointConfiguration.ContentType.Contains(
                          MediaTypeNames.Application.Zip,
                          StringComparison.OrdinalIgnoreCase) ||
                      endpointConfiguration.ContentType.Contains(MediaTypeNames.Image.Gif,
                          StringComparison.OrdinalIgnoreCase)
                      || endpointConfiguration.ContentType.Contains(MediaTypeNames.Image.Jpeg,
                          StringComparison.OrdinalIgnoreCase)
                      || endpointConfiguration.ContentType.Contains(MediaTypeNames.Image.Png,
                          StringComparison.OrdinalIgnoreCase))
                     && payload is byte[] bytePayload)
            {
                httpRequestMessage.Content = new ByteArrayContent(bytePayload);
                httpRequestMessage.Content.Headers.ContentType =
                    ComposeMediaTypeHeaderValue(httpMessage, endpointConfiguration);
            }
            else
            {
                // Default to JSON serialization for objects
                var jsonPayload = JsonConvert.SerializeObject(payload);
                httpRequestMessage.Content = new StringContent(jsonPayload);
                httpRequestMessage.Content.Headers.ContentType =
                    ComposeMediaTypeHeaderValue(httpMessage, endpointConfiguration);
            }
        }

        return httpRequestMessage;
    }

    /// <summary>
    ///     Extracts the body content from an HTTP response message.
    /// </summary>
    /// <param name="responseMessage">The HTTP response message from which to extract the body.</param>
    /// <returns>
    ///     An object representing the extracted content of the response body, which could be a string or a byte array
    ///     depending on the content type.
    /// </returns>
    private object ExtractMessageBody(HttpResponseMessage responseMessage)
    {
        if (responseMessage?.Content == null)
        {
            return string.Empty;
        }

        // Check the content type
        var contentType = responseMessage.Content.Headers.ContentType?.MediaType;

        return contentType switch
        {
            "application/json" or "text/plain" => responseMessage.Content.ReadAsStringAsync().Result,
            "application/octet-stream" => responseMessage.Content.ReadAsByteArrayAsync().Result,
            _ => responseMessage.Content.ReadAsStringAsync().Result
        };
    }
}
