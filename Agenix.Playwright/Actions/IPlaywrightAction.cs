using Agenix.Api;
using Agenix.Playwright.Endpoint;

namespace Agenix.Playwright.Actions;

/// <summary>
///     Represents a Playwright-related test action interface that extends
///     the <see cref="ITestAction" /> interface and provides browser automation capabilities.
/// </summary>
public interface IPlaywrightAction : IAsyncTestAction
{
    /// <summary>
    ///     Gets the Playwright browser.
    /// </summary>
    /// <returns>The PlaywrightBrowser instance</returns>
    PlaywrightBrowser Browser { get; }
}
