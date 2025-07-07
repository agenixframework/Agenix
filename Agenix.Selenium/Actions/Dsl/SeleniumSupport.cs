using Agenix.Selenium.Endpoint;

namespace Agenix.Selenium.Actions.Dsl;

/// <summary>
/// Provides static support methods for initializing and working with Selenium-based test actions.
/// </summary>
public static class SeleniumSupport
{
    private static readonly object Lock = new();
    private static volatile SeleniumActionBuilder? _seleniumActionBuilder;
    private static volatile SeleniumBrowserBuilder? _seleniumBrowserBuilder;

    /// <summary>
    /// Provides a fluent entry point to create and configure Selenium-based test actions.
    /// </summary>
    /// <returns>
    /// A singleton instance of <see cref="SeleniumActionBuilder"/> for building Selenium test actions.
    /// </returns>
    public static SeleniumActionBuilder Selenium()
    {
        if (_seleniumActionBuilder == null)
        {
            lock (Lock)
            {
                _seleniumActionBuilder ??= SeleniumActionBuilder.Selenium();
            }
        }
        return _seleniumActionBuilder;
    }

    /// <summary>
    /// Creates a new Selenium browser configuration builder.
    /// </summary>
    /// <returns>A singleton instance of SeleniumBrowserBuilder for configuring browsers.</returns>
    public static SeleniumBrowserBuilder Browser()
    {
        if (_seleniumBrowserBuilder == null)
        {
            lock (Lock)
            {
                _seleniumBrowserBuilder ??= new SeleniumBrowserBuilder();
            }
        }
        return _seleniumBrowserBuilder;
    }

    /// <summary>
    /// Resets the singleton instances, forcing them to be recreated on next access.
    /// This is useful for testing scenarios or when you need fresh instances.
    /// </summary>
    public static void Reset()
    {
        lock (Lock)
        {
            _seleniumActionBuilder = null;
            _seleniumBrowserBuilder = null;
        }
    }
}

