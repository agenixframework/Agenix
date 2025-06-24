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
using Agenix.Http.Message;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Http.Tests.Message;

public class CookieEnricherTest
{
    private readonly CookieEnricher _cookieEnricher = new();

    private Cookie _cookie;
    private TestContext _testContextMock;

    [SetUp]
    public void BeforeMethod()
    {
        _testContextMock = new TestContext();
        _cookie = new Cookie("foo", "bar");
    }

    [Test]
    public void TestCookiesArePreserved()
    {
        // GIVEN
        var cookie = new Cookie("foo", "bar") { Domain = "domain", HttpOnly = true, Secure = true };
        var cookies = new List<Cookie> { cookie };

        var cookieEnricher = new CookieEnricher();

        // WHEN
        var enrichedCookies = cookieEnricher.Enrich(cookies, _testContextMock);

        // THEN
        ClassicAssert.AreEqual(1, enrichedCookies.Count, "Should preserve exactly one cookie");

        var enrichedCookie = enrichedCookies[0];
        ClassicAssert.AreEqual("foo", enrichedCookie.Name, "Cookie name should be preserved");
        ClassicAssert.AreEqual("bar", enrichedCookie.Value, "Cookie value should be preserved");
        ClassicAssert.AreEqual("domain", enrichedCookie.Domain, "Cookie Max-Age should be preserved");
        ClassicAssert.IsTrue(enrichedCookie.HttpOnly, "Cookie should remain HttpOnly enabled");
        ClassicAssert.IsTrue(enrichedCookie.Secure, "Cookie should remain Secure enabled");
    }

    [Test]
    public void TestTwoCookiesArePreserved()
    {
        // GIVEN
        var cookie = new Cookie("name", "value");
        var cookies = new List<Cookie> { cookie, cookie };

        var cookieEnricher = new CookieEnricher();

        // WHEN
        var enrichedCookies = cookieEnricher.Enrich(cookies, _testContextMock);

        // THEN
        ClassicAssert.AreEqual(2, enrichedCookies.Count, "The number of cookies after enrichment should be 2");
    }

    [Test]
    public void TestValueVariablesAreReplaced()
    {
        // GIVEN
        var cookie = new Cookie("foo", "${foobar}");
        var cookies = new List<Cookie> { cookie };

        _testContextMock.SetVariable("foobar", "bar");

        var cookieEnricher = new CookieEnricher();

        // WHEN
        var enrichedCookies = cookieEnricher.Enrich(cookies, _testContextMock);

        // THEN
        ClassicAssert.AreEqual("foo", enrichedCookies[0].Name, "Cookie name should be preserved");
        ClassicAssert.AreEqual("bar", enrichedCookies[0].Value, "Cookie value should have variable replaced");
    }

    [Test]
    public void TestPathVariablesAreReplaced()
    {
        // GIVEN
        var cookie = new Cookie("foo", "value") { Path = "/path/to/${variable}" };
        var cookies = new List<Cookie> { cookie };

        _testContextMock.SetVariable("variable", "foobar");

        var cookieEnricher = new CookieEnricher();

        // WHEN
        var enrichedCookies = cookieEnricher.Enrich(cookies, _testContextMock);

        // THEN
        ClassicAssert.AreEqual("foo", enrichedCookies[0].Name, "Cookie name should be preserved");
        ClassicAssert.AreEqual("/path/to/foobar", enrichedCookies[0].Path, "Cookie path should have variable replaced");
    }

    [Test]
    public void TestDomainVariablesAreReplaced()
    {
        // GIVEN
        var cookie = new Cookie("foo", "value") { Domain = "${variable}" };
        var cookies = new List<Cookie> { cookie };

        _testContextMock.SetVariable("variable", "localhost");

        var cookieEnricher = new CookieEnricher();

        // WHEN
        var enrichedCookies = cookieEnricher.Enrich(cookies, _testContextMock);

        // THEN
        ClassicAssert.AreEqual("foo", enrichedCookies[0].Name, "Cookie name should be preserved");
        ClassicAssert.AreEqual("localhost", enrichedCookies[0].Domain, "Cookie domain should have variable replaced");
    }
}
