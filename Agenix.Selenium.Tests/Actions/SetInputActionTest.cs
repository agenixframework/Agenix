using System.Collections.Generic;
using System.Collections.ObjectModel;
using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

[TestFixture]
public class SetInputActionTest : AbstractNUnitSetUp
{
    private SeleniumBrowser _seleniumBrowser = new();
    private Mock<IWebDriver> _webDriver = new();
    private Mock<IWebElement> _element = new();

    [SetUp]
    public void SetupMethod()
    {
        _webDriver.Reset();
        _element.Reset();

        _seleniumBrowser.WebDriver = _webDriver.Object;

        _element.Setup(x => x.Displayed).Returns(true);
        _element.Setup(x => x.Enabled).Returns(true);
        _element.Setup(x => x.TagName).Returns("input");
    }

    [Test]
    public void TestExecute()
    {
        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);

        var action = new SetInputAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("name", "textField")
            .SetValue("new_value")
            .Build();

        action.Execute(Context);

        _element.Verify(x => x.Clear(), Times.Once);
        _element.Verify(x => x.SendKeys("new_value"), Times.Once);
    }

    [Test]
    public void TestExecuteOnSelect()
    {
        var option = new Mock<IWebElement>();

        _webDriver.Setup(x => x.FindElement(It.IsAny<By>())).Returns(_element.Object);
        _element.Setup(x => x.TagName).Returns("select");

        _element.Setup(x => x.FindElements(It.IsAny<By>()))
            .Returns(new ReadOnlyCollection<IWebElement>(new List<IWebElement> { option.Object }));

        option.Setup(x => x.Enabled).Returns(true);
        option.Setup(x => x.Selected).Returns(false);

        var action = new SetInputAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .Element("name", "textField")
            .SetValue("option")
            .Build();

        action.Execute(Context);

        option.Verify(x => x.Click(), Times.Once);
    }
}
