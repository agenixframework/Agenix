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

using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

public class ClearBrowserCacheActionTest : AbstractNUnitSetUp
{
    private readonly SeleniumBrowser _seleniumBrowser = new();
    public readonly Mock<IWebDriver> WebDriver = new();
    private readonly Mock<IOptions> _webDriverOptions = new();
    private readonly Mock<ICookieJar> _cookieJar = new();


    [SetUp]
    public void SetupMethod()
    {
        WebDriver.Reset();
        _webDriverOptions.Reset();

        _seleniumBrowser.WebDriver = WebDriver.Object;

        WebDriver.Setup(x => x.Manage()).Returns(_webDriverOptions.Object);
        WebDriver.Reset();
        _webDriverOptions.Reset();

        _seleniumBrowser.WebDriver = WebDriver.Object;

        WebDriver.Setup(x => x.Manage()).Returns(_webDriverOptions.Object);
        _webDriverOptions.Setup(x => x.Cookies).Returns(_cookieJar.Object);

    }

    [Test]
    public void TestExecute()
    {
        var action = new ClearBrowserCacheAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Build();

        action.Execute(Context);

        _webDriverOptions.Verify(x => x.Cookies.DeleteAllCookies(), Times.Once);
    }
}
