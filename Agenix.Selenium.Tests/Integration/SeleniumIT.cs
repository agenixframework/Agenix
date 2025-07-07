using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Spi;
using Agenix.Core.Container;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Selenium.Actions.Dsl;
using Agenix.Selenium.Config;
using Agenix.Selenium.Endpoint;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.Events;

namespace Agenix.Selenium.Tests.Integration;

[NUnitAgenixSupport]
public class SeleniumIT
{
    [AgenixResource] private ITestCaseRunner _testCaseRunner;

    [BindToRegistry] private EventFiringWebDriver _webDriver = CreateChromeDriver(CreateChromeOptions());

    [AgenixEndpoint]
    [SeleniumBrowserConfig(
        Type = "chrome",
        WebDriver = "_webDriver",
        StartPage = "https://the-internet.herokuapp.com/login"
    )]
    private SeleniumBrowser browser;


    /// <summary>
    ///     Creates Chrome options with comprehensive widget and automation interference disabling
    /// </summary>
    /// <param name="headless">Whether to run in headless mode</param>
    /// <returns>Configured ChromeOptions instance</returns>
    private static ChromeOptions CreateChromeOptions(bool headless = true)
    {
        var options = new ChromeOptions();

        // Headless mode (optional)
        if (headless)
        {
            options.AddArgument("--headless");
        }

        // Disable all widget-related features
        options.AddArgument("--disable-notifications");
        options.AddArgument("--disable-geolocation");
        options.AddArgument("--disable-media-stream");
        options.AddArgument("--use-fake-ui-for-media-stream");
        options.AddArgument("--disable-save-password-bubble");
        options.AddArgument("--disable-autofill-keyboard-accessory-view");
        options.AddArgument("--disable-autofill-assistant");
        options.AddArgument("--disable-translate");
        options.AddArgument("--disable-popup-blocking");
        options.AddArgument("--no-default-browser-check");
        options.AddArgument("--no-first-run");
        options.AddArgument("--disable-background-networking");
        options.AddArgument("--disable-background-timer-throttling");
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-dev-tools");
        options.AddArgument("--disable-infobars");
        options.AddArgument("--disable-web-security");
        options.AddArgument("--disable-features=TranslateUI,BlinkGenPropertyTrees");
        options.AddArgument("--disable-component-extensions-with-background-pages");
        options.AddArgument("--disable-default-apps");
        options.AddArgument("--disable-sync");
        options.AddArgument("--enable-automation");
        options.AddArgument("--disable-blink-features=AutomationControlled");

        // Additional automation-friendly arguments
        options.AddArgument("--disable-plugins");
        options.AddArgument("--disable-images");
        options.AddArgument("--disable-java");
        options.AddArgument("--disable-plugins-discovery");
        options.AddArgument("--disable-preconnect");
        options.AddArgument("--disable-dns-prefetch");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        // Set preferences to disable additional widgets
        options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 2);
        options.AddUserProfilePreference("profile.default_content_setting_values.geolocation", 2);
        options.AddUserProfilePreference("profile.default_content_setting_values.media_stream", 2);
        options.AddUserProfilePreference("profile.default_content_setting_values.media_stream_mic", 2);
        options.AddUserProfilePreference("profile.default_content_setting_values.media_stream_camera", 2);
        options.AddUserProfilePreference("profile.default_content_settings.popups", 0);
        options.AddUserProfilePreference("profile.managed_default_content_settings.images", 1);
        options.AddUserProfilePreference("profile.password_manager_enabled", false);
        options.AddUserProfilePreference("credentials_enable_service", false);
        options.AddUserProfilePreference("profile.password_manager_leak_detection", false);
        options.AddUserProfilePreference("autofill.profile_enabled", false);
        options.AddUserProfilePreference("autofill.credit_card_enabled", false);
        options.AddUserProfilePreference("translate.enabled", false);

        // Exclude automation switches
        options.AddExcludedArgument("enable-automation");

        return options;
    }


    private static EventFiringWebDriver CreateChromeDriver(ChromeOptions options)
    {
        var baseDriver = new ChromeDriver(options);
        var eventDriver = new EventFiringWebDriver(baseDriver);

        // Navigation events
        eventDriver.Navigating += (sender, e) =>
        {
            Console.WriteLine($"[ACTION] Navigating to: {e.Url}");
        };

        eventDriver.Navigated += (sender, e) =>
        {
            Console.WriteLine($"[SUCCESS] Navigated to: {e.Url}");
        };

        eventDriver.NavigatingBack += (sender, e) =>
        {
            Console.WriteLine("[ACTION] Navigating back");
        };

        eventDriver.NavigatedBack += (sender, e) =>
        {
            Console.WriteLine("[SUCCESS] Navigated back");
        };

        eventDriver.NavigatingForward += (sender, e) =>
        {
            Console.WriteLine("[ACTION] Navigating forward");
        };

        eventDriver.NavigatedForward += (sender, e) =>
        {
            Console.WriteLine("[SUCCESS] Navigated forward");
        };

        // Element finding events
        eventDriver.FindingElement += (sender, e) =>
        {
            Console.WriteLine($"[ACTION] Finding element: {e.FindMethod}");
        };

        eventDriver.FindElementCompleted += (sender, e) =>
        {
            try
            {
                if (e.Element != null)
                {
                    Console.WriteLine($"[SUCCESS] Found element: {e.FindMethod} -> {SafeGetElementInfo(e.Element)}");
                }
                else
                {
                    Console.WriteLine($"[WARNING] Element not found: {e.FindMethod} -> <null element>");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Found element: {e.FindMethod} -> <error: {ex.Message}>");
            }
        };

        // Element clicking events
        eventDriver.ElementClicking += (sender, e) =>
        {
            Console.WriteLine($"[ACTION] Clicking element: {SafeGetElementInfo(e.Element)}");
        };

        eventDriver.ElementClicked += (sender, e) =>
        {
            Console.WriteLine($"[SUCCESS] Clicked element: {SafeGetElementInfo(e.Element)}");
        };

        // Element value changing events
        eventDriver.ElementValueChanging += (sender, e) =>
        {
            Console.WriteLine($"[ACTION] Changing value of {SafeGetElementInfo(e.Element)} to: '{e.Value}'");
        };

        eventDriver.ElementValueChanged += (sender, e) =>
        {
            Console.WriteLine($"[SUCCESS] Changed value of {SafeGetElementInfo(e.Element)} to: '{e.Value}'");
        };

        // Script execution events
        eventDriver.ScriptExecuting += (sender, e) =>
        {
            var script = e.Script.Length > 100 ? e.Script.Substring(0, 100) + "..." : e.Script;
            Console.WriteLine($"[ACTION] Executing script: {script}");
        };

        eventDriver.ScriptExecuted += (sender, e) =>
        {
            var script = e.Script.Length > 100 ? e.Script.Substring(0, 100) + "..." : e.Script;
            Console.WriteLine($"[SUCCESS] Executed script: {script}");
        };

        // Exception event
        eventDriver.ExceptionThrown += (sender, e) =>
        {
            Console.WriteLine(
                $"[ERROR] Exception thrown: {e.ThrownException.GetType().Name}: {e.ThrownException.Message}");
        };

        return eventDriver;
    }

    private static string SafeGetElementInfo(IWebElement element)
    {
        if (element == null)
        {
            return "<null element>";
        }

        try
        {
            var tagName = element.TagName ?? "unknown";
            var id = element.GetAttribute("id");
            var className = element.GetAttribute("class");

            var identifier = !string.IsNullOrEmpty(id) ? $"#{id}" :
                !string.IsNullOrEmpty(className) ? $".{className.Split(' ')[0]}" :
                "no-identifier";

            return $"{tagName}[{identifier}]";
        }
        catch (StaleElementReferenceException)
        {
            return "<stale element>";
        }
        catch (Exception ex)
        {
            return $"<error: {ex.Message}>";
        }
    }


    [Test]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("SonarQube", "S2699:Tests should include assertions",
        Justification = "Test uses fluent API assertion through _testCaseRunner.Then() which verifies element text contains expected value")]
    public void Test_Login_Page()
    {
        _testCaseRunner.Given(FinallySequence.Builder.DoFinally()
            .Actions(SeleniumSupport.Selenium().Browser(browser).Stop()));
        _testCaseRunner.Given(SeleniumSupport.Selenium().Start(browser));

        _testCaseRunner.When(SeleniumSupport.Selenium().FillForm()
            .Field("username", "tomsmith")
            .Field("password", "SuperSecretPassword!")
            .Submit(By.CssSelector("button[type='submit'].radius"))
        );

        _testCaseRunner.Then(SeleniumSupport.Selenium()
            .Find()
            .Element(By.Id("flash"))
            .SetText("@Contains(You logged into a secure area!)@"));
    }
}
