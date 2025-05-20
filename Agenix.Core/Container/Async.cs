using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Core.Actions;
using log4net;

namespace Agenix.Core.Container;

/// <summary>
///     Represents an asynchronous action container that manages the execution
///     of a set of actions asynchronously in a test context. It handles both
///     the primary action execution and the retrieval of success or error
///     actions configured within the container.
/// </summary>
public class Async(Async.Builder builder)
    : AbstractActionContainer(builder.GetName() ?? "async", builder.GetDescription(), builder.GetActions())
{
    /// Static logger instance for the Async class.
    /// /
    private static readonly ILog Log = LogManager.GetLogger(typeof(Async));

    private readonly List<ITestActionBuilder<ITestAction>> _errorActions = builder._errorActions;
    private readonly List<ITestActionBuilder<ITestAction>> _successActions = builder._successActions;

    /// Executes the asynchronous container's primary action in the provided test context.
    /// <param name="context">The context in which the test action is executed, providing runtime data and dependencies.</param>
    public override void DoExecute(TestContext context)
    {
        Log.Debug("Async container forking action execution ...");

        var asyncTestAction = new ExtendedAbstractAsyncTestAction(this);

        ExecuteAction(asyncTestAction, context);
    }

    /// Retrieves the list of success test actions that have been configured for this Async container.
    /// <return>A list of built ITestAction instances representing success actions.</return>
    public List<ITestAction> GetSuccessTestActions()
    {
        return _successActions.Select(actionBuilder => actionBuilder.Build()).ToList();
    }

    /// Retrieves the list of error test actions that have been configured for this Async container.
    /// <return>A list of built ITestAction instances representing error actions.</return>
    public List<ITestAction> GetErrorTestActions()
    {
        return _errorActions.Select(actionBuilder => actionBuilder.Build()).ToList();
    }

    /// <summary>
    ///     Represents an extended asynchronous test action within the async action container.
    ///     It is responsible for managing the execution of a set of actions asynchronously,
    ///     and handling completion events such as success or errors during the execution process.
    /// </summary>
    private class ExtendedAbstractAsyncTestAction(Async outerInstance) : AbstractAsyncTestAction
    {
        public Func<TestContext, Task> DoExecuteAsyncFunc { get; init; }

        public override Task DoExecuteAsync(TestContext context)
        {
            foreach (var action in outerInstance.actions.Select(actionBuilder => actionBuilder.Build()))
                outerInstance.ExecuteAction(action, context);

            return Task.CompletedTask;
        }

        public override void OnError(TestContext context, Exception error)
        {
            Log.Info("Apply error actions after async container ...");
            foreach (var action in outerInstance._errorActions.Select(actionBuilder => actionBuilder.Build()))
                action.Execute(context);
        }

        public override void OnSuccess(TestContext context)
        {
            Log.Info("Apply success actions after async container ...");
            foreach (var action in outerInstance._successActions.Select(actionBuilder => actionBuilder.Build()))
                action.Execute(context);
        }
    }

    /// <summary>
    ///     Builder class for constructing instances of the Async action container.
    ///     It provides methods to define success and error actions, allowing these
    ///     actions to be specified either individually or in bulk. The builder follows
    ///     a fluent API pattern to enable chaining and facilitate flexible configuration.
    /// </summary>
    public class Builder : AbstractTestContainerBuilder<Async, Builder>
    {
        internal readonly List<ITestActionBuilder<ITestAction>> _errorActions = [];
        internal readonly List<ITestActionBuilder<ITestAction>> _successActions = [];

        /// Fluent API action building entry method used in C# DSL.
        /// @return
        /// /
        public static Builder Async()
        {
            return new Builder();
        }

        /// Adds an error action.
        /// @param action The error action to add.
        /// @return Returns the builder instance for chaining.
        /// /
        public Builder ErrorAction(ITestAction action)
        {
            _errorActions.Add(new FuncITestActionBuilder<ITestAction>(() => action));
            return this;
        }

        /// <summary>
        ///     Adds a new error action to the list of error actions in the builder.
        /// </summary>
        /// <param name="action">The action to be added as an error action.</param>
        /// <returns>The builder instance with the newly added error action.</returns>
        public Builder ErrorAction(TestAction action)
        {
            _errorActions.Add(new FuncITestActionBuilder<ITestAction>(() => new DelegatingTestAction(action)));
            return this;
        }

        /// Adds an error action.
        /// <param name="action">The error action to add.</param>
        /// <return>Returns the builder instance for chaining.</return>
        public Builder ErrorAction(ITestActionBuilder<ITestAction> action)
        {
            _errorActions.Add(action);
            return this;
        }

        /// Adds a success action.
        /// @param action The success action to add.
        /// @return Returns the builder instance for chaining.
        /// /
        public Builder SuccessAction(ITestAction action)
        {
            _successActions.Add(new FuncITestActionBuilder<ITestAction>(() => action));
            return this;
        }

        /// <summary>
        ///     Adds a new success action to the list of success actions in the builder.
        /// </summary>
        /// <param name="action">The action to be added as a success action.</param>
        /// <returns>The builder instance with the newly added success action.</returns>
        public Builder SuccessAction(TestAction action)
        {
            _successActions.Add(new FuncITestActionBuilder<ITestAction>(() => new DelegatingTestAction(action)));
            return this;
        }

        /// Adds a success action.
        /// <param name="action">The success action to add.</param>
        /// <return>Returns the builder instance for chaining.</return>
        public Builder SuccessAction(ITestActionBuilder<ITestAction> action)
        {
            _successActions.Add(action);
            return this;
        }

        /// Adds multiple error actions.
        /// <param name="actions">The error actions to add as params.</param>
        /// <return>Returns the builder instance for chaining.</return>
        public Builder ErrorActions(params ITestActionBuilder<ITestAction>[] actions)
        {
            _errorActions.AddRange(actions.ToList());
            return this;
        }

        /// Adds a collection of error actions to the builder.
        /// <param name="actions">An array of error actions to be added.</param>
        /// <return>The current instance of the Builder.</return>
        public Builder ErrorActions(params TestAction[] actions)
        {
            _errorActions.AddRange(actions.Select(action =>
                new FuncITestActionBuilder<ITestAction>(() => new DelegatingTestAction(action))));
            return this;
        }

        /// Adds multiple error actions to the builder.
        /// <param name="actions">The error actions to be added.</param>
        /// <returns>The builder instance with the added error actions.</returns>
        public Builder ErrorActions(params ITestAction[] actions)
        {
            _errorActions.AddRange(actions.Select(action => new FuncITestActionBuilder<ITestAction>(() => action)));
            return this;
        }

        /// Adds multiple success actions.
        /// <param name="actions">The success actions to add as params.</param>
        /// <return>Returns the builder instance for chaining.</return>
        public Builder SuccessActions(params ITestActionBuilder<ITestAction>[] actions)
        {
            _successActions.AddRange(actions.ToList());
            return this;
        }

        /// Adds a collection of success actions to the builder.
        /// <param name="actions">An array of success actions to be added.</param>
        /// <return>The current instance of the Builder.</return>
        public Builder SuccessActions(params TestAction[] actions)
        {
            _successActions.AddRange(actions.Select(action =>
                new FuncITestActionBuilder<ITestAction>(() => new DelegatingTestAction(action))));
            return this;
        }

        /// Adds multiple success actions to the builder.
        /// <param name="actions">The success actions to be added.</param>
        /// <returns>The builder instance with the added success actions.</returns>
        public Builder SuccessActions(params ITestAction[] actions)
        {
            _successActions.AddRange(actions.Select(action => new FuncITestActionBuilder<ITestAction>(() => action)));
            return this;
        }

        /// Builds and returns an instance of Async.
        /// The constructed Async instance.
        public override Async Build()
        {
            return DoBuild();
        }

        /// Constructs a new instance of the Async class using the provided Builder instance.
        /// @return
        /// A new instance of the Async class.
        protected override Async DoBuild()
        {
            return new Async(this);
        }
    }
}