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

using Agenix.Api.Exceptions;
using Agenix.Playwright.Actions;
using Microsoft.Playwright;
using Moq;

namespace Agenix.Playwright.Tests.Actions;

[TestFixture]
public class ClickActionTest : AbstractPlaywrightActionTestBase
{
    protected override void CustomizeSetup()
    {
        // Setup default clickable element
        SetupLocatorForBasicInteraction();
    }

    [Test]
    public void TestExecute_BasicClick_ShouldClickElement()
    {
        // Arrange
        var action = new ClickAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.ClickAsync(It.IsAny<LocatorClickOptions>()), Times.Once);
    }

    [Test]
    public void TestExecute_WithTimeout_ShouldClickWithTimeout()
    {
        // Arrange
        MockSetup.ApplyDefaultSetup();
        MockSetup.SetupLocatorForBasicInteraction();

        // Arrange
        var action = new ClickAction.Builder()
            .WithTimeout(5000)
            .WithTagName("button")
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.ClickAsync(It.Is<LocatorClickOptions>(opts =>
            opts.Timeout >= 5000 && opts.Timeout <= 5000)), Times.Once);
    }

    [Test]
    public void TestExecute_WithForce_ShouldClickWithForce()
    {
        // Arrange
        var action = new ClickAction.Builder()
            .WithForce()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.ClickAsync(It.Is<LocatorClickOptions>(opts =>
            opts.Force == true)), Times.Once);
    }

    [Test]
    public void TestExecute_WithPosition_ShouldClickAtPosition()
    {
        // Arrange
        var action = new ClickAction.Builder()
            .WithPosition(100, 200)
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.ClickAsync(It.Is<LocatorClickOptions>(opts =>
            opts.Position != null &&
            opts.Position.X >= 99.999f && opts.Position.X <= 100.001f &&
            opts.Position.Y >= 199.999f && opts.Position.Y <= 200.001f)), Times.Once);
    }

    [Test]
    public void TestExecute_WithButton_ShouldClickWithSpecificButton()
    {
        // Arrange
        var action = new ClickAction.Builder()
            .WithButton(MouseButton.Right)
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.ClickAsync(It.Is<LocatorClickOptions>(opts =>
            opts.Button == MouseButton.Right)), Times.Once);
    }

    [Test]
    public void TestExecute_WithClickCount_ShouldClickMultipleTimes()
    {
        // Arrange
        var action = new ClickAction.Builder()
            .WithClickCount(3)
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.ClickAsync(It.Is<LocatorClickOptions>(opts =>
            opts.ClickCount == 3)), Times.Once);
    }

    [Test]
    public void TestExecute_WithDelay_ShouldClickWithDelay()
    {
        // Arrange
        var action = new ClickAction.Builder()
            .WithDelay(1000)
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.ClickAsync(It.Is<LocatorClickOptions>(opts =>
            opts.Delay >= 999.999f && opts.Delay <= 1000.001f)), Times.Once);
    }

    [Test]
    public void TestExecute_WithModifiers_ShouldClickWithModifiers()
    {
        // Arrange
        var modifiers = new[] { KeyboardModifier.Control, KeyboardModifier.Shift };
        var action = new ClickAction.Builder()
            .WithModifiers(modifiers)
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.ClickAsync(It.Is<LocatorClickOptions>(opts =>
            opts.Modifiers != null && opts.Modifiers.Contains(KeyboardModifier.Control) &&
            opts.Modifiers.Contains(KeyboardModifier.Shift))), Times.Once);
    }

    [Test]
    public void TestExecute_WithTrial_ShouldClickWithTrial()
    {
        // Arrange
        var action = new ClickAction.Builder()
            .WithTrial()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.ClickAsync(It.Is<LocatorClickOptions>(opts =>
            opts.Trial == true)), Times.Once);
    }

    [Test]
    public void TestExecute_WithAllOptions_ShouldClickWithAllOptions()
    {
        // Arrange
        var modifiers = new[] { KeyboardModifier.Control };
        var action = new ClickAction.Builder()
            .WithTimeout(5000)
            .WithForce()
            .WithPosition(100, 200)
            .WithButton(MouseButton.Right)
            .WithClickCount(2)
            .WithDelay(500)
            .WithModifiers(modifiers)
            .WithTrial(false)
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.ClickAsync(It.Is<LocatorClickOptions>(opts =>
            opts.Timeout >= 5000 && opts.Timeout <= 5000 &&
            opts.Force == true &&
            opts.Position != null &&
            opts.Position.X >= 99.999f && opts.Position.X <= 100.001f &&
            opts.Position.Y >= 199.999f && opts.Position.Y <= 200.001f &&
            opts.Button == MouseButton.Right &&
            opts.ClickCount == 2 &&
            opts.Delay >= 499.999f && opts.Delay <= 500.001f &&
            opts.Modifiers != null && opts.Modifiers.Contains(KeyboardModifier.Control) &&
            opts.Trial == false)), Times.Once);
    }

    [Test]
    public void TestExecute_WithCustomPageId_ShouldClickOnSpecificPage()
    {
        // Arrange
        const string customPageId = "custom-page";
        MockSetup.SetupCustomContextAndPage(PlaywrightTestMockSetup.DefaultContextId, customPageId);
        MockSetup.SetupLocatorForBasicInteraction(); // Add this line

        var action = new ClickAction.Builder()
            .WithTagName("button")
            .WithBrowser(PlaywrightBrowser.Object)
            .WithPageId(customPageId)
            .Build();

        // Act
        action.Execute(Context);

        // Assert - Based on the actual invocation log, verify what's really being called
        PlaywrightBrowser.Verify(x => x.SwitchToPage(customPageId), Times.Once);
        PlaywrightBrowser.Verify(x => x.GetCurrentPage(), Times.AtLeastOnce);
        Locator.Verify(x => x.ClickAsync(It.IsAny<LocatorClickOptions>()), Times.Once);
    }

    [Test]
    public void TestExecute_WithCustomContextId_ShouldClickOnSpecificContext()
    {
        // Arrange
        const string customContextId = "custom-context";
        MockSetup.SetupCustomContextAndPage(customContextId, PlaywrightTestMockSetup.DefaultPageId);
        MockSetup.SetupLocatorForBasicInteraction(); // Add this line

        var action = new ClickAction.Builder()
            .WithTagName("button")
            .WithBrowser(PlaywrightBrowser.Object)
            .WithContextId(customContextId)
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        // Based on the invocation log, GetContext is not being called
        // Instead, verify the methods that are actually being called
        PlaywrightBrowser.Verify(x => x.SwitchToContext(customContextId), Times.Once);
        PlaywrightBrowser.Verify(x => x.GetPagesInContext(customContextId), Times.Once);
        Locator.Verify(x => x.ClickAsync(It.IsAny<LocatorClickOptions>()), Times.Once);
    }

    [Test]
    public void TestExecute_ElementNotVisible_ShouldThrowException()
    {
        // Arrange - Make the click operation actually fail
        var playwrightException = new PlaywrightException("Element is not visible");
        Locator.Setup(x => x.ClickAsync(It.IsAny<LocatorClickOptions>()))
            .ThrowsAsync(playwrightException);

        var action = new ClickAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act & Assert - Use Assert.Throws since Execute is synchronous
        var exception = Assert.Throws<AgenixSystemException>(() => action.Execute(Context));

        Assert.That(exception.Message, Is.EqualTo("Failed to locate element"));
        Assert.That(exception.InnerException.InnerException, Is.EqualTo(playwrightException));
    }


    [Test]
    public void TestExecute_ElementNotEnabled_ShouldStillClick()
    {
        // Arrange - Playwright allows clicking disabled elements by default
        SetupLocatorForBasicInteraction(true, false);

        var action = new ClickAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act - This should not throw an exception, Playwright allows clicking disabled elements
        action.Execute(Context);

        // Assert - Click should still be attempted
        Locator.Verify(x => x.ClickAsync(It.IsAny<LocatorClickOptions>()), Times.Once);
    }

    [Test]
    public void TestExecute_WhenClickFails_ShouldThrowException()
    {
        // Arrange
        var expectedException = new PlaywrightException("Click failed");
        Locator.Setup(x => x.ClickAsync(It.IsAny<LocatorClickOptions>()))
            .ThrowsAsync(expectedException);

        var action = new ClickAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act & Assert
        var exception =
            Assert.ThrowsAsync<AgenixSystemException>(async () => await Task.Run(() => action.Execute(Context)));

        Assert.That(exception.InnerException.InnerException.Message, Is.EqualTo("Click failed"));
    }

    [Test]
    public void TestExecute_WhenPageNotFound_ShouldThrowException()
    {
        // Arrange
        const string nonExistentPageId = "non-existent-page";
        PlaywrightBrowser.Setup(x => x.GetPage(nonExistentPageId))
            .Throws(new ArgumentException($"Page '{nonExistentPageId}' not found"));

        var action = new ClickAction.Builder()
            .WithTagName("button")
            .WithBrowser(PlaywrightBrowser.Object)
            .WithPageId(nonExistentPageId)
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => action.Execute(Context));
    }

    [Test]
    public void TestBuilder_WithoutBrowser_ShouldCreateActionButFailOnExecution()
    {
        // Arrange & Act - Builder should succeed without browser
        var action = new ClickAction.Builder()
            .WithTagName("button")
            .Build();

        // Assert - Action is created successfully
        Assert.That(action, Is.Not.Null);
        Assert.That(action, Is.InstanceOf<ClickAction>());

        // But execution should fail
        var exception = Assert.Throws<InvalidOperationException>(() => action.Execute(Context));
        Assert.That(exception.Message, Does.Contain("Playwright browser not configured"));
    }

    [Test]
    public void TestBuilder_WithoutTagName_ShouldThrowException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new ClickAction.Builder()
                .WithBrowser(PlaywrightBrowser.Object)
                .Build());
    }

    [Test]
    public void TestBuilder_WithDefaultConfiguration_ShouldCreateAction()
    {
        // Arrange & Act
        var action = new ClickAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action, Is.InstanceOf<ClickAction>());
    }

    [Test]
    public void TestBuilder_WithCreateNewPageIfNotFound_ShouldCreateAction()
    {
        // Arrange & Act
        var action = new ClickAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .WithCreateNewPageIfNotFound()
            .Build();

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action, Is.InstanceOf<ClickAction>());
    }

    [Test]
    public void TestBuilder_WithCreateNewContextIfNotFound_ShouldCreateAction()
    {
        // Arrange & Act
        var action = new ClickAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .WithCreateNewContextIfNotFound()
            .Build();

        // Assert
        Assert.That(action, Is.Not.Null);
        Assert.That(action, Is.InstanceOf<ClickAction>());
    }

    [Test]
    public void TestExecute_MultipleCalls_ShouldClickEachTime()
    {
        // Arrange
        var action = new ClickAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        // Act
        action.Execute(Context);
        action.Execute(Context);
        action.Execute(Context);

        // Assert
        Locator.Verify(x => x.ClickAsync(It.IsAny<LocatorClickOptions>()), Times.Exactly(3));
    }

    [Test]
    public void TestExecute_WithDifferentSelectors_ShouldClickDifferentElements()
    {
        // Arrange
        var locator1 = new Mock<ILocator>();
        var locator2 = new Mock<ILocator>();

        locator1.Setup(x => x.ClickAsync(It.IsAny<LocatorClickOptions>())).Returns(Task.CompletedTask);
        locator2.Setup(x => x.ClickAsync(It.IsAny<LocatorClickOptions>())).Returns(Task.CompletedTask);

        Page.Setup(x => x.Locator("button", It.IsAny<PageLocatorOptions>())).Returns(locator1.Object);
        Page.Setup(x => x.Locator("input", It.IsAny<PageLocatorOptions>())).Returns(locator2.Object);

        var action1 = new ClickAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("button")
            .Build();

        var action2 = new ClickAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName("input")
            .Build();

        // Act
        action1.Execute(Context);
        action2.Execute(Context);

        // Assert
        locator1.Verify(x => x.ClickAsync(It.IsAny<LocatorClickOptions>()), Times.Once);
        locator2.Verify(x => x.ClickAsync(It.IsAny<LocatorClickOptions>()), Times.Once);
    }

    [Test]
    public void TestExecute_WithComplexSelector_ShouldClickElement()
    {
        // Arrange
        const string complexSelector = "button[data-testid='submit-btn']:nth-child(2)";
        var action = new ClickAction.Builder()
            .WithBrowser(PlaywrightBrowser.Object)
            .WithTagName(complexSelector)
            .Build();

        // Act
        action.Execute(Context);

        // Assert
        Page.Verify(x => x.Locator(complexSelector, It.IsAny<PageLocatorOptions>()), Times.Once);
        Locator.Verify(x => x.ClickAsync(It.IsAny<LocatorClickOptions>()), Times.Once);
    }
}
