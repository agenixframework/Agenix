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

using System.Net;
using System.Net.Mime;
using System.Text;
using Agenix.Api.Message;
using Agenix.Http.Client;
using Agenix.Http.Message;
using Moq;
using HttpClient = Agenix.Http.Client.HttpClient;

namespace Agenix.Http.Tests.Client;

public class HttpClientTest : AbstractNUnitSetUp
{
    private const int ServerPort = 8078;

    private const string RequestBody = "<TestRequest><Message>Hello Agenix!</Message></TestRequest>";
    private const string ResponseBody = "<TestResponse><Message>Hello World!</Message></TestResponse>";

    private HttpListener _server;

    [SetUp]
    public void StartServer()
    {
        _server = new HttpListener();
        _server.Prefixes.Add($"http://localhost:{ServerPort}/test/");
        _server.Start();
        _server.BeginGetContext(OnRequest, null);
    }

    [TearDown]
    public void StopServer()
    {
        try
        {
            if (_server is { IsListening: true })
            {
                _server.Stop();
            }

            _server.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping listener: {ex.Message}");
        }
    }

    private void OnRequest(IAsyncResult result)
    {
        if (result == null || !_server.IsListening)
        {
            return;
        }

        try
        {
            var context = _server.EndGetContext(result);
            context.Response.StatusCode = 200;
            var buffer = Encoding.UTF8.GetBytes(ResponseBody);
            context.Response.ContentLength64 = buffer.Length; // Specifies the Content-Length of the response
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);

            // Make sure to close the response
            context.Response.OutputStream.Close();
            context.Response.Close();

            _server.BeginGetContext(OnRequest, null);
        }
        catch (HttpListenerException ex)
        {
            Console.WriteLine($"HttpListener exception: {ex.Message}");
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("Listener has been disposed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unhandled exception: {ex}");
        }
    }

    [Test]
    public void TestHttpPostRequest()
    {
        var endpointConfiguration = new HttpEndpointConfiguration();
        var httpClient = new HttpClient(endpointConfiguration);
        var requestUrl = $"http://localhost:{ServerPort}/test/";

        endpointConfiguration.RequestMethod = HttpMethod.Post;
        endpointConfiguration.RequestUrl = requestUrl;

        var requestMessage = new HttpMessage(RequestBody);

        Assert.That(requestMessage.GetPayload<string>(), Is.EqualTo(RequestBody));
        Assert.That(requestMessage.GetHeaders().Count, Is.EqualTo(2));

        // Act
        httpClient.Send(requestMessage, Context);
        var responseMessage = (HttpMessage)httpClient.Receive(Context, endpointConfiguration.Timeout);

        Assert.That(responseMessage.GetPayload<string>(), Is.EqualTo(ResponseBody));
        Assert.That(responseMessage.GetStatusCode(), Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseMessage.GetVersion(), Is.EqualTo("HTTP/1.1"));
        Assert.That(responseMessage.GetReasonPhrase(), Is.EqualTo("OK"));
        ;
    }

    [Test]
    public void TestCustomHeaders()
    {
        var endpointConfiguration = new HttpEndpointConfiguration();
        var httpClient = new HttpClient(endpointConfiguration);
        var requestUrl = $"http://localhost:{ServerPort}/test/";

        endpointConfiguration.RequestMethod = HttpMethod.Post;
        endpointConfiguration.RequestUrl = requestUrl;
        endpointConfiguration.ContentType = "text/xml";
        endpointConfiguration.Charset = "ISO-8859-1";

        var requestMessage = new HttpMessage(RequestBody)
            .Header("Operation", "foo");

        Assert.That(requestMessage.GetPayload<string>(), Is.EqualTo(RequestBody));
        Assert.That(requestMessage.GetHeaders().Count, Is.EqualTo(3));
        Assert.That(requestMessage.GetHeader("Operation"), Is.EqualTo("foo"));

        // Act
        httpClient.Send(requestMessage, Context);
        var responseMessage = (HttpMessage)httpClient.Receive(Context, endpointConfiguration.Timeout);

        Assert.That(responseMessage.GetPayload<string>(), Is.EqualTo(ResponseBody));
        Assert.That(responseMessage.GetStatusCode(), Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseMessage.GetVersion(), Is.EqualTo("HTTP/1.1"));
        Assert.That(responseMessage.GetReasonPhrase(), Is.EqualTo("OK"));
        ;
    }

    [Test]
    public void TestOverwriteContentTypeHeader()
    {
        var endpointConfiguration = new HttpEndpointConfiguration();
        var httpClient = new HttpClient(endpointConfiguration);
        var requestUrl = $"http://localhost:{ServerPort}/test/";

        endpointConfiguration.RequestMethod = HttpMethod.Post;
        endpointConfiguration.RequestUrl = requestUrl;
        endpointConfiguration.ContentType = "text/xml";
        endpointConfiguration.Charset = "ISO-8859-1";

        var requestMessage = new HttpMessage(RequestBody)
            .ContentType("application/xml;charset=UTF-8")
            .Accept("application/xml");

        Assert.That(requestMessage.GetPayload<string>(), Is.EqualTo(RequestBody));
        Assert.That(requestMessage.GetHeaders().Count, Is.EqualTo(4));
        Assert.That(requestMessage.GetContentType(), Is.EqualTo("application/xml;charset=UTF-8"));
        Assert.That(requestMessage.GetAccept(), Is.EqualTo("application/xml"));

        // Act
        httpClient.Send(requestMessage, Context);
        var responseMessage = (HttpMessage)httpClient.Receive(Context, endpointConfiguration.Timeout);

        Assert.That(responseMessage.GetPayload<string>(), Is.EqualTo(ResponseBody));
        Assert.That(responseMessage.GetStatusCode(), Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseMessage.GetVersion(), Is.EqualTo("HTTP/1.1"));
        Assert.That(responseMessage.GetReasonPhrase(), Is.EqualTo("OK"));
        ;
    }

    [Test]
    public void TestOverwriteRequestMethod()
    {
        var endpointConfiguration = new HttpEndpointConfiguration();
        var httpClient = new HttpClient(endpointConfiguration);
        var requestUrl = $"http://localhost:{ServerPort}/test/";

        endpointConfiguration.RequestMethod = HttpMethod.Post;
        endpointConfiguration.RequestUrl = requestUrl;

        var requestMessage = new HttpMessage(RequestBody)
            .Method(HttpMethod.Get);

        Assert.That(requestMessage.GetPayload<string>(), Is.EqualTo(RequestBody));
        Assert.That(requestMessage.GetHeaders().Count, Is.EqualTo(3));

        // Act
        httpClient.Send(requestMessage, Context);
        var responseMessage = (HttpMessage)httpClient.Receive(Context, endpointConfiguration.Timeout);

        Assert.That(responseMessage.GetPayload<string>(), Is.EqualTo(ResponseBody));
        Assert.That(responseMessage.GetStatusCode(), Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseMessage.GetVersion(), Is.EqualTo("HTTP/1.1"));
        Assert.That(responseMessage.GetReasonPhrase(), Is.EqualTo("OK"));
        ;
    }

    [Test]
    public void TestHttpGetRequest()
    {
        var endpointConfiguration = new HttpEndpointConfiguration();
        var httpClient = new HttpClient(endpointConfiguration);
        var requestUrl = $"http://localhost:{ServerPort}/test/";

        endpointConfiguration.RequestMethod = HttpMethod.Get;
        endpointConfiguration.RequestUrl = requestUrl;

        var requestMessage = new HttpMessage(RequestBody)
            .Accept("application/xml");

        Assert.That(requestMessage.GetPayload<string>(), Is.EqualTo(RequestBody));
        Assert.That(requestMessage.GetHeaders().Count, Is.EqualTo(3));

        // Act
        httpClient.Send(requestMessage, Context);
        var responseMessage = (HttpMessage)httpClient.Receive(Context, endpointConfiguration.Timeout);

        Assert.That(responseMessage.GetPayload<string>(), Is.EqualTo(ResponseBody));
        Assert.That(responseMessage.GetStatusCode(), Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseMessage.GetVersion(), Is.EqualTo("HTTP/1.1"));
        Assert.That(responseMessage.GetReasonPhrase(), Is.EqualTo("OK"));
        ;
    }

    [Test]
    public void TestHttpPutRequest()
    {
        var endpointConfiguration = new HttpEndpointConfiguration();
        var httpClient = new HttpClient(endpointConfiguration);
        var requestUrl = $"http://localhost:{ServerPort}/test/";

        endpointConfiguration.RequestMethod = HttpMethod.Put;
        endpointConfiguration.RequestUrl = requestUrl;

        var requestMessage = new HttpMessage(RequestBody);

        Assert.That(requestMessage.GetPayload<string>(), Is.EqualTo(RequestBody));
        Assert.That(requestMessage.GetHeaders().Count, Is.EqualTo(2));

        // Act
        httpClient.Send(requestMessage, Context);
        var responseMessage = (HttpMessage)httpClient.Receive(Context, endpointConfiguration.Timeout);

        Assert.That(responseMessage.GetPayload<string>(), Is.EqualTo(ResponseBody));
        Assert.That(responseMessage.GetStatusCode(), Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseMessage.GetVersion(), Is.EqualTo("HTTP/1.1"));
        Assert.That(responseMessage.GetReasonPhrase(), Is.EqualTo("OK"));
        ;
    }

    [Test]
    public void TestReplyMessageCorrelator()
    {
        var endpointConfiguration = new HttpEndpointConfiguration();
        var httpClient = new HttpClient(endpointConfiguration);
        var requestUrl = $"http://localhost:{ServerPort}/test/";

        endpointConfiguration.RequestMethod = HttpMethod.Get;
        endpointConfiguration.RequestUrl = requestUrl;

        var messageCorrelator = new Mock<IMessageCorrelator>();
        endpointConfiguration.Correlator = messageCorrelator.Object;

        var requestMessage = new HttpMessage(RequestBody);

        Assert.That(requestMessage.GetPayload<string>(), Is.EqualTo(RequestBody));
        Assert.That(requestMessage.GetHeaders().Count, Is.EqualTo(2));

        // Setup the behavior for GetCorrelationKey method
        messageCorrelator.Setup(c => c.GetCorrelationKey(requestMessage))
            .Returns("correlationKey");

        // Setup the behavior for GetCorrelationKeyName method
        messageCorrelator.Setup(c => c.GetCorrelationKeyName(It.IsAny<string>()))
            .Returns("correlationKeyName");

        // Act
        httpClient.Send(requestMessage, Context);
        var responseMessage = (HttpMessage)httpClient.Receive("correlationKey", Context, endpointConfiguration.Timeout);

        Assert.That(responseMessage.GetPayload<string>(), Is.EqualTo(ResponseBody));
        Assert.That(responseMessage.GetStatusCode(), Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseMessage.GetVersion(), Is.EqualTo("HTTP/1.1"));
        Assert.That(responseMessage.GetReasonPhrase(), Is.EqualTo("OK"));
        ;
    }

    [Test]
    public void TestHttpPatchRequest()
    {
        var endpointConfiguration = new HttpEndpointConfiguration();
        var httpClient = new HttpClient(endpointConfiguration);
        var requestUrl = $"http://localhost:{ServerPort}/test/";

        endpointConfiguration.RequestMethod = HttpMethod.Patch;
        endpointConfiguration.RequestUrl = requestUrl;

        var requestMessage = new HttpMessage(RequestBody);

        Assert.That(requestMessage.GetPayload<string>(), Is.EqualTo(RequestBody));
        Assert.That(requestMessage.GetHeaders().Count, Is.EqualTo(2));

        // Act
        httpClient.Send(requestMessage, Context);
        var responseMessage = (HttpMessage)httpClient.Receive(Context, endpointConfiguration.Timeout);

        Assert.That(responseMessage.GetPayload<string>(), Is.EqualTo(ResponseBody));
        Assert.That(responseMessage.GetStatusCode(), Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseMessage.GetVersion(), Is.EqualTo("HTTP/1.1"));
        Assert.That(responseMessage.GetReasonPhrase(), Is.EqualTo("OK"));
        ;
    }

    [Test]
    public void TestBinaryBody()
    {
        var endpointConfiguration = new HttpEndpointConfiguration();
        var httpClient = new HttpClient(endpointConfiguration);
        var requestUrl = $"http://localhost:{ServerPort}/test/";

        var responseBody = new byte[20];
        new Random().NextBytes(responseBody);

        var requestBody = new byte[20];
        new Random().NextBytes(requestBody);

        endpointConfiguration.RequestMethod = HttpMethod.Post;
        endpointConfiguration.RequestUrl = requestUrl;

        var requestMessage = new HttpMessage(requestBody)
            .Accept(MediaTypeNames.Application.Octet)
            .ContentType(MediaTypeNames.Application.Octet);

        Assert.That(requestMessage.GetPayload<byte[]>(), Is.EqualTo(requestBody));
        Assert.That(requestMessage.GetHeaders().Count, Is.EqualTo(4));

        // Act
        httpClient.Send(requestMessage, Context);
        var responseMessage = (HttpMessage)httpClient.Receive(Context, endpointConfiguration.Timeout);

        Assert.That(responseMessage.GetPayload<string>(), Is.EqualTo(ResponseBody));
        Assert.That(responseMessage.GetStatusCode(), Is.EqualTo(HttpStatusCode.OK));
        Assert.That(responseMessage.GetVersion(), Is.EqualTo("HTTP/1.1"));
        Assert.That(responseMessage.GetReasonPhrase(), Is.EqualTo("OK"));
        ;
    }
}
