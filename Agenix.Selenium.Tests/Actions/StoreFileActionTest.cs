using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

[TestFixture]
public class StoreFileActionTest : AbstractNUnitSetUp
{
    private SeleniumBrowser _seleniumBrowser;
    private Mock<IWebDriver> _webDriver;

    [SetUp]
    public void SetupMethod()
    {
        _webDriver = new Mock<IWebDriver>();
        _seleniumBrowser = new SeleniumBrowser();

        _seleniumBrowser.WebDriver = _webDriver.Object;
    }

    [Test]
    public void TestExecute()
    {
        var action = new StoreFileAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .FilePath("download/file.txt")
            .Build();

        action.Execute(Context);

        Assert.That(_seleniumBrowser.GetStoredFile("file.txt"), Is.Not.Null);
    }

    [Test]
    public void TestExecuteVariableSupport()
    {
        Context.SetVariable("file", "download/file.xml");

        var action = new StoreFileAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .FilePath("${file}")
            .Build();

        action.Execute(Context);

        Assert.That(_seleniumBrowser.GetStoredFile("file.xml"), Is.Not.Null);
    }
}
