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

using Agenix.Api.Context;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// Sequence container executing a set of nested test actions in a simple sequence.
/// /
public class Sequence(Sequence.Builder builder) : AbstractActionContainer(builder.GetName() ?? "sequential",
    builder.GetDescription(), builder.GetActions())
{
    /// <summary>
    ///     A logger instance for the Sequence class, used to record log messages
    ///     related to the execution of a sequence of test actions.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Sequence));

    /// Executes the sequence of nested test actions in the provided context.
    /// @param context The context in which the test actions are executed.
    /// /
    public override void DoExecute(TestContext context)
    {
        foreach (var actionBuilder in actions)
        {
            ExecuteAction(actionBuilder.Build(), context);
        }

        Log.LogDebug("Action sequence finished successfully.");
    }

    /// Builder class for constructing instances of the Sequence action.
    public sealed class Builder : AbstractTestContainerBuilder<Sequence, Builder>
    {
        /// Initializes a builder for constructing a Sequence container.
        /// A Sequence container ensures actions are executed in a sequential order.
        /// <returns>A builder instance for creating a Sequence container.</returns>
        public static Builder Sequential()
        {
            return new Builder();
        }

        protected override Sequence DoBuild()
        {
            return new Sequence(this);
        }
    }
}
