#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Net.Http;
using System.Threading.Tasks;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Condition;

/// <summary>
///     Tests if an HTTP Endpoint is reachable. The test is successful if the endpoint responds with the expected
///     response code. By default, a HTTP 200 response code is expected.
/// </summary>
public class HttpCondition() : AbstractCondition("http-check")
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(HttpCondition));

    /// <summary>
    ///     Represents the expected HTTP response code used to validate
    ///     if an HTTP request returns the correct status.
    /// </summary>
    public string HttpResponseCode { get; set; } = "200";

    /// <summary>
    ///     The HTTP method used to make the web request, e.g., GET, POST, HEAD, etc.
    /// </summary>
    public string Method { get; set; } = "HEAD";

    /// <summary>
    ///     URL to be checked as part of the HTTP condition.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    ///     Specifies the timeout duration, in milliseconds, for the HTTP request.
    /// </summary>
    public string Timeout { get; set; } = "1000";

    /// Evaluates whether the HTTP condition is satisfied based on the test context.
    /// Compares the HTTP response code with the expected value obtained by invoking a URL within the given test context.
    /// <param name="context">The test context containing necessary parameters for the HTTP condition evaluation.</param>
    /// <return>
    ///     True if the HTTP response code matches the expected value, otherwise false.
    /// </return>
    /// /
    public override bool IsSatisfied(TestContext context)
    {
        return GetHttpResponseCode(context) == InvokeUrl(context);
    }

    /// Constructs a success message for the HTTP condition, indicating that the request
    /// to a specific URL returned the expected status code.
    /// <param name="context">The test context containing execution details to retrieve URL and response code information.</param>
    /// <return>A formatted string conveying the success message for the HTTP condition.</return>
    /// /
    public override string GetSuccessMessage(TestContext context)
    {
        return string.Format("Http condition success - request url '{0}' did return expected status '{1}'",
            GetUrl(context), GetHttpResponseCode(context));
    }

    /// Generates an error message if the HTTP condition check fails.
    /// <param name="context">The test context containing details of the HTTP request.</param>
    /// <return>A string describing the error encountered during the HTTP condition check.</return>
    /// /
    public override string GetErrorMessage(TestContext context)
    {
        return string.Format("Failed to check Http condition - request url '{0}' did not return expected status '{1}'",
            GetUrl(context), GetHttpResponseCode(context));
    }

    /// Invokes an HTTP request using the specified context and returns the HTTP response code.
    /// <param name="context">The context that provides configuration and settings for the HTTP request</param>
    /// <return>The HTTP response code indicating the result of the request</return>
    /// /
    private int InvokeUrl(TestContext context)
    {
        var contextUrl = GetUrl(context);
        if (Log.IsEnabled(LogLevel.Debug)) Log.LogDebug($"Probing Http request url '{contextUrl.ToString()}'");

        var responseCode = -1;

        try
        {
            responseCode = ExecuteHttpRequestAsync(contextUrl, context).GetAwaiter().GetResult();
        }
        catch (HttpRequestException e)
        {
            // HttpRequestException doesn't directly expose status codes like WebException did
            // We can extract it if it's available in the exception data
            if (e.StatusCode.HasValue) responseCode = (int)e.StatusCode.Value;
            Log.LogWarning($"Could not access Http url '{contextUrl.ToString()}' - {e.Message}");
        }
        catch (TaskCanceledException)
        {
            // Handle timeout
            Log.LogWarning($"Request to '{contextUrl.ToString()}' timed out after {GetTimeout(context)}ms");
        }

        return responseCode;
    }

    /// Sends an HTTP request to a specified URL using the method and timeout defined in the context.
    /// Evaluates the HTTP response and returns the status code as an integer.
    /// <param name="url">The URI of the HTTP endpoint to which the request will be made.</param>
    /// <param name="context">
    ///     The test context containing the necessary parameters, such as method and timeout, for the HTTP
    ///     request.
    /// </param>
    /// <return>
    ///     The response status code represented as an integer, corresponding to the HTTP response from the endpoint.
    /// </return>
    private async Task<int> ExecuteHttpRequestAsync(Uri url, TestContext context)
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromMilliseconds(GetTimeout(context));

        var method = context.ResolveDynamicValue(Method);
        using var requestMessage = new HttpRequestMessage(new HttpMethod(method), url);
        using var response = await httpClient.SendAsync(requestMessage);

        return (int)response.StatusCode;
    }

    /// Gets the request URL with test variable support.
    /// <param name="context">The test context to get the URL from.</param>
    /// <return>The extracted URL.</return>
    /// /
    private Uri GetUrl(TestContext context)
    {
        try
        {
            return new Uri(context.ReplaceDynamicContentInString(Url));
        }
        catch (UriFormatException e)
        {
            throw new AgenixSystemException("Invalid request url", e);
        }
    }

    /// Retrieves the timeout value in milliseconds from the provided test context.
    /// <param name="context">The test context from which to extract the timeout.</param>
    /// <return>The extracted timeout as an integer.</return>
    /// /
    private int GetTimeout(TestContext context)
    {
        return int.Parse(context.ResolveDynamicValue(Timeout));
    }

    /// Gets the expected Http response code.
    /// <param name="context">The test context to get the response code from</param>
    /// <return>The extracted response code</return>
    /// /
    private int GetHttpResponseCode(TestContext context)
    {
        return int.Parse(context.ResolveDynamicValue(HttpResponseCode));
    }


    public override string ToString()
    {
        return "HttpCondition{" +
               "url='" + Url + '\'' +
               ", timeout='" + Timeout + '\'' +
               ", httpResponseCode='" + HttpResponseCode + '\'' +
               ", method='" + Method + '\'' +
               ", name='" + GetName() + '\'' +
               '}';
    }
}
