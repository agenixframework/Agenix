using System.Diagnostics.CodeAnalysis;
using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Core.Actions;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Playwright.Actions;
using Agenix.Playwright.Actions.Dsl;
using Agenix.Playwright.Config;
using Agenix.Playwright.Endpoint;

namespace Agenix.Playwright.Tests.Integration;

[NUnitAgenixSupport]
[NonParallelizable]
public class PlaywrightIT
{
    [AgenixResource] private ITestCaseRunner _gherkin;

    [AgenixEndpoint(Name = "playwright-browser")]
    [PlaywrightBrowserConfig(
        Type = "chromium",
        StartPage = "https://the-internet.herokuapp.com",
        Headless = true,
        HttpCredentialsUsername = "admin",
        HttpCredentialsPassword = "admin"
    )]
    private PlaywrightBrowser browser;

    [SetUp]
    public void BeforeTestMethod()
    {
        # region Set Environment Variables

        // Set PWDEBUG environment variable for Playwright debugging
        Environment.SetEnvironmentVariable("PWDEBUG", AgenixSettings.GetProperty("PWDEBUG"));

        # endregion

        _gherkin.Given(PlaywrightSupport.Playwright().Start(browser));
    }

    [TearDown]
    public void AfterTestMethod()
    {
        _gherkin.Then(PlaywrightSupport.Playwright().Stop(browser));
    }

    [Test]
    [SuppressMessage("SonarQube", "S2699:Tests should include assertions",
        Justification =
            "Test uses fluent API assertion through _testCaseRunner.Then() which verifies element text contains expected value")]
    public void Test_Login_Page()
    {
        _gherkin.Given(CreateVariablesAction.Builder.CreateVariable("username", "tomsmith"));

        _gherkin.Given(PlaywrightSupport.Playwright().Click()
            .Description("Click on Form Authentification link")
            .WithCss("a[href='/login']"));

        // Fill the login form
        _gherkin.When(PlaywrightSupport.Playwright().FillForm()
            .Description("Fill Login Form")
            .WithFieldByName("username", "${username}")
            .WithFieldByName("password", "SuperSecretPassword!")
            .WithSubmitButtonByCss("button[type='submit'].radius")
        );

        // Verify the success message
        _gherkin.Then(PlaywrightSupport.Playwright().ExpectLocator()
            .WithId("flash")
            .ToContainText("You logged into a secure area!")
            .Description("Verify success message")
        );
    }

    [Test]
    [SuppressMessage("SonarQube", "S2699:Tests should include assertions",
        Justification =
            "Test uses fluent API assertion through _testCaseRunner.Then() which verifies element text contains expected value")]
    public void Test_Dropdown_Selection()
    {
        // Navigate to the dropdown page
        _gherkin.Given(PlaywrightSupport.Playwright().Click()
            .Description("Click on Dropdown link")
            .WithCss("a[href='/dropdown']"));

        // Select Option 1 from the dropdown
        _gherkin.When(PlaywrightSupport.Playwright().Select()
            .SelectByLabel("Option 1")
            .WithId("dropdown")
            .Description("Select Option 1 from dropdown"));

        // Verify Option 1 is selected
        _gherkin.Then(PlaywrightSupport.Playwright().ExpectLocator()
            .ToHaveValue("1")
            .WithId("dropdown")
            .Description("Verify Option 1 is selected"));

        // Select Option 2 from the dropdown
        _gherkin.When(PlaywrightSupport.Playwright().Select()
            .SelectByLabel("Option 2")
            .WithId("dropdown")
            .Description("Select Option 2 from dropdown")
        );

        // Verify Option 2 is selected
        _gherkin.Then(PlaywrightSupport.Playwright().ExpectLocator()
            .ToHaveValue("2")
            .WithId("dropdown")
            .Description("Verify Option 2 is selected"));
    }

    [Test]
    [SuppressMessage("SonarQube", "S2699:Tests should include assertions",
        Justification =
            "Test uses fluent API assertion through _testCaseRunner.Then() which verifies element text contains expected value")]
    public void Test_Add_Remove_Elements()
    {
        // Navigate to the Add/Remove Elements page
        _gherkin.Given(PlaywrightSupport.Playwright().Click()
            .Description("Click on Add/Remove Elements link")
            .WithCss("a[href='/add_remove_elements/']"));

        // Verify the initial state - no delete buttons should be present
        _gherkin.Then(PlaywrightSupport.Playwright().ExpectLocator()
            .ToHaveCount(0)
            .WithCss("button.added-manually")
            .Description("Verify no delete buttons are initially present"));

        // Click the "Add Element" button to add the first element
        _gherkin.When(PlaywrightSupport.Playwright().Click()
            .Description("Click Add Element button to add first element")
            .WithCss("button[onclick='addElement()']"));

        // Verify that one delete button is now present
        _gherkin.Then(PlaywrightSupport.Playwright().ExpectLocator()
            .WithCss("button.added-manually")
            .ToHaveCount(1)
            .Description("Verify one delete button is present after adding first element"));

        // Add a second element
        _gherkin.When(PlaywrightSupport.Playwright().Click()
            .Description("Click Add Element button to add second element")
            .WithCss("button[onclick='addElement()']"));

        // Verify that two delete buttons are now present
        _gherkin.Then(PlaywrightSupport.Playwright().ExpectLocator()
            .ToHaveCount(2)
            .WithCss("button.added-manually")
            .Description("Verify two delete buttons are present after adding second element"));

        // Add a third element
        _gherkin.When(PlaywrightSupport.Playwright().Click()
            .Description("Click Add Element button to add third element")
            .WithCss("button[onclick='addElement()']"));

        // Verify that three delete buttons are now present
        _gherkin.Then(PlaywrightSupport.Playwright().ExpectLocator()
            .ToHaveCount(3)
            .WithCss("button.added-manually")
            .Description("Verify three delete buttons are present after adding third element"));

        // Click the first delete button to remove one element (using first-child selector)
        _gherkin.When(PlaywrightSupport.Playwright().Click()
            .Description("Click first delete button to remove one element")
            .WithCss("button.added-manually:first-child"));

        // Verify that two delete buttons remain
        _gherkin.Then(PlaywrightSupport.Playwright().ExpectLocator()
            .ToHaveCount(2)
            .WithCss("button.added-manually")
            .Description("Verify two delete buttons remain after removing one element"));

        // Click another delete button (always click the first available one)
        _gherkin.When(PlaywrightSupport.Playwright().Click()
            .Description("Click another delete button")
            .WithCss("button.added-manually:first-child"));

        // Verify that one delete button remains
        _gherkin.Then(PlaywrightSupport.Playwright().ExpectLocator()
            .ToHaveCount(1)
            .WithCss("button.added-manually")
            .Description("Verify one delete button remains"));

        // Click the last delete button
        _gherkin.When(PlaywrightSupport.Playwright().Click()
            .Description("Click the last delete button")
            .WithCss("button.added-manually"));

        // Verify that no deletes buttons remain
        _gherkin.Then(PlaywrightSupport.Playwright().ExpectLocator()
            .ToHaveCount(0)
            .WithCss("button.added-manually")
            .Description("Verify no delete buttons remain after removing all elements"));
    }

    [Test]
    public void TestBasicAuthentication()
    {
        // The browser will automatically handle basic auth with the configured credentials
        _gherkin.Given(PlaywrightSupport.Playwright().Navigate()
            .WithUrl("https://the-internet.herokuapp.com/basic_auth"));

        // Verify authentication was successful
        _gherkin.Then(PlaywrightSupport.Playwright().ExpectMultipleLocators()
            .AddExpectation(builder => builder
                .WithText("Congratulations! You must have the proper credentials.")
                .ToBeVisible())
            .AddExpectation(builder => builder
                .ToContainText("Basic Auth")
                .WithCss("h3"))
            .AddExpectation(builder => builder
                .ToContainText("Congratulations!")
                .WithCss("p"))
            .WithExecutionMode(ExpectMultipleLocatorsAction.ExecutionMode.PARALLEL)
        );
    }
}
