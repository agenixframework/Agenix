using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Playwright.Actions.Dsl;
using Agenix.Playwright.Config;
using Agenix.Playwright.Endpoint;

namespace Agenix.Playwright.Tests.Integration;

[NUnitAgenixSupport]
[NonParallelizable]
public class TracingActionIT
{
    [AgenixResource] private IAsyncTestCaseRunner _gherkin;

    [AgenixEndpoint(Name = "tracing-browser")]
    [PlaywrightBrowserConfig(
        Type = "chromium",
        StartPage = "https://the-internet.herokuapp.com",
        Headless = true
    )]
    private PlaywrightBrowser browser;

    [SetUp]
    public async Task BeforeTestMethod()
    {
        # region Set Environment Variables

        // Set PWDEBUG environment variable for Playwright debugging
        Environment.SetEnvironmentVariable("PWDEBUG", AgenixSettings.GetProperty("PWDEBUG"));

        # endregion

        await _gherkin.Given(PlaywrightSupport.Playwright().Start(browser));
    }

    [TearDown]
    public async Task AfterTestMethod()
    {
        await _gherkin.Then(PlaywrightSupport.Playwright().Stop(browser));
    }

    [Test]
    public async Task TracingStartAndStopTest()
    {
        // Start tracing at the beginning of the test
        await _gherkin.Given(PlaywrightSupport.Playwright().Tracing()
            .Start()
            .WithName("Test_Login_Page")
            .WithTitle("Login Page Test Trace")
            .WithScreenshots()
            .WithSnapshots()
            .WithSources()
            .Description("Start tracing for login page test"));

        // Stop tracing and save the trace file
        Assert.DoesNotThrowAsync(async () =>
        {
            await _gherkin.Then(PlaywrightSupport.Playwright().Tracing()
                .Stop("traces/Test_Login_Page.zip")
                .Description("Stop tracing and save trace file"));
        });
    }
}
