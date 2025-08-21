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

using Agenix.Playwright.Actions;
using Microsoft.Playwright;
using Moq;

namespace Agenix.Playwright.Tests.Actions;

/// <summary>
///     Test class for CloseWindowAction functionality.
/// </summary>
[TestFixture]
public class CloseWindowActionTest : AbstractPlaywrightActionTestBase
{
    [Test]
    public async Task TestExecute_ShouldCloseCurrentPage()
    {
        // Arrange
        MockSetup.ApplyDefaultSetup();

        var action = new CloseWindowAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        // Act
        await action.ExecuteAsync(Context);

        // Assert
        Page.Verify(x => x.CloseAsync(It.IsAny<PageCloseOptions>()), Times.Once);
    }

    [Test]
    public async Task TestExecute_WithSpecificPageId_ShouldCloseSpecificPage()
    {
        // Arrange
        const string pageId = "specific-page";
        MockSetup.SetupCustomContextAndPage(PlaywrightTestMockSetup.DefaultContextId, pageId);

        var action = new CloseWindowAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithPageId(pageId)
            .Build();

        // Act
        await action.ExecuteAsync(Context);

        // Assert
        PlaywrightBrowser.Verify(x => x.SwitchToPage(pageId), Times.Once);
        Page.Verify(x => x.CloseAsync(It.IsAny<PageCloseOptions>()), Times.Once);
    }

    [Test]
    public async Task TestExecute_WithSpecificContextId_ShouldClosePageInContext()
    {
        // Arrange
        const string contextId = "specific-context";
        MockSetup.SetupCustomContextAndPage(contextId, PlaywrightTestMockSetup.DefaultPageId);

        var action = new CloseWindowAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithContextId(contextId)
            .Build();

        // Act
        await action.ExecuteAsync(Context);

        // Assert
        PlaywrightBrowser.Verify(x => x.SwitchToContext(contextId), Times.Once);
        Page.Verify(x => x.CloseAsync(It.IsAny<PageCloseOptions>()), Times.Once);
    }

    [Test]
    public async Task TestExecute_WithRunBeforeClose_ShouldExecuteBeforeClosing()
    {
        // Arrange
        MockSetup.ApplyDefaultSetup();

        var action = new CloseWindowAction.Builder()
            .WithRunBeforeUnload()
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        // Act
        await action.ExecuteAsync(Context);

        // Assert
        Page.Verify(x => x.CloseAsync(It.Is<PageCloseOptions>(opts => opts.RunBeforeUnload == true)), Times.Once);
    }

    [Test]
    public async Task TestExecute_WithoutRunBeforeClose_ShouldNotExecuteBeforeClosing()
    {
        // Arrange
        MockSetup.ApplyDefaultSetup();

        var action = new CloseWindowAction.Builder()
            .WithRunBeforeUnload(false)
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        // Act
        await action.ExecuteAsync(Context);

        // Assert
        Page.Verify(x => x.CloseAsync(It.Is<PageCloseOptions>(opts => opts.RunBeforeUnload == false)), Times.Once);
    }

    [Test]
    public void TestBuild_ShouldCreateCloseWindowAction()
    {
        // Arrange & Act
        var action = new CloseWindowAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action, Is.InstanceOf<CloseWindowAction>());
        Assert.That(action.Name, Does.Contain("close"));
    }
}
