using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using static Microsoft.Playwright.Assertions;


namespace Agenix.Playwright.Actions;

/// <summary>
///     Represents an action to validate the state of a web page, including URL, title, and locator-based conditions,
///     during the execution of Playwright-based tests.
/// </summary>
/// <remarks>
///     The <see cref="ExpectPageStateAction" /> facilitates defining and executing expectations for both page-wide and
///     element-specific states.
///     It is typically used in scenarios where precise verification of the page's conditions is crucial for asserting the
///     test's correctness.
/// </remarks>
public class ExpectPageStateAction : AbstractPlaywrightAction
{
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(ExpectPageStateAction));
    private readonly bool _continueOnFailure;

    private readonly ExpectMultipleLocatorsAction _locatorExpectations;
    private readonly List<Func<IPage, TestContext, Task>> _pageExpectations;

    /// <summary>
    ///     Represents an action to verify the state of a web page during a test, including various checks at both the page and
    ///     locator levels.
    /// </summary>
    /// <remarks>
    ///     This action ensures that the defined conditions for the page state, such as URL, title, and other element-based
    ///     expectations, are validated as part of the test execution.
    /// </remarks>
    public ExpectPageStateAction(Builder builder) : this("expect-page-state", builder)
    {
    }

    /// <summary>
    ///     Represents an action to validate the state of a page, including specified page-level expectations and locator-based
    ///     conditions.
    ///     This action ensures that the defined expectations for the page and its elements are met during a test execution.
    /// </summary>
    public ExpectPageStateAction(string name, Builder builder) : base(name, builder)
    {
        _locatorExpectations = builder.GetLocatorExpectations();
        _pageExpectations = builder.GetPageExpectations();
        _continueOnFailure = builder.ContinueOnFailure;
    }

    /// <summary>
    ///     Executes the defined page and locator expectation logic against the specified browser and context,
    ///     ensuring that the required page state matches the expectations.
    ///     This method evaluates page-level expectations first, followed by locator-level expectations, and handles
    ///     failures based on configured rules (e.g., whether to continue on failure or throw exceptions immediately).
    /// </summary>
    /// <param name="browser">
    ///     The instance of <c>PlaywrightBrowser</c> representing the browser to execute expectations
    ///     against.
    /// </param>
    /// <param name="context">
    ///     The <c>TestContext</c> providing contextual information for the test action, such as test state
    ///     or additional metadata.
    /// </param>
    /// <returns>
    ///     An asynchronous task representing the completion of the expectation execution.
    ///     Throws an exception if expectations fail and the operation is not set to continue on failure.
    /// </returns>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        var failures = new List<Exception>();

        // Execute page-level expectations first
        foreach (var pageExpectation in _pageExpectations)
        {
            try
            {
                await pageExpectation(browser.Page, context);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Page expectation failed");

                if (_continueOnFailure)
                {
                    failures.Add(ex);
                }
                else
                {
                    throw;
                }
            }
        }

        // Execute locator expectations
        try
        {
            await Task.Run(() => _locatorExpectations.Execute(context));
        }
        catch (Exception ex)
        {
            if (_continueOnFailure)
            {
                failures.Add(ex);
            }
            else
            {
                throw;
            }
        }

        if (failures.Count != 0)
        {
            var aggregateException = new AggregateException(
                $"Page state expectations failed ({failures.Count} expectations)",
                failures);

            throw new AgenixSystemException("Page state expectations failed", aggregateException);
        }
    }

    /// <summary>
    ///     Provides methods for building and configuring an <see cref="ExpectPageStateAction" />.
    ///     The <see cref="Builder" /> allows defining expectations for page state such as URL, title, and various
    ///     locator-based conditions.
    /// </summary>
    /// <remarks>
    ///     This class supports chaining to enable fluent configuration of the <see cref="ExpectPageStateAction" />.
    /// </remarks>
    public class Builder : Builder<ExpectPageStateAction, Builder>
    {
        private readonly ExpectMultipleLocatorsAction.Builder _locatorBuilder = new();
        private readonly List<Func<IPage, TestContext, Task>> _pageExpectations = [];
        internal bool ContinueOnFailure { get; private set; }

        /// <summary>
        ///     Configures the builder to allow the action to proceed even if a failure
        ///     occurs during the evaluation of expectations, enabling a more fault-tolerant
        ///     execution of locator or page state assertions.
        /// </summary>
        /// <returns>The current instance of the <c>Builder</c> class, allowing further configuration via method chaining.</returns>
        public Builder WithContinueOnFailure()
        {
            ContinueOnFailure = true;
            _locatorBuilder.WithContinueOnFailure();
            return this;
        }

        /// <summary>
        ///     Configures the execution mode for handling multiple locator expectations.
        /// </summary>
        /// <param name="mode">
        ///     The execution mode to apply, which determines how multiple locator expectations are executed (e.g.,
        ///     sequentially, in parallel, or fail-fast).
        /// </param>
        /// <returns>The current instance of the <see cref="ExpectPageStateAction.Builder" /> to enable method chaining.</returns>
        public Builder WithExecutionMode(ExpectMultipleLocatorsAction.ExecutionMode mode)
        {
            _locatorBuilder.WithExecutionMode(mode);
            return this;
        }

        // Page-level expectations
        /// <summary>
        ///     Adds an expectation to verify that the page URL matches the specified expected URL.
        /// </summary>
        /// <param name="expectedUrl">The expected URL of the page. Supports dynamic content resolution using the test context.</param>
        /// <returns>The current instance of the <see cref="ExpectPageStateAction.Builder" /> for method chaining.</returns>
        public Builder ExpectUrl(string expectedUrl)
        {
            _pageExpectations.Add(async (page, context) =>
            {
                var resolvedUrl = context.ReplaceDynamicContentInString(expectedUrl);
                await Expect(page).ToHaveURLAsync(resolvedUrl);
            });
            return this;
        }

        /// <summary>
        ///     Adds an expectation to verify that the page title matches the specified expected title.
        /// </summary>
        /// <param name="expectedTitle">The expected title of the page. Supports dynamic content resolution.</param>
        /// <returns>The current instance of the <see cref="ExpectPageStateAction.Builder" /> for method chaining.</returns>
        public Builder ExpectTitle(string expectedTitle)
        {
            _pageExpectations.Add(async (page, context) =>
            {
                var resolvedTitle = context.ReplaceDynamicContentInString(expectedTitle);
                await Expect(page).ToHaveTitleAsync(resolvedTitle);
            });
            return this;
        }

        /// <summary>
        ///     Adds an expectation that ensures the page has been completely loaded, including waiting for the network to be idle
        ///     and the body of the page to be visible.
        /// </summary>
        /// <returns>The current instance of the <see cref="ExpectPageStateAction.Builder" /> for method chaining.</returns>
        public Builder ExpectPageToBeLoaded()
        {
            _pageExpectations.Add(async (page, _) =>
            {
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                await Expect(page.Locator("body")).ToBeVisibleAsync();
            });
            return this;
        }

        // Locator expectations (delegate to ExpectMultipleLocatorsAction.Builder)
        /// <summary>
        ///     Adds a new locator expectation to the builder using the specified action.
        /// </summary>
        /// <param name="action">
        ///     An instance of <see cref="ExpectLocatorAction" /> representing the locator expectation to be
        ///     added.
        /// </param>
        /// <returns>The current instance of the <see cref="ExpectPageStateAction.Builder" /> for method chaining.</returns>
        public Builder AddExpectation(ExpectLocatorAction action)
        {
            _locatorBuilder.AddExpectation(action);
            return this;
        }

        /// <summary>
        ///     Adds a new expectation to the builder using the specified action builder function.
        /// </summary>
        /// <param name="builderFunc">
        ///     A function that configures and returns an instance of <see cref="ExpectLocatorAction" />
        ///     using its builder.
        /// </param>
        /// <returns>The current instance of the <see cref="ExpectPageStateAction.Builder" /> for method chaining.</returns>
        public Builder AddExpectation(Func<ExpectLocatorAction.Builder, ExpectLocatorAction> builderFunc)
        {
            _locatorBuilder.AddExpectation(builderFunc);
            return this;
        }

        internal ExpectMultipleLocatorsAction GetLocatorExpectations()
        {
            return _locatorBuilder.Build();
        }

        internal List<Func<IPage, TestContext, Task>> GetPageExpectations()
        {
            return _pageExpectations;
        }

        /// <summary>
        ///     Builds and returns an instance of <see cref="ExpectPageStateAction" />.
        /// </summary>
        /// <returns>An instance of <see cref="ExpectPageStateAction" />.</returns>
        public override ExpectPageStateAction Build()
        {
            return new ExpectPageStateAction(this);
        }
    }
}
