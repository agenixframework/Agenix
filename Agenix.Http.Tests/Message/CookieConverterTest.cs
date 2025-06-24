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
using System.Net.Http.Headers;
using Agenix.Http.Message;
using NUnit.Framework.Legacy;

namespace Agenix.Http.Tests.Message;

public class CookieConverterTest
{
    private readonly CookieConverter _cookieConverter = new();
    private Cookie _cookie;
    private HttpRequestHeaders _httpRequestHeaders;
    private HttpResponseHeaders _httpResponseHeaders;

    [SetUp]
    public void SetUpMethod()
    {
        _cookie = new Cookie("foo", "bar");
        _httpRequestHeaders = new HttpRequestMessage().Headers;
        _httpResponseHeaders = new HttpResponseMessage().Headers;
    }

    [Test]
    public void TestCookiesAreParsedCorrectly()
    {
        // GIVEN
        _httpResponseHeaders.TryAddWithoutValidation("Set-Cookie", new List<string> { "foo=bar" });
        var responseMessage = new HttpResponseMessage();
        foreach (var header in _httpResponseHeaders)
        {
            responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // WHEN
        var cookies = _cookieConverter.ConvertCookies(responseMessage);

        // THEN
        ClassicAssert.AreEqual("foo", cookies[0].Name);
        ClassicAssert.AreEqual("bar", cookies[0].Value);
    }

    [Test]
    public void TestAdditionalCookieDirectivesAreDiscarded()
    {
        // GIVEN
        _httpResponseHeaders.TryAddWithoutValidation("Set-Cookie", new List<string> { "foo=bar;HttpOnly" });
        var responseMessage = new HttpResponseMessage();
        foreach (var header in _httpResponseHeaders)
        {
            responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // WHEN
        var cookies = _cookieConverter.ConvertCookies(responseMessage);

        // THEN
        ClassicAssert.AreEqual("foo", cookies[0].Name);
        ClassicAssert.AreEqual("bar", cookies[0].Value);
    }

    [Test]
    public void TestCookieCommentIsNoLongerPreserved()
    {
        // GIVEN
        _httpResponseHeaders.TryAddWithoutValidation("Set-Cookie", new List<string> { "foo=bar;Comment=wtf" });
        var responseMessage = new HttpResponseMessage();
        foreach (var header in _httpResponseHeaders)
        {
            responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // WHEN
        var cookies = _cookieConverter.ConvertCookies(responseMessage);

        // THEN
        Assert.That(cookies[0].Comment, Is.Empty);
    }

    [Test]
    public void TestCookieDomainIsPreserved()
    {
        // GIVEN
        _httpResponseHeaders.TryAddWithoutValidation("Set-Cookie", new List<string> { "foo=bar;Domain=whatever" });
        var responseMessage = new HttpResponseMessage();
        foreach (var header in _httpResponseHeaders)
        {
            responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // WHEN
        var cookies = _cookieConverter.ConvertCookies(responseMessage);

        // THEN
        ClassicAssert.AreEqual("whatever", cookies[0].Domain);
    }

    [Test]
    public void TestCookieMaxAgeIsPreserved()
    {
        // GIVEN
        _httpResponseHeaders.TryAddWithoutValidation("Set-Cookie", new List<string> { "foo=bar;Max-Age=42" });
        var responseMessage = new HttpResponseMessage();
        foreach (var header in _httpResponseHeaders)
        {
            responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // WHEN
        var cookies = _cookieConverter.ConvertCookies(responseMessage);

        // THEN
        var expectedExpiration = DateTime.UtcNow.AddSeconds(42);
        Assert.That(cookies[0].Expires, Is.EqualTo(expectedExpiration).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void TestCookiePathIsPreserved()
    {
        // GIVEN
        _httpResponseHeaders.TryAddWithoutValidation("Set-Cookie", new List<string> { "foo=bar;Path=foobar" });
        var responseMessage = new HttpResponseMessage();
        foreach (var header in _httpResponseHeaders)
        {
            responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // WHEN
        var cookies = _cookieConverter.ConvertCookies(responseMessage);

        // THEN
        ClassicAssert.AreEqual("foobar", cookies[0].Path);
    }

    [Test]
    public void TestCookieSecureIsPreserved()
    {
        // GIVEN
        _httpResponseHeaders.TryAddWithoutValidation("Set-Cookie", new List<string> { "foo=bar;Secure" });
        var responseMessage = new HttpResponseMessage();
        foreach (var header in _httpResponseHeaders)
        {
            responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // WHEN
        var cookies = _cookieConverter.ConvertCookies(responseMessage);

        // THEN
        ClassicAssert.IsTrue(cookies[0].Secure);
    }

    [Test]
    public void TestCookieVersionIsNoLongerPreserved()
    {
        // GIVEN
        _httpResponseHeaders.TryAddWithoutValidation("Set-Cookie", new List<string> { "foo=bar;Version=1" });
        var responseMessage = new HttpResponseMessage();
        foreach (var header in _httpResponseHeaders)
        {
            responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // WHEN
        var cookies = _cookieConverter.ConvertCookies(responseMessage);

        // THEN
        ClassicAssert.AreEqual(0, cookies[0].Version);
    }

    [Test]
    public void TestCookieHttpOnlyIsPreserved()
    {
        // GIVEN
        _httpResponseHeaders.TryAddWithoutValidation("Set-Cookie", new List<string> { "foo=bar;HttpOnly" });
        var responseMessage = new HttpResponseMessage();
        foreach (var header in _httpResponseHeaders)
        {
            responseMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // WHEN
        var cookies = _cookieConverter.ConvertCookies(responseMessage);

        // THEN
        ClassicAssert.IsTrue(cookies[0].HttpOnly);
    }

    [Test]
    public void TestCookieStringContainsPath()
    {
        // GIVEN
        _cookie.Path = "/foo/bar";
        const string expectedCookieString = "foo=bar;Path=/foo/bar";

        // WHEN
        var cookieString = _cookieConverter.GetCookieString(_cookie);

        // THEN
        ClassicAssert.AreEqual(expectedCookieString, cookieString);
    }

    [Test]
    public void TestCookieStringContainsDomain()
    {
        // GIVEN
        _cookie.Domain = "localhost";
        const string expectedCookieString = "foo=bar;Domain=localhost";

        // WHEN
        var cookieString = _cookieConverter.GetCookieString(_cookie);

        // THEN
        ClassicAssert.AreEqual(expectedCookieString, cookieString);
    }

    [Test]
    public void TestCookieStringContainsMaxAge()
    {
        // GIVEN
        const int expectedMaxAge = 42;
        var expires = DateTime.UtcNow.AddSeconds(expectedMaxAge);
        _cookie.Expires = expires;

        // Convert the expiration to Max-Age
        var maxAge = Math.Round((expires - DateTime.UtcNow).TotalSeconds);
        var expectedCookieString = $"foo=bar;Max-Age={maxAge}";

        // WHEN
        var cookieString = _cookieConverter.GetCookieString(_cookie);

        // Extract the actual Max-Age from the cookie string
        var parts = cookieString.Split(';');
        var maxAgeString = Array.Find(parts, part => part.StartsWith("Max-Age"));
        var actualMaxAge = int.Parse(maxAgeString.Split('=')[1]);

        // THEN
        Assert.That(actualMaxAge, Is.InRange(expectedMaxAge - 1, expectedMaxAge + 1)); // Allowing 1 second tolerance
    }

    [Test]
    public void TestCookieStringContainsComment()
    {
        // GIVEN
        _cookie.Comment = "whatever";
        const string expectedCookieString = "foo=bar";

        // WHEN
        var cookieString = _cookieConverter.GetCookieString(_cookie);

        // THEN
        ClassicAssert.AreEqual(expectedCookieString, cookieString);
    }

    [Test]
    public void TestCookieStringContainsSecure()
    {
        // GIVEN
        _cookie.Secure = true;
        const string expectedCookieString = "foo=bar;Secure";

        // WHEN
        var cookieString = _cookieConverter.GetCookieString(_cookie);

        // THEN
        ClassicAssert.AreEqual(expectedCookieString, cookieString);
    }

    [Test]
    public void TestCookieStringContainsHttpOnly()
    {
        // GIVEN
        _cookie.HttpOnly = true;
        const string expectedCookieString = "foo=bar;HttpOnly";

        // WHEN
        var cookieString = _cookieConverter.GetCookieString(_cookie);

        // THEN
        ClassicAssert.AreEqual(expectedCookieString, cookieString);
    }
}
