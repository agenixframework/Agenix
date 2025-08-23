using Agenix.Playwright.Endpoint;
using Microsoft.Playwright;

namespace Agenix.Screenplay.Playwright.Abilities
{
    /// <summary>
    /// This ability wraps the Playwright Browser object for C# Screenplay pattern.
    /// Provides web browsing capabilities using Microsoft Playwright integrated with existing Agenix.Playwright actions.
    /// </summary>
    public sealed class BrowseTheWebWithPlaywright : IAbility
    {
        private readonly PlaywrightBrowserConfiguration _configuration;
        private PlaywrightBrowser? _playwrightBrowser;
        private Actor? _actor;

        private BrowseTheWebWithPlaywright(PlaywrightBrowserConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Gets the browsing ability for the specified actor.
        /// </summary>
        public static BrowseTheWebWithPlaywright As(Actor actor)
        {
            var ability = actor.AbilityTo<BrowseTheWebWithPlaywright>();
            if (ability == null)
            {
                throw new InvalidOperationException($"Actor {actor.Name} cannot browse the web with Playwright");
            }

            ability._actor = actor;
            return ability;
        }

        /// <summary>
        /// Creates a new Playwright browsing ability with custom configuration.
        /// </summary>
        public static BrowseTheWebWithPlaywright WithConfiguration(PlaywrightBrowserConfiguration configuration)
        {
            return new BrowseTheWebWithPlaywright(configuration);
        }

        /// <summary>
        /// Creates a new Playwright browsing ability using a browser builder for configuration.
        /// </summary>
        /// <returns>A new BrowseTheWebWithPlaywright instance configured via builder pattern</returns>
        public static BrowseTheWebWithPlaywright WithBrowserBuilder()
        {
            var builder = new PlaywrightBrowserBuilder();
            var configuration = builder.Build().EndpointConfiguration;
            return new BrowseTheWebWithPlaywright(configuration);
        }

        /// <summary>
        /// Creates a new Playwright browsing ability using a browser builder for configuration.
        /// </summary>
        /// <param name="configureBuilder">Action to configure the browser builder</param>
        /// <returns>A new BrowseTheWebWithPlaywright instance configured via builder pattern</returns>
        public static BrowseTheWebWithPlaywright WithBrowserBuilder(Action<PlaywrightBrowserBuilder> configureBuilder)
        {
            var builder = new PlaywrightBrowserBuilder();
            configureBuilder(builder);
            var configuration = builder.Build().EndpointConfiguration;
            return new BrowseTheWebWithPlaywright(configuration);
        }

        /// <summary>
        /// Gets the current Playwright browser instance from the existing Agenix.Playwright framework.
        /// </summary>
        public PlaywrightBrowser GetBrowser()
        {
            return _playwrightBrowser ??= new PlaywrightBrowser(_configuration);
        }

        /// <summary>
        /// Gets the current browser context.
        /// </summary>
        public IBrowserContext? GetCurrentContext()
        {
            return GetBrowser().GetCurrentContext();
        }

        /// <summary>
        /// Gets the current page.
        /// </summary>
        public IPage? GetCurrentPage()
        {
            return GetBrowser().GetCurrentPage();
        }

        /// <summary>
        /// Gets the configuration used by this ability.
        /// </summary>
        public PlaywrightBrowserConfiguration Configuration => _configuration;
    }
}
