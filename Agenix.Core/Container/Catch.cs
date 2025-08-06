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
using System.Threading;
using System.Threading.Tasks;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// <summary>
///     Action catches possible exceptions in nested test actions.
/// </summary>
/// <param name="builder"></param>
public class Catch(Catch.Builder builder)
    : AbstractAsyncActionContainer(builder.GetName() ?? "catch", builder.GetDescription(), builder.GetActions())
{
    /// Static logger instance for the Catch class.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Catch));

    /// The type of exception that the Catch container is designed to catch.
    public string Exception { get; } = builder._exception;

    /// <summary>
    ///     Executes the "catch" container's logic by iterating through defined actions,
    ///     evaluating exceptions thrown by actions, and handling specific exceptions as per configuration.
    /// </summary>
    /// <param name="context">The test context containing details or objects required during action execution.</param>
    /// <param name="cancellationToken">A token to observe cancellation requests for the asynchronous operation.</param>
    /// <returns>A task that completes when the "catch" container process finishes execution.</returns>
    /// <exception cref="AgenixSystemException">Thrown when an unhandled exception occurs during the execution of actions.</exception>
    public override async Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Catch container catching exceptions of type {Exception}", Exception);
        }

        foreach (var actionBuilder in Actions)
        {
            try
            {
                await ExecuteAction(actionBuilder.Build(), context);
            }
            catch (Exception e)
            {
                if (Exception != null && Exception.Equals(e.GetType().Name))
                {
                    Log.LogInformation(e, "Caught exception {GetType}:{Message}", e.GetType(), e.Message);
                    continue;
                }

                throw new AgenixSystemException(e.Message, e);
            }
        }
    }


    /// Provides a fluent API for building and configuring exception handling actions within an action container.
    /// The Builder class is responsible for setting up the types of exceptions to be caught during execution.
    /// /
    public class Builder : AbstractExceptionContainerBuilder<Catch, Builder>
    {
        internal string _exception = nameof(AgenixSystemException);

        /// Fluent API action building entry method used in C# DSL.
        /// @return A builder instance for configuring exception catching behavior.
        /// /
        public static Builder CatchException()
        {
            return new Builder();
        }

        /// Catch an exception type during execution.
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

        /// Builds and returns a new instance of the `Catch` container.
        /// @return A constructed instance of the `Catch` container configured using the builder.
        protected override Catch DoBuild()
        {
            return new Catch(this);
        }
    }
}
