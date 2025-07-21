using Agenix.Api.Exceptions;
using Agenix.Playwright.Actions;
using Microsoft.Playwright;
using Moq;

namespace Agenix.Playwright.Tests.Actions;

public class DialogActionTest : AbstractPlaywrightActionTestBase
{
    private Mock<IDialog> _mockDialog;

    private Mock<IPage> _mockPage;

    [SetUp]
    public void SetUp()
    {
        _mockPage = new Mock<IPage>();
        _mockDialog = new Mock<IDialog>();

        // Override the default setup to ensure our mock page is returned
        MockSetup.ApplyDefaultSetup();

        // Ensure our mock page is returned by GetCurrentPage()
        PlaywrightBrowser.Setup(b => b.GetCurrentPage()).Returns(_mockPage.Object);

        // Setup the required browser context and page resolution
        PlaywrightBrowser.Setup(b => b.ContextIds).Returns(new[] { "default-context" });
        PlaywrightBrowser.Setup(b => b.PageIds).Returns(new[] { "default-page" });
        PlaywrightBrowser.Setup(b => b.GetPagesInContext("default-context"))
            .Returns(new Dictionary<string, IPage> { { "default-page", _mockPage.Object } });
        PlaywrightBrowser.Setup(b => b.SwitchToContext(It.IsAny<string>()));
        PlaywrightBrowser.Setup(b => b.SwitchToPage(It.IsAny<string>()));
    }

    [Test]
    public void Execute_AcceptDialog_ShouldSetupAndTeardownDialogHandler()
    {
        // Arrange
        var action = new DialogAction.Builder()
            .Accept("test input")
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        // Setup to verify dialog event handler registration and removal
        _mockPage.SetupAdd(p => p.Dialog += It.IsAny<EventHandler<IDialog>>());
        _mockPage.SetupRemove(p => p.Dialog -= It.IsAny<EventHandler<IDialog>>());

        // Act
        action.Execute(Context);

        // Assert - Verify event handler was registered and removed
        _mockPage.VerifyAdd(p => p.Dialog += It.IsAny<EventHandler<IDialog>>(), Times.Once);
        _mockPage.VerifyRemove(p => p.Dialog -= It.IsAny<EventHandler<IDialog>>(), Times.Once);
    }

    [Test]
    public void Execute_AcceptDialog_DialogHandlerShouldAcceptDialog()
    {
        // Arrange
        var action = new DialogAction.Builder()
            .Accept("test input")
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        _mockDialog.Setup(d => d.Message).Returns("Test dialog message");
        _mockDialog.Setup(d => d.Type).Returns("alert");
        _mockDialog.Setup(d => d.AcceptAsync("test input")).Returns(Task.CompletedTask);

        // Setup to capture dialog event handler
        EventHandler<IDialog> capturedHandler = null;
        _mockPage.SetupAdd(p => p.Dialog += It.IsAny<EventHandler<IDialog>>())
            .Callback<EventHandler<IDialog>>(handler => capturedHandler = handler);

        // Act
        action.Execute(Context);

        // Assert handler was captured
        Assert.That(capturedHandler, Is.Not.Null, "Dialog handler should have been registered");

        // Simulate dialog appearing by invoking the captured handler
        capturedHandler.Invoke(_mockPage.Object, _mockDialog.Object);

        // Give the Task.Run in the handler time to complete
        Task.Delay(500).GetAwaiter().GetResult();

        // Assert
        _mockDialog.Verify(d => d.AcceptAsync("test input"), Times.Once);
    }

    [Test]
    public void Execute_DismissDialog_DialogHandlerShouldDismissDialog()
    {
        // Arrange
        var action = new DialogAction.Builder()
            .Dismiss()
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        _mockDialog.Setup(d => d.Message).Returns("Test dialog message");
        _mockDialog.Setup(d => d.Type).Returns("confirm");
        _mockDialog.Setup(d => d.DismissAsync()).Returns(Task.CompletedTask);

        // Setup to capture dialog event handler
        EventHandler<IDialog> capturedHandler = null;
        _mockPage.SetupAdd(p => p.Dialog += It.IsAny<EventHandler<IDialog>>())
            .Callback<EventHandler<IDialog>>(handler => capturedHandler = handler);

        // Act
        action.Execute(Context);

        // Assert handler was captured
        Assert.That(capturedHandler, Is.Not.Null, "Dialog handler should have been registered");

        // Simulate dialog appearing
        capturedHandler.Invoke(_mockPage.Object, _mockDialog.Object);

        // Give the Task.Run in the handler time to complete
        Task.Delay(500).GetAwaiter().GetResult();

        // Assert
        _mockDialog.Verify(d => d.DismissAsync(), Times.Once);
    }

    [Test]
    public void Builder_Accept_ShouldSetCorrectActionType()
    {
        // Arrange & Act
        var builder = new DialogAction.Builder().Accept("test").Build();

        // Assert
        Assert.That(builder.ActionType, Is.EqualTo(DialogAction.DialogActionType.ACCEPT));
        Assert.That(builder.Text, Is.EqualTo("test"));
    }

    [Test]
    public void Builder_Dismiss_ShouldSetCorrectActionType()
    {
        // Arrange & Act
        var builder = new DialogAction.Builder().Dismiss().Build();

        // Assert
        Assert.That(builder.ActionType, Is.EqualTo(DialogAction.DialogActionType.DISMISS));
    }

    [Test]
    public void Builder_GetText_ShouldSetCorrectActionType()
    {
        // Arrange & Act
        var builder = new DialogAction.Builder().GetText().Build();

        // Assert
        Assert.That(builder.ActionType, Is.EqualTo(DialogAction.DialogActionType.GET_TEXT));
    }

    [Test]
    public void Builder_SendKeys_ShouldSetCorrectActionType()
    {
        // Arrange & Act
        var builder = new DialogAction.Builder().SendKeys("input text").Build();

        // Assert
        Assert.That(builder.ActionType, Is.EqualTo(DialogAction.DialogActionType.SEND_KEYS));
        Assert.That(builder.Text, Is.EqualTo("input text"));
    }

    [Test]
    public void Builder_WaitForDialog_ShouldSetWaitFlag()
    {
        // Arrange & Act
        var builder = new DialogAction.Builder().WaitForDialogToAppear().Build();

        // Assert
        Assert.That(builder.WaitForDialog, Is.True);
    }

    [Test]
    public void Builder_Timeout_ShouldSetTimeoutValue()
    {
        // Arrange & Act
        var builder = new DialogAction.Builder().WithTimeout(10000).Build();

        // Assert
        Assert.That(builder.TimeoutMs, Is.EqualTo(10000));
    }

    [Test]
    public void Execute_SendKeysDialog_DialogHandlerShouldAcceptWithText()
    {
        // Arrange
        var action = new DialogAction.Builder()
            .SendKeys("user input")
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        _mockDialog.Setup(d => d.Message).Returns("Test prompt message");
        _mockDialog.Setup(d => d.Type).Returns("prompt");
        _mockDialog.Setup(d => d.AcceptAsync("user input")).Returns(Task.CompletedTask);

        // Setup to capture dialog event handler
        EventHandler<IDialog> capturedHandler = null;
        _mockPage.SetupAdd(p => p.Dialog += It.IsAny<EventHandler<IDialog>>())
            .Callback<EventHandler<IDialog>>(handler => capturedHandler = handler);

        // Act
        action.Execute(Context);

        // Assert handler was captured
        Assert.That(capturedHandler, Is.Not.Null, "Dialog handler should have been registered");

        // Simulate dialog appearing
        capturedHandler.Invoke(_mockPage.Object, _mockDialog.Object);

        // Give the Task.Run in the handler time to complete
        Task.Delay(500).GetAwaiter().GetResult();

        // Assert
        _mockDialog.Verify(d => d.AcceptAsync("user input"), Times.Once);
    }

    [Test]
    public void Execute_GetTextDialog_DialogHandlerShouldCaptureTextAndDismiss()
    {
        // Arrange
        var action = new DialogAction.Builder()
            .GetText()
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        _mockDialog.Setup(d => d.Message).Returns("Dialog message text");
        _mockDialog.Setup(d => d.Type).Returns("alert");
        _mockDialog.Setup(d => d.DismissAsync()).Returns(Task.CompletedTask);

        // Setup to capture dialog event handler
        EventHandler<IDialog> capturedHandler = null;
        _mockPage.SetupAdd(p => p.Dialog += It.IsAny<EventHandler<IDialog>>())
            .Callback<EventHandler<IDialog>>(handler => capturedHandler = handler);

        // Act
        action.Execute(Context);

        // Assert handler was captured
        Assert.That(capturedHandler, Is.Not.Null, "Dialog handler should have been registered");

        // Simulate dialog appearing
        capturedHandler.Invoke(_mockPage.Object, _mockDialog.Object);

        // Give the Task.Run in the handler time to complete
        Task.Delay(500).GetAwaiter().GetResult();

        // Assert
        _mockDialog.Verify(d => d.DismissAsync(), Times.Once);
        Assert.That(Context.GetVariable("DIALOG_TEXT"), Is.EqualTo("Dialog message text"));
    }

    [Test]
    public void Execute_NoActivePage_ShouldThrowException()
    {
        // Arrange
        var action = new DialogAction.Builder()
            .Accept()
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        PlaywrightBrowser.Setup(b => b.GetCurrentPage()).Returns((IPage)null);

        // Act & Assert
        var exception = Assert.Throws<AgenixSystemException>(() => action.Execute(Context));
        Assert.That(exception.InnerException.Message, Is.EqualTo("No active page available"));
    }

    [Test]
    public void Execute_DialogTimeout_ShouldThrowTimeoutException()
    {
        // Arrange
        var action = new DialogAction.Builder()
            .Accept()
            .WaitForDialogToAppear()
            .WithTimeout(100) // Very short timeout
            .WithBrowser(PlaywrightBrowser.Object)
            .Build();

        // Act & Assert
        var exception = Assert.Throws<AgenixSystemException>(() => action.Execute(Context));

        Assert.That(exception.InnerException, Is.TypeOf<TimeoutException>());
        Assert.That(exception.InnerException.Message, Contains.Substring("Dialog did not appear within 100ms"));
    }
}
