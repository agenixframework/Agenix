using System.Collections.ObjectModel;
using Agenix.Api.Exceptions;
using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

[TestFixture]
public class SwitchWindowActionTest : AbstractNUnitSetUp
{
    [SetUp]
    public void SetupMethod()
    {
        _webDriver = new Mock<IWebDriver>();
        _locator = new Mock<ITargetLocator>();
        _seleniumBrowser = new SeleniumBrowser();

        _seleniumBrowser.WebDriver = _webDriver.Object;

        _webDriver.Setup(x => x.SwitchTo()).Returns(_locator.Object);
    }

    [Test]
    public async Task TestSwitchToActiveWindow()
    {
        var windows = new ReadOnlyCollection<string>
        ([
                "active_window",
                "last_window",
                "other_window"
            ]
        );

        _webDriver.Setup(x => x.WindowHandles).Returns(windows);
        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("active_window");

        Context.SetVariable(SeleniumHeaders.SeleniumLastWindow, "last_window");
        Context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, "active_window");

        var action = new SwitchWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Build();

        await action.ExecuteAsync(Context);

        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumLastWindow), Is.EqualTo("last_window"));
        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumActiveWindow), Is.EqualTo("active_window"));

        _locator.Verify(x => x.Window(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task TestSwitchWindow()
    {
        var windows = new ReadOnlyCollection<string>
        (
            [
                "active_window",
                "other_window"
            ]
        );

        _webDriver.Setup(x => x.WindowHandles).Returns(windows);
        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("active_window");

        Context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, "active_window");
        Context.SetVariable("myWindow", "other_window");

        var action = new SwitchWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Window("myWindow")
            .Build();

        await action.ExecuteAsync(Context);

        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumLastWindow), Is.EqualTo("active_window"));
        Assert.That(Context.GetVariable(SeleniumHeaders.SeleniumActiveWindow), Is.EqualTo("other_window"));

        _locator.Verify(x => x.Window("other_window"), Times.Once);
    }

    [Test]
    public void TestSwitchWindowNotFound()
    {
        var windows = new ReadOnlyCollection<string>
        (
            ["active_window"]
        );

        _webDriver.Setup(x => x.WindowHandles).Returns(windows);
        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("active_window");

        Context.SetVariable(SeleniumHeaders.SeleniumActiveWindow, "active_window");
        Context.SetVariable("myWindow", "other_window");

        var action = new SwitchWindowAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Window("myWindow")
            .Build();

        var ex = Assert.ThrowsAsync<AgenixSystemException>(async () => await action.ExecuteAsync(Context));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Does.Match("Failed to find window.*"));
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private SeleniumBrowser _seleniumBrowser;
    private Mock<IWebDriver> _webDriver;
    private Mock<ITargetLocator> _locator;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}
