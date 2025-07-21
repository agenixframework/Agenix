using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Playwright.Endpoint;
using Microsoft.Extensions.Logging;

namespace Agenix.Playwright.Actions;

/// <summary>
///     The ExpectMultipleLocatorsAction class is designed to execute expectations on multiple locators
///     using Playwright. It provides functionality for configuring and performing operations on web elements
///     and supports different execution modes such as sequential, parallel, and fail-fast.
/// </summary>
public class ExpectMultipleLocatorsAction : AbstractPlaywrightAction
{
    /// <summary>
    ///     Defines the different modes of execution for locator actions.
    /// </summary>
    public enum ExecutionMode
    {
        /// <summary>
        ///     Specifies that locator actions will be executed sequentially, one after another.
        ///     This mode ensures that each action completes before the next one begins,
        ///     maintaining a strict order of operations.
        /// </summary>
        SEQUENTIAL, // Execute expectations one after another

        /// <summary>
        ///     Specifies that locator actions will be executed concurrently,
        ///     allowing multiple actions to proceed in parallel without waiting for each other.
        ///     This mode can improve execution speed but does not guarantee the order of completion.
        /// </summary>
        PARALLEL, // Execute all expectations simultaneously

        /// <summary>
        ///     Specifies that locator actions will be executed in a fail-fast manner.
        ///     This mode ensures that execution stops immediately upon encountering the
        ///     first failure, without attempting any subsequent actions.
        /// </summary>
        FAIL_FAST // Stop on first failure (overrides ContinueOnFailure)
    }

    private static readonly ILogger Logger = LogManager.GetLogger(typeof(ExpectMultipleLocatorsAction));
    private readonly bool _continueOnFailure;
    private readonly ExecutionMode _executionMode;
    private readonly int _globalTimeoutMs;

    private readonly List<ExpectLocatorAction> _locatorActions;


    /// <summary>
    ///     Represents an action that applies a set of expectations to multiple locators in a Playwright browser context.
    ///     This action supports specifying different configurations, such as execution modes and timeouts, and allows
    ///     multiple expectations to be added and executed.
    /// </summary>
    public ExpectMultipleLocatorsAction(Builder builder) : this("expect-multiple-locators", builder) { }

    /// <summary>
    ///     Defines an action that allows applying multiple expectations to locators within a Playwright browser context.
    ///     This action manages a collection of locator-based expectations and provides various configurations
    ///     including execution mode (sequential, parallel, fail-fast), timeout settings, and the option to continue processing
    ///     on failures.
    /// </summary>
    public ExpectMultipleLocatorsAction(string name, Builder builder) : base(name, builder)
    {
        _locatorActions = builder.GetLocatorActions();
        _continueOnFailure = builder.ContinueOnFailure;
        _executionMode = builder.ExecutionMode;
        _globalTimeoutMs = builder.GlobalTimeoutMs;
    }

    /// <summary>
    ///     Executes the configured actions for multiple locators in the Playwright browser
    ///     using the provided test context. It processes each locator action based on the
    ///     defined execution mode and optionally continues on failures.
    /// </summary>
    /// <param name="browser">
    ///     An instance of <c>PlaywrightBrowser</c> used for performing actions in the Playwright testing
    ///     environment.
    /// </param>
    /// <param name="context">An instance of <c>TestContext</c> providing contextual information required for execution.</param>
    /// <returns>A task representing the asynchronous operation of executing multiple locator actions.</returns>
    /// <exception cref="AgenixSystemException">Thrown when an unexpected error occurs during the execution process.</exception>
    protected override async Task Execute(PlaywrightBrowser browser, TestContext context)
    {
        if (_locatorActions.Count == 0)
        {
            Logger.LogWarning("No locator actions to execute");
            return;
        }

        Logger.LogInformation(
            "Executing {ActionCount} locator actions with mode={ExecutionMode}, continueOnFailure={ContinueOnFailure}",
            _locatorActions.Count, _executionMode, _continueOnFailure);

        var failures = new List<Exception>();
        using var cancellationTokenSource = new CancellationTokenSource();

        if (_globalTimeoutMs > 0)
        {
            cancellationTokenSource.CancelAfter(_globalTimeoutMs);
        }

        try
        {
            switch (_executionMode)
            {
                case ExecutionMode.SEQUENTIAL:
                    await ExecuteSequentially(context, failures, cancellationTokenSource.Token);
                    break;

                case ExecutionMode.PARALLEL:
                    await ExecuteInParallel(context, failures, cancellationTokenSource.Token);
                    break;

                case ExecutionMode.FAIL_FAST:
                    await ExecuteFailFast(context, cancellationTokenSource.Token);
                    break;
                default:
                    throw new AgenixSystemException("Unknown execution mode");
            }

            if (failures.Count != 0)
            {
                var aggregateException = new AggregateException(
                    $"Multiple locator expectations failed ({failures.Count} of {_locatorActions.Count})",
                    failures);

                Logger.LogError(aggregateException,
                    "Multiple locator expectations completed with {FailureCount} failures", failures.Count);
                throw new AgenixSystemException("Multiple locator expectations failed", aggregateException);
            }

            Logger.LogInformation("All locator expectations completed successfully");
        }
        catch (OperationCanceledException ex)
        {
            Logger.LogError(ex, "Multiple locator expectations timed out after {TimeoutMs}ms", _globalTimeoutMs);
            throw new AgenixSystemException($"Multiple locator expectations timed out after {_globalTimeoutMs}ms");
        }
    }

    private async Task ExecuteSequentially(TestContext context, List<Exception> failures,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < _locatorActions.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var action = _locatorActions[i];
            try
            {
                Logger.LogDebug("Executing locator action {Index}/{Total}: {ActionName}", i + 1, _locatorActions.Count,
                    action.GetType().Name);
                await Task.Run(() => action.Execute(context), cancellationToken);
                Logger.LogDebug("Locator action {Index}/{Total} completed successfully", i + 1, _locatorActions.Count);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Locator action {Index}/{Total} failed", i + 1, _locatorActions.Count);

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
    }

    private async Task ExecuteInParallel(TestContext context, List<Exception> failures,
        CancellationToken cancellationToken)
    {
        var tasks = _locatorActions.Select(async (action, index) =>
        {
            try
            {
                Logger.LogDebug("Starting parallel execution of locator action {Index}: {ActionName}", index,
                    action.GetType().Name);
                await Task.Run(() => action.Execute(context), cancellationToken);
                Logger.LogDebug("Parallel locator action {Index} completed successfully", index);
                return (Index: index, Success: true, Exception: null);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Parallel locator action {Index} failed", index);
                return (Index: index, Success: false, Exception: ex);
            }
        });

        var results = await Task.WhenAll(tasks);

        var exceptions = results.Where(r => !r.Success).Select(result => result.Exception);
        foreach (var exception in exceptions)
        {
            if (_continueOnFailure)
            {
                failures.Add(exception!);
            }
            else
            {
                throw exception!;
            }
        }
    }

    private async Task ExecuteFailFast(TestContext context, CancellationToken cancellationToken)
    {
        for (var i = 0; i < _locatorActions.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var action = _locatorActions[i];
            Logger.LogDebug("Executing fail-fast locator action {Index}/{Total}: {ActionName}", i + 1,
                _locatorActions.Count, action.GetType().Name);
            await Task.Run(() => action.Execute(context), cancellationToken);
            Logger.LogDebug("Fail-fast locator action {Index}/{Total} completed successfully", i + 1,
                _locatorActions.Count);
        }
    }

    /// <summary>
    ///     The Builder class is a fluent API for constructing and configuring instances of
    ///     the ExpectMultipleLocatorsAction class. This class provides methods to define
    ///     expectations for multiple locators and configure execution parameters.
    /// </summary>
    public class Builder : Builder<ExpectMultipleLocatorsAction, Builder>
    {
        private readonly List<ExpectLocatorAction> _locatorActions = [];
        internal bool ContinueOnFailure { get; private set; }
        internal ExecutionMode ExecutionMode { get; private set; } = ExecutionMode.SEQUENTIAL;
        internal int GlobalTimeoutMs { get; private set; } = 30000; // 30 seconds default

        /// <summary>
        ///     Configures the action to continue execution even if a failure is encountered
        ///     during the evaluation of expectations for multiple locators. This method
        ///     sets an internal flag that allows the process to proceed without halting
        ///     on the first error.
        /// </summary>
        /// <returns>The current instance of the <c>Builder</c> class, enabling method chaining for further configuration.</returns>
        public Builder WithContinueOnFailure()
        {
            ContinueOnFailure = true;
            return this;
        }

        /// <summary>
        ///     Configures the execution mode for the action when evaluating expectations for multiple locators.
        ///     The execution mode determines how the steps are processed: sequentially, in parallel,
        ///     or halting immediately upon the first failure.
        /// </summary>
        /// <param name="mode">
        ///     The <c>ExecutionMode</c> to set for the action, defining the desired
        ///     processing strategy (Sequential, Parallel, or FailFast).
        /// </param>
        /// <returns>The current instance of the <c>Builder</c> class, enabling method chaining for further configuration.</returns>
        public Builder WithExecutionMode(ExecutionMode mode)
        {
            ExecutionMode = mode;
            return this;
        }

        /// <summary>
        ///     Configures a global timeout for the evaluation of expectations across multiple locators.
        ///     This setting determines the maximum amount of time, in milliseconds, that the entire
        ///     action is allowed to take before it times out.
        /// </summary>
        /// <param name="timeoutMs">The global timeout value in milliseconds to be applied for this action.</param>
        /// <returns>The current instance of the <c>Builder</c> class, enabling method chaining for further configuration.</returns>
        public Builder WithGlobalTimeout(int timeoutMs)
        {
            GlobalTimeoutMs = timeoutMs;
            return this;
        }

        /// <summary>
        ///     Adds a single expectation to be evaluated against a locator action. This method
        ///     allows appending a specific action to the collection of locator expectations
        ///     managed by the builder instance.
        /// </summary>
        /// <param name="action">The instance of <c>ExpectLocatorAction</c> to be added. Must not be null.</param>
        /// <returns>The current instance of the <c>Builder</c> class to allow for fluent method chaining.</returns>
        public Builder AddExpectation(ExpectLocatorAction action)
        {
            _locatorActions.Add(action ?? throw new ArgumentNullException(nameof(action)));
            return this;
        }

        /// <summary>
        ///     Adds an expectation to the builder, using the specified <see cref="ExpectLocatorAction.Builder" /> instance to
        ///     define it.
        ///     The added expectation specifies the conditions that one or more locators need to satisfy during execution.
        /// </summary>
        /// <param name="action">The builder used to construct and configure the expectation to be added.</param>
        /// <returns>The current instance of the builder, allowing for method chaining.</returns>
        public Builder AddExpectation(ExpectLocatorAction.Builder action)
        {
            return AddExpectation(action.Build());
        }

        /// <summary>
        ///     Adds an expectation to the list of locators that will be evaluated. This method allows
        ///     configuring the expected behavior of a locator by using a builder function to define
        ///     specific conditions or assertions. The newly constructed action is appended to the internal
        ///     collection of locator actions, which will be processed during execution.
        /// </summary>
        /// <param name="builderFunc">
        ///     A function that configures and constructs an instance
        ///     of the <c>ExpectLocatorAction</c> using its builder.
        /// </param>
        /// <returns>
        ///     The current instance of the <c>Builder</c> class, enabling method chaining
        ///     for further configuration.
        /// </returns>
        public Builder AddExpectation(Func<ExpectLocatorAction.Builder, ExpectLocatorAction> builderFunc)
        {
            var expectBuilder = new ExpectLocatorAction.Builder();

            // Copy browser configuration from parent to child
            if (Browser != null)
            {
                expectBuilder.WithBrowser(Browser);
            }

            var action = builderFunc(expectBuilder);
            _locatorActions.Add(action);
            return this;
        }

        /// <summary>
        ///     Adds an expectation to the list of locator expectations to be evaluated.
        ///     This method accepts a function that configures a builder for the locator action,
        ///     enabling fine-grained control over the expectation settings and conditions.
        /// </summary>
        /// <param name="builderFunc">
        ///     A function that takes an <see cref="ExpectLocatorAction.Builder" />
        ///     as input and returns a configured builder for the locator action.
        /// </param>
        /// <returns>Returns the builder instance to allow method chaining.</returns>
        public Builder AddExpectation(Func<ExpectLocatorAction.Builder, ExpectLocatorAction.Builder> builderFunc)
        {
            var expectBuilder = new ExpectLocatorAction.Builder();

            // Copy browser configuration from parent to child
            if (Browser != null)
            {
                expectBuilder.WithBrowser(Browser);
            }

            var action = builderFunc(expectBuilder);
            _locatorActions.Add(action.Build());
            return this;
        }

        /// <summary>
        ///     Adds one or more expectations to the collection of locator actions to be evaluated
        ///     during the execution of the ExpectMultipleLocatorsAction. This method allows for
        ///     configuring multiple expectations for locators in a single step.
        /// </summary>
        /// <param name="actions">
        ///     An array of <c>ExpectLocatorAction</c> objects representing the locator expectations to be added.
        ///     Cannot be null.
        /// </param>
        /// <returns>The current instance of the <c>Builder</c> class, enabling method chaining for further configuration.</returns>
        public Builder AddExpectations(params ExpectLocatorAction[] actions)
        {
            _locatorActions.AddRange(actions ?? throw new ArgumentNullException(nameof(actions)));
            return this;
        }

        /// <summary>
        ///     Adds a collection of expectations to the action, allowing multiple
        ///     instances of <c>ExpectLocatorAction</c> to be specified. Each expectation
        ///     contains its own configuration and logic for validating locators during
        ///     execution.
        /// </summary>
        /// <param name="actions">
        ///     An <c>IEnumerable</c> collection of
        ///     <c>ExpectLocatorAction</c> objects to be added. Each action represents
        ///     an individual expectation for a locator.
        /// </param>
        /// <returns>
        ///     The current instance of the <c>Builder</c> class, enabling
        ///     method chaining for further configuration.
        /// </returns>
        public Builder AddExpectations(IEnumerable<ExpectLocatorAction> actions)
        {
            _locatorActions.AddRange(actions ?? throw new ArgumentNullException(nameof(actions)));
            return this;
        }

        internal List<ExpectLocatorAction> GetLocatorActions()
        {
            return _locatorActions;
        }

        /// <summary>
        ///     Constructs and returns a fully configured instance of the <c>ExpectMultipleLocatorsAction</c> class.
        ///     This method finalizes the current configuration and prepares the action for execution.
        /// </summary>
        /// <returns>
        ///     A new instance of the <c>ExpectMultipleLocatorsAction</c> class, built based on the current state of the
        ///     <c>Builder</c>.
        /// </returns>
        public override ExpectMultipleLocatorsAction Build()
        {
            return new ExpectMultipleLocatorsAction(this);
        }
    }
}
