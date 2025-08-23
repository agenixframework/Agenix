using Agenix.Selenium.Actions;
using Agenix.Selenium.Endpoint;
using Moq;
using OpenQA.Selenium;

namespace Agenix.Selenium.Tests.Actions;

[TestFixture]
public class StoreFileActionTest : AbstractNUnitSetUp
{
    [SetUp]
    public void SetupMethod()
    {
        _webDriver = new Mock<IWebDriver>();
        _seleniumBrowser = new SeleniumBrowser();

        _seleniumBrowser.WebDriver = _webDriver.Object;
    }

    [Test]
    public async Task TestExecute()
    {
        var action = new StoreFileAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .FilePath("download/file.txt")
            .Build();

        await action.ExecuteAsync(Context);

        Assert.That(_seleniumBrowser.GetStoredFile("file.txt"), Is.Not.Null);
    }

    [Test]
    public async Task TestExecuteVariableSupport()
    {
        Context.SetVariable("file", "download/file.xml");

        var action = new StoreFileAction.Builder()
            .WithBrowser(_seleniumBrowser)
            .FilePath("${file}")
            .Build();

        await action.ExecuteAsync(Context);

        Assert.That(_seleniumBrowser.GetStoredFile("file.xml"), Is.Not.Null);
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private SeleniumBrowser _seleniumBrowser;
    private Mock<IWebDriver> _webDriver;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}
