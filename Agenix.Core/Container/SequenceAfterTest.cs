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

using System.Linq;
using Agenix.Api.Container;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// <summary>
///     Sequence of test actions executed after a test case. Container execution can be restricted according to test name ,
///     namespace and test groups.
/// </summary>
public class SequenceAfterTest : AbstractTestBoundaryActionContainer, IAfterTest
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(SequenceAfterTest));

    /// Executes the set of actions contained in the SequenceAfterTest after a test is completed.
    /// <param name="context">The context in which the actions are executed.</param>
    public override void DoExecute(TestContext context)
    {
        if (actions is { Count: 0 })
        {
            return;
        }

        Log.LogInformation("Entering after test block");

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Executing " + actions.Count + " actions after test");
            Log.LogDebug("");
        }

        foreach (var action in actions.Select(actionBuilder => actionBuilder.Build()))
        {
            action.Execute(context);
        }
    }

    /// The Builder class is a concrete implementation of the AbstractTestBoundaryContainerBuilder used for constructing instances
    /// of SequenceAfterTest with specific configurations. This class provides a fluent API for setting various testing conditions.
    public class Builder : AbstractTestBoundaryContainerBuilder<SequenceAfterTest, Builder>
    {
        /// Fluent API action building entry method used in C# DSL.
        /// @return
        /// /
        public static Builder AfterTest()
        {
            return new Builder();
        }

        /// Builds and returns an instance of SequenceAfterTest.
        /// @return An instance of SequenceAfterTest.
        protected override SequenceAfterTest DoBuild()
        {
            return new SequenceAfterTest();
        }
    }
}
