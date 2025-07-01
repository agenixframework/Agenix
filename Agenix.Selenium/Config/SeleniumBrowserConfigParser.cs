using Agenix.Api.Config.Annotation;
using Agenix.Api.Spi;
using Agenix.Selenium.Endpoint;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.Events;

namespace Agenix.Selenium.Config;

/// <summary>
/// Parser for SeleniumBrowserConfig attribute to create SeleniumBrowser instances
/// </summary>
public class SeleniumBrowserConfigParser : IAnnotationConfigParser<SeleniumBrowserConfigAttribute, SeleniumBrowser>
{
    /// <summary>
    /// Parses the SeleniumBrowserConfig attribute and creates a configured SeleniumBrowser instance
    /// </summary>
    /// <param name="attribute">The SeleniumBrowserConfig attribute to parse</param>
    /// <param name="referenceResolver">Reference resolver for dependency injection</param>
    /// <returns>A configured SeleniumBrowser instance</returns>
    public SeleniumBrowser Parse(SeleniumBrowserConfigAttribute attribute, IReferenceResolver referenceResolver)
    {
        var builder = new SeleniumBrowserBuilder();

        if (!string.IsNullOrWhiteSpace(attribute.StartPage))
        {
            builder.StartPage(attribute.StartPage);
        }

        if (!string.IsNullOrWhiteSpace(attribute.Version))
        {
            builder.Version(attribute.Version);
        }

        if (!string.IsNullOrWhiteSpace(attribute.RemoteServer))
        {
            builder.RemoteServer(attribute.RemoteServer);
        }

        if (!string.IsNullOrWhiteSpace(attribute.Type))
        {
            builder.Type(attribute.Type);
        }

        if (!string.IsNullOrWhiteSpace(attribute.WebDriver))
        {
            builder.WebDriver(referenceResolver.Resolve<IWebDriver>(attribute.WebDriver));
        }

        if (!string.IsNullOrWhiteSpace(attribute.FirefoxProfile))
        {
            builder.Profile(referenceResolver.Resolve<FirefoxProfile>(attribute.FirefoxProfile));
        }

        if (attribute.EventListeners?.Length > 0)
        {
            var handlers = attribute.EventListeners
                .Select(referenceResolver.Resolve<Action<EventFiringWebDriver>>)
                .ToList();
            builder.EventHandlers(handlers);
        }


        builder.JavaScript(attribute.JavaScript);

        builder.Timeout(attribute.Timeout);

        return builder.Build();
    }

    /// <summary>
    /// Parses a SeleniumBrowserConfig attribute and creates a configured SeleniumBrowser instance.
    /// </summary>
    /// <param name="annotation">The SeleniumBrowserConfig attribute to parse.</param>
    /// <param name="referenceResolver">The reference resolver used for resolving dependencies during parsing.</param>
    /// <returns>A configured SeleniumBrowser instance.</returns>
    public object Parse(Attribute annotation, IReferenceResolver referenceResolver)
    {
        if (annotation is SeleniumBrowserConfigAttribute seleniumBrowserConfig)
        {
            return Parse(seleniumBrowserConfig, referenceResolver);
        }

        throw new ArgumentException($"Unsupported attribute type: {annotation.GetType().Name}. Expected {nameof(SeleniumBrowserConfigAttribute)}.");
    }
}
