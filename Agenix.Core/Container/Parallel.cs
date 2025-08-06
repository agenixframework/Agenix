#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2025 Agenix
//
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    : AbstractAsyncActionContainer(builder.GetName() ?? "parallel", builder.GetName(), builder.GetActions())
{
    /// Static logger instance for the Parallel class.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Parallel));

    /**
     * Collect exceptions in a list
     */
    private readonly List<AgenixSystemException> _exceptions = [];

    /// Executes all the test actions in parallel using Task.Run and captures any exceptions.
    /// <param name="context">The TestContext in which the actions are executed.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <return>Returns a Task that completes when all actions have been executed or an exception is thrown.</return>
    public override async Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        var tasks = Actions.Select(actionBuilder => actionBuilder.Build())
            .Select(action => Task.Run(async () => await new ActionRunner(action, context, _exceptions.Add).Run(),
                cancellationToken))
            .ToArray();

        await Task.WhenAll(tasks);

        if (_exceptions.Count > 0)
        {
            if (_exceptions.Count == 1)
            {
                throw _exceptions[0];
            }

            throw new ParallelContainerException(_exceptions);
        }
    }

    /// <summary>
    ///     Executes a given test action, handling any exceptions that may occur.
    /// </summary>
    private sealed class ActionRunner(
        IAsyncTestAction action,
        TestContext context,
        Action<AgenixSystemException> exceptionHandler)
    {
        /// <summary>
        ///     Execute the test action, capturing any exceptions and logging errors.
        /// </summary>
        public async Task Run()
        {
            try
            {
                await action.ExecuteAsync(context);
            }
            catch (AgenixSystemException e)
            {
                Log.LogError(e, "Parallel test action raised error");
                lock (exceptionHandler)
                {
                    exceptionHandler(e);
                }
            }
            catch (Exception e)
            {
                Log.LogError(e, "Parallel test action raised error");
                lock (exceptionHandler)
                {
                    exceptionHandler(new AgenixSystemException(e.Message, e));
                }
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

        /// Builds and returns an instance of the Parallel test action container.
        /// <returns>An instance of the Parallel test action container.</returns>
        protected override Parallel DoBuild()
        {
            return new Parallel(this);
        }
    }
}
