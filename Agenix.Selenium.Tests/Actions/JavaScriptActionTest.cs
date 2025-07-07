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

using Agenix.Api.Exceptions;
using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

public class JavaScriptActionTest : AbstractNUnitSetUp
{
    private readonly SeleniumBrowser _seleniumBrowser = new();
    private readonly Mock<IWebDriver> _webDriver = new();

    [SetUp]
    public void SetupMethod()
    {
        _webDriver.Reset();

        // Configure the WebDriver to also implement IJavaScriptExecutor
        _webDriver.As<IJavaScriptExecutor>();

        _seleniumBrowser.WebDriver = _webDriver.Object;
    }

    [Test]
    public void TestExecute()
    {
        _webDriver.As<IJavaScriptExecutor>()
                 .Setup(x => x.ExecuteScript("return window._selenide_jsErrors"))
                 .Returns(new List<object>());

        var action = new JavaScriptAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetScript("alert('Hello')")
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariableObject(SeleniumHeaders.SeleniumJsErrors), Is.Not.Null);
        Assert.That(((List<string>)Context.GetVariableObject(SeleniumHeaders.SeleniumJsErrors)).Count, Is.EqualTo(0));

        _webDriver.As<IJavaScriptExecutor>().Verify(x => x.ExecuteScript("alert('Hello')"), Times.Once);
    }

    [Test]
    public void TestExecuteVariableSupport()
    {
        _webDriver.As<IJavaScriptExecutor>()
                 .Setup(x => x.ExecuteScript("return window._selenide_jsErrors"))
                 .Returns(new List<object>());

        Context.SetVariable("text", "Hello");

        var action = new JavaScriptAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetScript("alert('${text}')")
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariableObject(SeleniumHeaders.SeleniumJsErrors), Is.Not.Null);
        Assert.That(((List<string>)Context.GetVariableObject(SeleniumHeaders.SeleniumJsErrors)).Count, Is.EqualTo(0));

        _webDriver.As<IJavaScriptExecutor>().Verify(x => x.ExecuteScript("alert('Hello')"), Times.Once);
    }

    [Test]
    public void TestExecuteWithErrorValidation()
    {
        _webDriver.As<IJavaScriptExecutor>()
                 .Setup(x => x.ExecuteScript("return window._selenide_jsErrors"))
                 .Returns(new List<string> { "This went totally wrong!" });

        var action = new JavaScriptAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetScript("alert('Hello')")
            .AddExpectedError("This went totally wrong!")
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariableObject(SeleniumHeaders.SeleniumJsErrors), Is.Not.Null);
        Assert.That(((List<string>)Context.GetVariableObject(SeleniumHeaders.SeleniumJsErrors)).Count, Is.EqualTo(1));

        _webDriver.As<IJavaScriptExecutor>().Verify(x => x.ExecuteScript("alert('Hello')"), Times.Once);
    }

    [Test]
    public void TestExecuteWithErrorValidationFailed()
    {
        _webDriver.As<IJavaScriptExecutor>()
                 .Setup(x => x.ExecuteScript("return window._selenide_jsErrors"))
                 .Returns(new List<string>());

        var action = new JavaScriptAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetScript("alert('Hello')")
            .SetExpectedErrors("This went totally wrong!")
            .Build();

        Assert.Throws<ValidationException>(() => action.Execute(Context));
    }
}
