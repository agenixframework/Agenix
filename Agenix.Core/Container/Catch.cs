using System;
using Agenix.Core.Exceptions;
using log4net;

namespace Agenix.Core.Container;

/// <summary>
///     Action catches possible exceptions in nested test actions.
/// </summary>
/// <param name="builder"></param>
public class Catch(Catch.Builder builder)
    : AbstractActionContainer(builder.GetName() ?? "catch", builder.GetDescription(), builder.GetActions())
{
    /// Static logger instance for the Catch class.
    /// /
    private static readonly ILog Log = LogManager.GetLogger(typeof(Catch));

    /// The type of exception that the Catch container is designed to catch.
    public string Exception { get; } = builder._exception;

    public override void DoExecute(TestContext context)
    {
        if (Log.IsDebugEnabled) Log.Debug("Catch container catching exceptions of type " + Exception);

        foreach (var actionBuilder in actions)
            try
            {
                ExecuteAction(actionBuilder.Build(), context);
            }
            catch (Exception e)
            {
                if (Exception != null && Exception.Equals(e.GetType().Name))
                {
                    Log.Info("Caught exception " + e.GetType() + ": " + e.Message);
                    continue;
                }

                throw new CoreSystemException(e.Message, e);
            }
    }


    /// Provides a fluent API for building and configuring exception handling actions within an action container.
    /// The Builder class is responsible for setting up the types of exceptions to be caught during execution.
    /// /
    public class Builder : AbstractExceptionContainerBuilder<Catch, Builder>
    {
        internal string _exception = nameof(CoreSystemException);

        /// Fluent API action building entry method used in C# DSL.
        /// @return A builder instance for configuring exception catching behavior.
        /// /
        public static Builder CatchException()
        {
            return new Builder();
        }

        /// Catch exception type during execution.
        /// @param exception The type of exception to catch during execution.
        /// @return A builder instance that allows further configuration.
        /// /
        public Builder Exception(Type exception)
        {
            _exception = exception.Name;
            return this;
        }

        /// Represents an exception type that indicates errors during program execution.
        /// /
        public Builder Exception(string type)
        {
            _exception = type;
            return this;
        }

        protected override Catch DoBuild()
        {
            return new Catch(this);
        }
    }
}