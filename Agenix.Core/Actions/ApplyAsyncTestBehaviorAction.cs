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

using System.Threading;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Context;

namespace Agenix.Core.Actions;

/// <summary>
///     Represents an action that applies a test behavior using a specified runner.
/// </summary>
public class ApplyAsyncTestBehaviorAction(ApplyAsyncTestBehaviorAction.Builder builder)
    : AbstractTestActionAsync(builder.GetName() ?? "apply-behaviour", builder.GetDescription())
{
    private readonly IAsyncTestBehavior _behavior = builder._behavior;
    private readonly IAsyncTestActionRunner _runner = builder._runner;

    /// <summary>
    ///     Executes the specified test behavior using the provided test context and cancellation token.
    /// </summary>
    /// <param name="context">The <see cref="TestContext" /> used to execute the test behavior.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A <see cref="Task" /> that represents the execution of the test behavior.</returns>
    public override Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        return _behavior.Apply(_runner);
    }

    /// <summary>
    ///     A builder class for creating instances of ApplyTestBehaviorAction.
    /// </summary>
    public class Builder : AbstractAsyncTestActionBuilder<ApplyAsyncTestBehaviorAction, Builder>,
        IAsyncTestActionBuilder<ApplyAsyncTestBehaviorAction>
    {
        internal IAsyncTestBehavior _behavior;
        internal IAsyncTestActionRunner _runner;

        /// <summary>
        ///     Constructs an instance of <see cref="ApplyTestBehaviorAction" /> using the current state of the builder.
        /// </summary>
        /// <returns>An instance of <see cref="ApplyTestBehaviorAction" />.</returns>
        public override ApplyAsyncTestBehaviorAction Build()
        {
            return new ApplyAsyncTestBehaviorAction(this);
        }

        /// <summary>
        ///     Creates a new instance of the Builder.
        /// </summary>
        /// <returns>A new instance of the Builder.</returns>
        public static Builder Apply()
        {
            return new Builder();
        }

        /// <summary>
        ///     Creates a new instance of the Builder with the provided behavior.
        /// </summary>
        /// <param name="behavior">The behavior to be applied to the builder.</param>
        /// <returns>A new instance of the Builder with the behavior set.</returns>
        public static Builder Apply(IAsyncTestBehavior behavior)
        {
            var builder = new Builder { _behavior = behavior };
            return builder;
        }

        /// <summary>
        ///     Creates a new instance of the Builder with the specified test behavior.
        /// </summary>
        /// <param name="behavior">The specific test behavior to be applied to the builder.</param>
        /// <returns>A new instance of the Builder with the specified test behavior set.</returns>
        public static Builder Apply(AsyncTestBehavior behavior)
        {
            return Apply(new DelegatingAsyncTestBehaviour(behavior));
        }

        /// <summary>
        ///     Sets the specified behavior for the builder.
        /// </summary>
        /// <param name="behavior">The behavior to be applied to the builder.</param>
        /// <returns>The builder instance with the behavior set.</returns>
        public Builder Behavior(IAsyncTestBehavior behavior)
        {
            _behavior = behavior;
            return this;
        }

        /// <summary>
        ///     Sets the specified behavior for the builder using the TestBehavior delegate.
        /// </summary>
        /// <param name="behavior">The TestBehavior delegate to be applied to the builder.</param>
        /// <returns>The builder instance with the behavior set through a DelegatingTestBehaviour.</returns>
        public Builder Behavior(AsyncTestBehavior behavior)
        {
            return Behavior(new DelegatingAsyncTestBehaviour(behavior));
        }

        /// <summary>
        ///     Assigns the provided test action runner to the builder.
        /// </summary>
        /// <param name="runner">The test action runner to be assigned.</param>
        /// <returns>The builder instance with the runner assigned.</returns>
        public Builder On(IAsyncTestActionRunner runner)
        {
            _runner = runner;
            return this;
        }
    }
}
