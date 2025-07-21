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

using System.Text.Json;
using Agenix.Playwright.Actions;
using Microsoft.Playwright;
using Moq;

namespace Agenix.Playwright.Tests.Actions;

[TestFixture]
public class ClearBrowserCacheActionTest : AbstractPlaywrightActionTestBase
{
    private Mock<ICDPSession> _cdpSession;

    protected override void CustomizeSetup()
    {
        // Setup CDP session for browser cache clearing
        _cdpSession = new Mock<ICDPSession>();

        // Setup the CDP session to return successfully
        _cdpSession.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<Dictionary<string, object>?>()))
            .ReturnsAsync((JsonElement?)null);

        // Setup browser context to provide CDP session
        BrowserContext.Setup(x => x.NewCDPSessionAsync(It.IsAny<IPage>()))
            .ReturnsAsync(_cdpSession.Object);

        // Setup page to return the browser context
        Page.Setup(x => x.Context).Returns(BrowserContext.Object);

        // Ensure the PlaywrightBrowser returns the current context properly
        PlaywrightBrowser.Setup(x => x.BrowserContext).Returns(BrowserContext.Object);
        PlaywrightBrowser.Setup(x => x.GetCurrentContext()).Returns(BrowserContext.Object);

        // Make sure there's at least one context available
        var contextIds = new List<string> { PlaywrightTestMockSetup.DefaultContextId }.AsReadOnly();
        PlaywrightBrowser.Setup(x => x.ContextIds).Returns(contextIds);
        PlaywrightBrowser.Setup(x => x.ContextCount).Returns(1);

        // Ensure there's a current context ID
        PlaywrightBrowser.Setup(x => x.CurrentContextId).Returns(PlaywrightTestMockSetup.DefaultContextId);
    }

    [Test]
    public void TestExecute_ShouldClearBrowserCache()
    {
        // Arrange
        var action = new ClearBrowserCacheAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        BrowserContext.Verify(x => x.NewCDPSessionAsync(It.IsAny<IPage>()), Times.Once);
        _cdpSession.Verify(x => x.SendAsync("Network.clearBrowserCache", null), Times.Once);
    }


    [Test]
    public void TestExecute_WhenPageNotFound_ShouldThrowException()
    {
        // Arrange
        const string nonExistentPageId = "non-existent-page";
        PlaywrightBrowser.Setup(x => x.GetPage(nonExistentPageId))
            .Throws(new InvalidOperationException($"Page '{nonExistentPageId}' not found"));

        var action = new ClearBrowserCacheAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithPageId(nonExistentPageId)
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => action.Execute(Context));
    }

    [Test]
    public void TestExecute_WhenContextNotFound_ShouldThrowException()
    {
        // Arrange
        const string nonExistentContextId = "non-existent-context";
        PlaywrightBrowser.Setup(x => x.GetContext(nonExistentContextId))
            .Throws(new InvalidOperationException($"Context '{nonExistentContextId}' not found"));

        var action = new ClearBrowserCacheAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithContextId(nonExistentContextId)
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => action.Execute(Context));
    }

    [Test]
    public void TestBuilder_WithDefaultConfiguration_ShouldCreateAction()
    {
        // Arrange & Act
        var action = new ClearBrowserCacheAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action, Is.InstanceOf<ClearBrowserCacheAction>());
    }

    [Test]
    public void TestBuilder_WithCustomPageId_ShouldCreateActionWithPageId()
    {
        // Arrange
        const string customPageId = "custom-page";

        // Act
        var action = new ClearBrowserCacheAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithPageId(customPageId)
            .Build();

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action, Is.InstanceOf<ClearBrowserCacheAction>());
    }

    [Test]
    public void TestBuilder_WithCustomContextId_ShouldCreateActionWithContextId()
    {
        // Arrange
        const string customContextId = "custom-context";

        // Act
        var action = new ClearBrowserCacheAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithContextId(customContextId)
            .Build();

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action, Is.InstanceOf<ClearBrowserCacheAction>());
    }

    [Test]
    public void TestBuilder_WithCreateNewPageIfNotFound_ShouldCreateAction()
    {
        // Arrange & Act
        var action = new ClearBrowserCacheAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithCreateNewPageIfNotFound()
            .Build();

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action, Is.InstanceOf<ClearBrowserCacheAction>());
    }

    [Test]
    public void TestBuilder_WithCreateNewContextIfNotFound_ShouldCreateAction()
    {
        // Arrange & Act
        var action = new ClearBrowserCacheAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithCreateNewContextIfNotFound()
            .Build();

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action, Is.InstanceOf<ClearBrowserCacheAction>());
    }

    [Test]
    public void TestExecute_MultipleCalls_ShouldClearCacheEachTime()
    {
        // Arrange
        var action = new ClearBrowserCacheAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        // Act
        action.Execute(Context);
        action.Execute(Context);
        action.Execute(Context);

        // Assert
        BrowserContext.Verify(x => x.NewCDPSessionAsync(It.IsAny<IPage>()), Times.Exactly(3));
        _cdpSession.Verify(x => x.SendAsync("Network.clearBrowserCache", null), Times.Exactly(3));
    }
}
