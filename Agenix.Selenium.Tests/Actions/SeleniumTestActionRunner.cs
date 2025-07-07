using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Agenix.Api.Report;
using Agenix.Api.Spi;
using Agenix.Core;
using Agenix.Core.Container;
using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Agenix.Selenium.Util;
using Moq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions.Internal;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Selenium.Tests.Actions;

[TestFixture]
public class SeleniumTestActionBuilderTest : AbstractNUnitSetUp
{
    private SeleniumBrowser _seleniumBrowser;
    private readonly Mock<SeleniumBrowserConfiguration> _seleniumBrowserConfiguration = new();
    private readonly Mock<IWebDriver> _webDriver = new();
    private readonly Mock<IReferenceResolver> _referenceResolver = new();
    private readonly Mock<IWebElement> _element = new();
    private readonly Mock<IWebElement> _button = new();
    private readonly Mock<IWebElement> _link = new();
    private readonly Mock<IWebElement> _input = new();
    private readonly Mock<IWebElement> _checkbox = new();
    private readonly Mock<IWebElement> _hidden = new();
    private readonly Mock<IAlert> _alert = new();
    private readonly Mock<INavigation> _navigation = new();
    private readonly Mock<ITargetLocator> _locator = new();
    private readonly Mock<IOptions> _options = new();
    private readonly Mock<IOptions> _webDriverOptions = new();
    private readonly Mock<ICookieJar> _cookieJar = new();

    [Test]
    public void TestSeleniumBuilder()
    {
        _referenceResolver.Setup(x => x.Resolve<TestContext>()).Returns(Context);
        _referenceResolver.Setup(x => x.Resolve<TestActionListeners>()).Returns(new TestActionListeners());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceBeforeTest>())
            .Returns(new ConcurrentDictionary<string, SequenceBeforeTest>());
        _referenceResolver.Setup(x => x.ResolveAll<SequenceAfterTest>())
            .Returns(new ConcurrentDictionary<string, SequenceAfterTest>());

        _seleniumBrowser = new SeleniumBrowser(_seleniumBrowserConfiguration.Object);
        _webDriver.As<IJavaScriptExecutor>();
        _webDriver.As<IActionExecutor>();

        _webDriver.As<IJavaScriptExecutor>()
            .Setup(x => x.ExecuteScript(It.IsAny<string>()))
            .Returns(new List<string> { "This went wrong!" });

        _webDriver.Setup(x => x.Manage()).Returns(_webDriverOptions.Object);
        _webDriverOptions.Setup(x => x.Cookies).Returns(_cookieJar.Object);
        _cookieJar.Setup(x => x.DeleteAllCookies());
        _webDriver.Setup(x => x.SwitchTo()).Returns(_locator.Object);
        _webDriver.Setup(x => x.Navigate()).Returns(_navigation.Object);
        _locator.Setup(x => x.Window(It.IsAny<string>())).Returns(_webDriver.Object);

        // CRITICAL: Precise WindowHandles sequence for OpenWindowAction
        // The action calls WindowHandles exactly twice: before and after opening
        _webDriver.SetupSequence(x => x.WindowHandles)
            .Returns(new ReadOnlyCollection<string>(["last_window"]))               // 1st call - before opening
            .Returns(new ReadOnlyCollection<string>(["last_window", "new_window"])) // 2nd call - after opening
            .Returns(new ReadOnlyCollection<string>(["last_window", "new_window"])) // For subsequent actions
            .Returns(new ReadOnlyCollection<string>(["last_window", "new_window"])) // For switch window
            .Returns(new ReadOnlyCollection<string>(["last_window", "new_window"])) // For close window
            .Returns(new ReadOnlyCollection<string>(["last_window"]));              // After close

        _webDriver.Setup(x => x.CurrentWindowHandle).Returns("last_window");
        _seleniumBrowserConfiguration.Setup(x => x.BrowserType).Returns(BrowserType.CHROME.GetBrowserName());

        _seleniumBrowser.SetName("mockBrowser");
        _seleniumBrowser.WebDriver = _webDriver.Object;
        _referenceResolver.Setup(x => x.Resolve<SeleniumBrowser>("mockBrowser")).Returns(_seleniumBrowser);

        _locator.Setup(x => x.Alert()).Returns(_alert.Object);
        _alert.Setup(x => x.Text).Returns("Hello!");

        _webDriver.Setup(x => x.FindElement(By.Id("header"))).Returns(_element.Object);
        _element.Setup(x => x.TagName).Returns("h1");
        _element.Setup(x => x.Enabled).Returns(true);
        _element.Setup(x => x.Displayed).Returns(true);

        _webDriver.Setup(x => x.FindElement(By.LinkText("Click Me!"))).Returns(_link.Object);
        _link.Setup(x => x.TagName).Returns("a");
        _link.Setup(x => x.Enabled).Returns(true);
        _link.Setup(x => x.Displayed).Returns(true);

        _webDriver.Setup(x => x.FindElement(By.LinkText("Hover Me!"))).Returns(_link.Object);

        _webDriver.Setup(x => x.FindElement(By.Name("username"))).Returns(_input.Object);
        _input.Setup(x => x.TagName).Returns("input");
        _input.Setup(x => x.Enabled).Returns(true);
        _input.Setup(x => x.Displayed).Returns(true);

        _webDriver.Setup(x => x.FindElement(By.Name("hiddenButton"))).Returns(_hidden.Object);
        _hidden.Setup(x => x.TagName).Returns("input");
        _hidden.Setup(x => x.Enabled).Returns(true);
        _hidden.Setup(x => x.Displayed).Returns(false);

        _webDriver.Setup(x => x.FindElement(By.XPath("//input[@type='checkbox']"))).Returns(_checkbox.Object);
        _checkbox.Setup(x => x.TagName).Returns("input");
        _checkbox.Setup(x => x.Enabled).Returns(true);
        _checkbox.Setup(x => x.Displayed).Returns(true);
        _checkbox.Setup(x => x.Selected).Returns(false);


        _webDriver.Setup(x => x.FindElement(By.ClassName("btn"))).Returns(_button.Object);
        _button.Setup(x => x.TagName).Returns("button");
        _button.Setup(x => x.Enabled).Returns(false);
        _button.Setup(x => x.Displayed).Returns(false);

        // CRITICAL: For WaitUntil to work, we need to handle FindElements (plural) as well
        _webDriver.Setup(x => x.FindElements(By.Name("hiddenButton")))
            .Returns(new ReadOnlyCollection<IWebElement>([])); // Empty collection means element is not found/hidden

        _button.Setup(x => x.Text).Returns("Click Me!");
        _button.Setup(x => x.GetAttribute("type")).Returns("submit");
        _button.Setup(x => x.GetCssValue("color")).Returns("red");
        Context.SetVariable("cssClass", "btn");
        Context.SetVariable("my_window", "new_window"); // This is what SwitchWindow looks for!

        Context.SetReferenceResolver(_referenceResolver.Object);

        var builder = new DefaultTestCaseRunner(Context);

        builder.Run(SeleniumActionBuilder.Selenium().Start(_seleniumBrowser));
        builder.Run(SeleniumActionBuilder.Selenium().Navigate("http://localhost:9090"));
        builder.Run(SeleniumActionBuilder.Selenium().Find().Element(By.Id("header")));
        builder.Run(SeleniumActionBuilder.Selenium().Find().Element("class-name", "${cssClass}")
            .SetTagName("button")
            .SetEnabled(false)
            .SetDisplayed(false)
            .SetText("Click Me!")
            .SetStyle("color", "red")
            .SetAttribute("type", "submit"));

        builder.Run(SeleniumActionBuilder.Selenium().Click().Element(By.LinkText("Click Me!")));
        builder.Run(SeleniumActionBuilder.Selenium().Hover().Element(By.LinkText("Hover Me!")));
        builder.Run(SeleniumActionBuilder.Selenium().SetInput("Agenix").Element(By.Name("username")));
        builder.Run(SeleniumActionBuilder.Selenium().CheckInput(false).Element(By.XPath("//input[@type='checkbox']")));
        builder.Run(
            SeleniumActionBuilder.Selenium().JavaScript("alert('Hello!')").SetExpectedErrors("This went wrong!"));
        builder.Run(SeleniumActionBuilder.Selenium().Alert().SetText("Hello!").AcceptAlert());
        builder.Run(SeleniumActionBuilder.Selenium().ClearCache());
        builder.Run(SeleniumActionBuilder.Selenium().Store("download/file.txt"));
        builder.Run(SeleniumActionBuilder.Selenium().Open().SetWindow("my_window"));
        builder.Run(SeleniumActionBuilder.Selenium().Focus().Window("my_window"));
        builder.Run(SeleniumActionBuilder.Selenium().Close().SetWindow("my_window"));
        //        builder.Run(SeleniumActionBuilder.Selenium().WaitUntil().Hidden().Element(By.Name("hiddenButton")));
        builder.Run(SeleniumActionBuilder.Selenium().Stop(_seleniumBrowser));

        var test = builder.GetTestCase();
        var actionIndex = 0;

        Assert.That(test.GetActionCount(), Is.EqualTo(16));

        Assert.That(test.GetActions()[actionIndex], Is.TypeOf<StartBrowserAction>());
        var startBrowserAction = (StartBrowserAction)test.GetActions()[actionIndex++];
        Assert.That(startBrowserAction.Name, Is.EqualTo("selenium:start"));
        Assert.That(startBrowserAction.Browser, Is.Not.Null);

        Assert.That(test.GetActions()[actionIndex], Is.TypeOf<NavigateAction>());
        var navigateAction = (NavigateAction)test.GetActions()[actionIndex++];
        Assert.That(navigateAction.Name, Is.EqualTo("selenium:navigate"));
        Assert.That(navigateAction.Page, Is.EqualTo("http://localhost:9090"));

        Assert.That(test.GetActions()[actionIndex], Is.TypeOf<FindElementAction>());
        var findElementAction = (FindElementAction)test.GetActions()[actionIndex++];
        Assert.That(findElementAction.Name, Is.EqualTo("selenium:find"));
        Assert.That(findElementAction.By, Is.EqualTo(By.Id("header")));

        Assert.That(test.GetActions()[actionIndex], Is.TypeOf<FindElementAction>());
        findElementAction = (FindElementAction)test.GetActions()[actionIndex];
        Assert.That(findElementAction.Name, Is.EqualTo("selenium:find"));
        Assert.That(findElementAction.Property, Is.EqualTo("class-name"));
    }
}
