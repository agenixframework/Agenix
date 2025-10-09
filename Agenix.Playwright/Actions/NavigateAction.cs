using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace Agenix.Playwright.Actions;

/// <summary>
///     Playwright action for navigating to a URL.
///     Supports navigation to specific pages and contexts, with support for multi-context and multi-page scenarios.
///     Navigates to a new page either by using a new absolute page URL or relative page path.
/// </summary>
public class NavigateAction : AbstractPlaywrightAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(NavigateAction));
    private readonly PageGotoOptions? _options;

    private  string _url;


    /// <summary>
    ///     Represents a Playwright action that facilitates navigation to a specified URL.
    /// </summary>
    /// <remarks>
    ///     This action allows for navigation to specific pages and contexts, with support for multi-context and multi-page
    ///     scenarios.
    ///     It enables navigating to a target page using either an absolute URL or a relative path.
    ///     It incorporates configurable navigation options and timeout settings.
    /// </remarks>
    /// <exception cref="ArgumentException">
    ///     Thrown when no URL is provided during the construction of the action.
    /// </exception>
    public NavigateAction(Builder builder) : this("navigate", builder) { }

    /// <summary>
    ///     Represents a Playwright action for navigating to a specified URL.
    /// </summary>
    /// <remarks>
    ///     This class facilitates navigation to a given page URL using Playwright, supporting
    ///     multi-context and multipage scenarios. It allows specifying navigation
    ///     options, such as timeout and wait conditions, to control the behavior.
    /// </remarks>
    /// <exception cref="ArgumentException">
    ///     Thrown when no URL is provided during the construction of the action.
    /// </exception>
    public NavigateAction(string name, Builder builder) : base(name, builder)
    {
        _url = builder.Url ?? throw new ArgumentException("URL is required", nameof(builder));
        _options = builder.Options;
    }

    /// <summary>
    ///     Executes the navigation process to a specified URL using Playwright.
    /// </summary>
    /// <param name="browser">
    ///     The instance of <see cref="PlaywrightBrowser" /> used to perform navigation actions.
    /// </param>
    /// <param name="context">
    ///     The current test context provided for managing test-specific configurations and actions.
    /// </param>
    /// <returns>
    ///     A task that represents the completion of the navigation process.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if an operation is performed on an invalid Playwright state, such as no active page or context.
    /// </exception>
    /// <exception cref="Exception">
    ///     Thrown if navigation fails for reasons including, but not limited to, invalid URL or underlying I/O errors.
    /// </exception>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        try
        {
            _url = context.ReplaceDynamicContentInString(_url);
            Logger.LogDebug("Navigating to URL: {Url}", _url);

            // Use the browser's async NavigateToAsync method
            await browser.NavigateToAsync(null, _url, _options);

            Logger.LogInformation("Successfully navigated to URL: {Url}", _url);
        }
        catch (Exception ex)
        {
            throw new AgenixSystemException($"Failed to navigate to URL: {_url}", ex);
        }
    }

    /// <summary>
    ///     Provides a builder pattern implementation for constructing instances of the <see cref="NavigateAction" /> class.
    ///     Enables chaining of configuration methods to set URL, navigation options, and other related parameters.
    /// </summary>
    public class Builder : Builder<NavigateAction, Builder>
    {
        /// <summary>
        ///     Gets the URL to which the browser should navigate. This property is set using the
        ///     <see cref="Builder.WithUrl(string)" /> method.
        ///     It represents an absolute or relative path to the desired page. The value cannot be null and must be defined before
        ///     building the action.
        /// </summary>
        public string? Url { get; private set; }

        /// <summary>
        ///     Gets or sets the navigation options for the action. This property allows configuring various parameters for page
        ///     navigation,
        ///     such as timeout, waiting conditions, and referer. It is defined using methods like
        ///     <see cref="Builder.WithTimeout(float)" />,
        ///     <see cref="Builder.WithWaitUntil(WaitUntilState)" />, and <see cref="Builder.WithReferer(string)" />.
        ///     If not provided, default navigation settings are applied.
        /// </summary>
        public PageGotoOptions? Options { get; private set; }

        /// <summary>
        ///     Sets the URL to which the browser should navigate.
        /// </summary>
        /// <param name="url">
        ///     The URL to navigate to. This must be a valid URL in absolute or relative format.
        ///     Throws <see cref="ArgumentNullException" /> if the provided URL is null.
        /// </param>
        /// <returns>
        ///     The current instance of the <see cref="Builder" /> to enable method chaining.
        /// </returns>
        public Builder WithUrl(string url)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            return this;
        }

        /// <summary>
        ///     Configures the navigation options for the <see cref="NavigateAction" />.
        /// </summary>
        /// <param name="options">
        ///     An instance of <see cref="PageGotoOptions" /> that specifies additional navigation options,
        ///     such as timeout, wait conditions, or referer.
        /// </param>
        /// <returns>
        ///     The current instance of the <see cref="Builder" /> to support method chaining.
        /// </returns>
        public Builder WithOptions(PageGotoOptions options)
        {
            Options = options;
            return this;
        }

        /// <summary>
        ///     Configures a timeout value for the navigation action.
        /// </summary>
        /// <param name="timeout">
        ///     The maximum time in milliseconds to wait for navigation to complete.
        ///     A value of 0 disables the timeout, and the default behavior will apply.
        /// </param>
        /// <returns>
        ///     The instance of the builder with the timeout configuration applied, enabling method chaining.
        /// </returns>
        public Builder WithTimeout(float timeout)
        {
            Options ??= new PageGotoOptions();
            Options.Timeout = timeout;
            return this;
        }

        /// <summary>
        ///     Configures the wait condition for navigation completion using the specified <see cref="WaitUntilState" /> value.
        /// </summary>
        /// <param name="waitUntil">
        ///     The wait condition that determines when the navigation is considered finished. Valid values include:
        ///     Load, DOMContentLoaded, NetworkIdle, or Commit.
        /// </param>
        /// <returns>
        ///     An instance of the builder with the specified wait condition applied.
        /// </returns>
        /// <exception cref="ArgumentException">
        ///     Thrown if an invalid <see cref="WaitUntilState" /> is provided.
        /// </exception>
        public Builder WithWaitUntil(WaitUntilState waitUntil)
        {
            Options ??= new PageGotoOptions();
            Options.WaitUntil = waitUntil;
            return this;
        }

        /// <summary>
        ///     Sets the referer URL within the navigation options for the <see cref="NavigateAction" />.
        /// </summary>
        /// <param name="referer">
        ///     The referer URL to be included in the navigation request. This parameter should represent
        ///     the source URL that the current navigation is referencing.
        /// </param>
        /// <returns>
        ///     The current instance of the builder, enabling method chaining for further configuration.
        /// </returns>
        public Builder WithReferer(string referer)
        {
            Options ??= new PageGotoOptions();
            Options.Referer = referer;
            return this;
        }

        /// <summary>
        ///     Constructs and returns a new instance of the NavigateAction class.
        /// </summary>
        /// <remarks>
        ///     This builder method is responsible for creating an instance of the NavigateAction class
        ///     with the configuration and parameters specified in the builder. It uses the Builder pattern
        ///     to allow fluent customization of the action before instantiation.
        /// </remarks>
        /// <returns>
        ///     A fully constructed instance of the NavigateAction class.
        /// </returns>
        public override NavigateAction Build()
        {
            return new NavigateAction(this);
        }
    }
}
