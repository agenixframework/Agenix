using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;

namespace Agenix.Playwright.Actions;

/// <summary>
///     Abstract base class for all Playwright actions.
///     Provides common functionality for executing Playwright browser commands and managing browser instances.
/// </summary>
public abstract class AbstractPlaywrightAction : AbstractTestAction, IPlaywrightAction
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(AbstractPlaywrightAction));

    /// <summary>
    ///     Playwright browser instance
    /// </summary>
    private readonly PlaywrightBrowser? _browser;

    /// <summary>
    ///     The specific context ID to use (optional)
    /// </summary>
    private readonly string? _contextId;

    /// <summary>
    ///     Whether to create a new context if contextId is not found
    /// </summary>
    private readonly bool _createNewContextIfNotFound;

    /// <summary>
    ///     Whether to create a new page if pageId is not found
    /// </summary>
    private readonly bool _createNewPageIfNotFound;

    /// <summary>
    ///     The specific page ID to use (optional)
    /// </summary>
    private readonly string? _pageId;

    /// <summary>
    ///     Initializes a new instance of the AbstractPlaywrightAction class
    /// </summary>
    /// <param name="builder">The builder instance containing configuration</param>
    protected AbstractPlaywrightAction(IPlaywrightActionBuilder<IPlaywrightAction> builder) : this(builder.GetName(),
        builder)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the AbstractPlaywrightAction class
    /// </summary>
    /// <param name="name">The action name</param>
    /// <param name="builder">The builder instance containing configuration</param>
    protected AbstractPlaywrightAction(string name, IPlaywrightActionBuilder<IPlaywrightAction> builder)
        : base("playwright:" + name, builder.GetDescription())
    {
        _browser = builder.Browser;
        _pageId = builder.PageId;
        _contextId = builder.ContextId;
        _createNewPageIfNotFound = builder.CreateNewPageIfNotFound;
        _createNewContextIfNotFound = builder.CreateNewContextIfNotFound;
    }

    /// <summary>
    ///     Gets the target page ID
    /// </summary>
    protected string? PageId => _pageId;

    /// <summary>
    ///     Gets the target context ID
    /// </summary>
    protected string? ContextId => _contextId;

    /// <summary>
    ///     Gets whether to create a new page if not found
    /// </summary>
    protected bool CreateNewPageIfNotFound => _createNewPageIfNotFound;

    /// <summary>
    ///     Gets whether to create a new context if not found
    /// </summary>
    protected bool CreateNewContextIfNotFound => _createNewContextIfNotFound;

    /// <summary>
    ///     Gets the Playwright browser instance
    /// </summary>
    public PlaywrightBrowser Browser =>
        _browser ?? throw new InvalidOperationException("Playwright browser not configured");

    /// <summary>
    ///     Executes the Playwright action within the provided test context
    /// </summary>
    /// <param name="context">The test context</param>
    public override void DoExecute(TestContext context)
    {
        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.LogDebug("Executing Playwright browser command: '{ActionName}' with description: '{Description}'",
                Name, GetDescription());
        }

        var browserToUse = _browser;
        if (browserToUse == null && context.GetVariables().ContainsKey(PlaywrightHeaders.PlaywrightBrowser))
        {
            // Try to resolve browser from context variables
            var browserReference = context.GetVariable(PlaywrightHeaders.PlaywrightBrowser);
            browserToUse = context.ReferenceResolver.Resolve<PlaywrightBrowser>(browserReference);
        }

        if (browserToUse == null)
        {
            Logger.LogError("Playwright browser not configured for action: '{ActionName}'. " +
                            "Either configure browser in builder or set context variable '{ContextKey}'",
                Name, PlaywrightHeaders.PlaywrightBrowser);
            throw new InvalidOperationException($"Playwright browser not configured for action: '{Name}'. " +
                                                $"Either configure browser in builder or set context variable '{PlaywrightHeaders.PlaywrightBrowser}'");
        }

        // Resolve target context and page
        var resolvedContext = ResolveTargetContext(browserToUse);
        var resolvedPage = ResolveTargetPage(browserToUse, resolvedContext);

        // Switch to the resolved context and page
        browserToUse.SwitchToContext(resolvedContext);
        browserToUse.SwitchToPage(resolvedPage);

        Execute(browserToUse, context).GetAwaiter().GetResult();

        Logger.LogInformation("Playwright browser command execution successful: '{ActionName}'", Name);
    }

    /// <summary>
    ///     Resolves the target context ID, creating one if necessary.
    /// </summary>
    /// <param name="browser">The Playwright browser instance.</param>
    /// <returns>The resolved context ID.</returns>
    protected virtual string ResolveTargetContext(PlaywrightBrowser browser)
    {
        // If a specific context ID is provided, use it
        if (!string.IsNullOrEmpty(_contextId))
        {
            if (browser.ContextIds.Contains(_contextId))
            {
                return _contextId;
            }

            if (_createNewContextIfNotFound)
            {
                Logger.LogDebug("Creating new context with ID: {ContextId}", _contextId);
                var newContextId = browser.CreateContextAsync(contextId: _contextId).Result;
                return newContextId;
            }

            throw new ArgumentException(
                $"Context with ID '{_contextId}' not found and createNewContextIfNotFound is false");
        }

        return browser.ContextIds.FirstOrDefault() ??
               throw new InvalidOperationException("No contexts available");
    }

    /// <summary>
    ///     Resolves the target page ID within a browser and context, creating one if necessary.
    /// </summary>
    /// <param name="browser">The Playwright browser instance managing the target context and pages.</param>
    /// <param name="contextId">The identifier of the target browser context.</param>
    /// <returns>The resolved page ID.</returns>
    protected virtual string ResolveTargetPage(PlaywrightBrowser browser, string contextId)
    {
        // If a specific page ID is provided, use it
        if (!string.IsNullOrEmpty(_pageId))
        {
            if (browser.PageIds.Contains(_pageId))
            {
                // Verify the page belongs to the target context
                var pagesInContext = browser.GetPagesInContext(contextId);
                if (pagesInContext.ContainsKey(_pageId))
                {
                    return _pageId;
                }

                throw new ArgumentException($"Page with ID '{_pageId}' exists but not in context '{contextId}'");
            }

            if (_createNewPageIfNotFound)
            {
                Logger.LogDebug("Creating new page with ID: {PageId} in context: {ContextId}", _pageId, contextId);
                var newPageId = browser.CreatePageAsync(contextId, _pageId).Result;
                return newPageId;
            }

            throw new ArgumentException($"Page with ID '{_pageId}' not found and createNewPageIfNotFound is false");
        }

        // Use the current page if it belongs to the target context
        var currentPage = browser.GetCurrentPage();
        if (currentPage != null)
        {
            var pagesInContext = browser.GetPagesInContext(contextId);
            // Find the page ID for the current page in the target context
            foreach (var pageEntry in pagesInContext)
            {
                if (pageEntry.Value == currentPage)
                {
                    return pageEntry.Key;
                }
            }
        }

        // Use the first page in the target context
        var firstPageInContext = browser.GetPagesInContext(contextId).FirstOrDefault();
        if (firstPageInContext.Key != null)
        {
            return firstPageInContext.Key;
        }

        // Create a new page in the target context
        Logger.LogDebug("Creating new page in context: {ContextId}", contextId);
        var createdPageId = browser.CreatePageAsync(contextId).Result;
        return createdPageId;
    }

    /// <summary>
    ///     Abstract method to be implemented by concrete Playwright actions
    /// </summary>
    /// <param name="browser">The Playwright browser instance to use</param>
    /// <param name="context">The test context</param>
    protected abstract Task Execute(PlaywrightBrowser browser, TestContext context);

    /// <summary>
    ///     Interface for Playwright action builders
    /// </summary>
    public interface IPlaywrightActionBuilder<out T> : ITestActionBuilder<T> where T : IPlaywrightAction
    {
        /// <summary>
        ///     Gets the Playwright browser instance
        /// </summary>
        PlaywrightBrowser? Browser { get; }

        /// <summary>
        ///     Gets the specific page ID to use (optional)
        /// </summary>
        string? PageId { get; }

        /// <summary>
        ///     Gets the specific context ID to use (optional)
        /// </summary>
        string? ContextId { get; }

        /// <summary>
        ///     Gets whether to create a new page if pageId is not found
        /// </summary>
        bool CreateNewPageIfNotFound { get; }

        /// <summary>
        ///     Gets whether to create a new context if contextId is not found
        /// </summary>
        bool CreateNewContextIfNotFound { get; }

        /// <summary>
        ///     Sets the Playwright browser instance
        /// </summary>
        /// <param name="playwrightBrowser">The Playwright browser instance</param>
        /// <returns>The builder instance for method chaining</returns>
        IPlaywrightActionBuilder<T> WithBrowser(PlaywrightBrowser playwrightBrowser);

        /// <summary>
        ///     Sets the specific page ID to use
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <returns>The builder instance for method chaining</returns>
        IPlaywrightActionBuilder<T> WithPageId(string pageId);

        /// <summary>
        ///     Sets the specific context ID to use
        /// </summary>
        /// <param name="contextId">The context ID</param>
        /// <returns>The builder instance for method chaining</returns>
        IPlaywrightActionBuilder<T> WithContextId(string contextId);

        /// <summary>
        ///     Sets whether to create a new page if the specified page ID is not found
        /// </summary>
        /// <param name="createNewPage">True to create a new page if not found</param>
        /// <returns>The builder instance for method chaining</returns>
        IPlaywrightActionBuilder<T> WithCreateNewPageIfNotFound(bool createNewPage = true);

        /// <summary>
        ///     Sets whether to create a new context if the specified context ID is not found
        /// </summary>
        /// <param name="createNewContext">True to create a new context if not found</param>
        /// <returns>The builder instance for method chaining</returns>
        IPlaywrightActionBuilder<T> WithCreateNewContextIfNotFound(bool createNewContext = true);

        /// <summary>
        ///     Gets the action name
        /// </summary>
        /// <returns>The action name</returns>
        string GetName();

        /// <summary>
        ///     Retrieves the description for the current action.
        /// </summary>
        /// <returns>
        ///     A string containing the description of the action.
        /// </returns>
        string GetDescription();
    }

    /// <summary>
    ///     Abstract base builder class for Playwright actions
    /// </summary>
    public abstract class Builder<T, TB> : AbstractTestActionBuilder<T, TB>, IPlaywrightActionBuilder<T>
        where T : IPlaywrightAction
        where TB : Builder<T, TB>
    {
        /// <summary>
        ///     The specific page ID to use (optional)
        /// </summary>
        protected string? PageIdValue { get; private set; }

        /// <summary>
        ///     The specific context ID to use (optional)
        /// </summary>
        protected string? ContextIdValue { get; private set; }

        /// <summary>
        ///     Whether to create a new page if pageId is not found
        /// </summary>
        protected bool CreateNewPageIfNotFoundValue { get; private set; }

        /// <summary>
        ///     Whether to create a new context if contextId is not found
        /// </summary>
        protected bool CreateNewContextIfNotFoundValue { get; private set; }

        /// <summary>
        ///     The browser instance to use for the action
        /// </summary>
        public PlaywrightBrowser? Browser { get; private set; }

        /// <summary>
        ///     Gets the specific page ID to use
        /// </summary>
        public string? PageId => PageIdValue;

        /// <summary>
        ///     Gets the specific context ID to use
        /// </summary>
        public string? ContextId => ContextIdValue;

        /// <summary>
        ///     Gets whether to create a new page if pageId is not found
        /// </summary>
        public bool CreateNewPageIfNotFound => CreateNewPageIfNotFoundValue;

        /// <summary>
        ///     Gets whether to create a new context if contextId is not found
        /// </summary>
        public bool CreateNewContextIfNotFound => CreateNewContextIfNotFoundValue;

        /// <summary>
        ///     Sets the specific page ID to use
        /// </summary>
        /// <param name="pageId">The page ID</param>
        /// <returns>The builder instance for method chaining</returns>
        public IPlaywrightActionBuilder<T> WithPageId(string pageId)
        {
            PageIdValue = pageId;
            return this;
        }

        /// <summary>
        ///     Sets the specific context ID to use
        /// </summary>
        /// <param name="contextId">The context ID</param>
        /// <returns>The builder instance for method chaining</returns>
        public IPlaywrightActionBuilder<T> WithContextId(string contextId)
        {
            ContextIdValue = contextId;
            return this;
        }

        /// <summary>
        ///     Sets whether to create a new page if the specified page ID is not found
        /// </summary>
        /// <param name="createNewPage">True to create a new page if not found</param>
        /// <returns>The builder instance for method chaining</returns>
        public IPlaywrightActionBuilder<T> WithCreateNewPageIfNotFound(bool createNewPage = true)
        {
            CreateNewPageIfNotFoundValue = createNewPage;
            return this;
        }

        /// <summary>
        ///     Sets whether to create a new context if the specified context ID is not found
        /// </summary>
        /// <param name="createNewContext">True to create a new context if not found</param>
        /// <returns>The builder instance for method chaining</returns>
        public IPlaywrightActionBuilder<T> WithCreateNewContextIfNotFound(bool createNewContext = true)
        {
            CreateNewContextIfNotFoundValue = createNewContext;
            return this;
        }

        // Explicit interface implementation to return the interface type
        /// <summary>
        ///     Specifies the browser to be used for the Playwright action.
        /// </summary>
        /// <param name="playwrightBrowser">An instance of the PlaywrightBrowser to be used.</param>
        /// <returns>An instance of IPlaywrightActionBuilder with the specified browser.</returns>
        IPlaywrightActionBuilder<T> IPlaywrightActionBuilder<T>.WithBrowser(PlaywrightBrowser playwrightBrowser)
        {
            return WithBrowser(playwrightBrowser);
        }

        /// <summary>
        ///     Builds an instance of the specified action type.
        /// </summary>
        /// <returns>
        ///     An instance of the corresponding action.
        /// </returns>
        public abstract override T Build();

        /// <summary>
        ///     Sets a custom Playwright browser for the action.
        /// </summary>
        /// <param name="playwrightBrowser">The Playwright browser instance to be used.</param>
        /// <returns>The current builder instance, enabling method chaining.</returns>
        public virtual TB WithBrowser(PlaywrightBrowser playwrightBrowser)
        {
            Browser = playwrightBrowser;
            return self;
        }

        /// <summary>
        ///     Sets the name of the action and returns the builder instance for further configuration.
        /// </summary>
        /// <param name="name">The name to be assigned to the action.</param>
        /// <returns>The updated builder instance.</returns>
        public override TB Name(string name)
        {
            base.Name(name);
            return self;
        }
    }
}
