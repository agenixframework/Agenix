using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using log4net;

namespace Agenix.Core.Container;

/// Represents a container for executing multiple test actions in parallel.
/// Each action is executed concurrently in its own thread, and the container
/// waits for the successful completion or failure of all threads.
/// /
public class Parallel(Parallel.Builder builder)
    : AbstractActionContainer(builder.GetName() ?? "parallel", builder.GetName(), builder.GetActions())
{
    /// Static logger instance for the Parallel class.
    /// /
    private static readonly ILog Log = LogManager.GetLogger(typeof(Parallel));

    /**
     * Collect exceptions in list
     */
    private readonly List<AgenixSystemException> _exceptions = [];

    /**
     * Store created threads in stack
     */
    private readonly Stack<Thread> _threads = new();

    /// Executes all the test actions in parallel, managing threads and capturing any exceptions.
    /// <param name="context">The TestContext in which the actions are executed.</param>
    public override void DoExecute(TestContext context)
    {
        foreach (var t in actions.Select(actionBuilder => actionBuilder.Build()).Select(action =>
                     new Thread(new ActionRunner(action, context, _exceptions.Add).Run)))
        {
            _threads.Push(t);
            t.Start();
        }

        while (_threads.Count > 0)
            try
            {
                _threads.Pop().Join();
            }
            catch (ThreadInterruptedException e)
            {
                Log.Error("Unable to join thread", e);
            }

        if (_exceptions.Count > 0)
        {
            if (_exceptions.Count == 1) throw _exceptions[0];

            throw new ParallelContainerException(_exceptions);
        }
    }

    /// Executes a given test action within a separate thread, handling any exceptions that may occur.
    /// /
    private class ActionRunner(ITestAction action, TestContext context, Action<AgenixSystemException> exceptionHandler)
    {
        /// Test action to execute.
        private readonly ITestAction _action = action;

        /// Test context used for executing the action within the ActionRunner class.
        private readonly TestContext _context = context;

        /// Delegate responsible for handling instances of CoreSystemException.
        private readonly Action<AgenixSystemException> _exceptionHandler = exceptionHandler;

        /// Execute the test action, capturing any exceptions and logging errors.
        /// /
        public void Run()
        {
            try
            {
                _action.Execute(_context);
            }
            catch (AgenixSystemException e)
            {
                Log.Error("Parallel test action raised error", e);
                _exceptionHandler(e);
            }
            catch (Exception e)
            {
                Log.Error("Parallel test action raised error", e);
                _exceptionHandler(new AgenixSystemException(e.Message));
            }
        }
    }

    /// Provides a builder for constructing Parallel action containers.
    /// /
    public class Builder : AbstractTestContainerBuilder<Parallel, Builder>
    {
        /// Fluent API action building entry method used in C# DSL.
        /// @return An instance of the builder for Parallel actions.
        /// /
        public static Builder Parallel()
        {
            return new Builder();
        }

        protected override Parallel DoBuild()
        {
            return new Parallel(this);
        }
    }
}