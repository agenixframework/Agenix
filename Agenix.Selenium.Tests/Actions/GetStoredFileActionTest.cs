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

public class GetStoredFileActionTest : AbstractNUnitSetUp
{
    private readonly SeleniumBrowser _seleniumBrowser = new();
    private readonly Mock<IWebDriver> _webDriver = new();

    [SetUp]
    public void SetupMethod()
    {
        _webDriver.Reset();

        _seleniumBrowser.WebDriver = _webDriver.Object;
    }

    [Test]
    public void TestExecute()
    {
        _seleniumBrowser.StoreFile("download/file.txt");

        var action = new GetStoredFileAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetFileName("file.txt")
            .Build();

        action.Execute(Context);

        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumDownloadFile), Is.Not.Null);
        var content = File.ReadAllText(Context.GetVariable(SeleniumHeaders.SeleniumDownloadFile));

        Assert.That(content, Is.EqualTo(string.Empty));

    }

    [Test]
    public void TestExecuteError()
    {
        var action = new GetStoredFileAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .SetFileName("unknown.txt")
            .Build();

        var ex = Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
        Assert.That(ex.Message, Does.Match("Failed to retrieve file.*"));
    }
}
