#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

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
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Parallel));

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
                Log.LogError(e, "Unable to join thread");
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
                Log.LogError(e, "Parallel test action raised error");
                _exceptionHandler(e);
            }
            catch (Exception e)
            {
                Log.LogError(e, "Parallel test action raised error");
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
