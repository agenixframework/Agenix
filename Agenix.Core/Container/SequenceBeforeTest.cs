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
///     Sequence of test actions executed before a test case. Container execution can be restricted according to test
///     name,namespace and test groups.
/// </summary>
public class SequenceBeforeTest : AbstractTestBoundaryActionContainer, IBeforeTest
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(SequenceAfterTest));

    /// Executes a sequence of actions before a test is run.
    /// <param name="context">
    ///     The context in which the test actions are executed, providing state and environment information
    ///     for the actions.
    /// </param>
    public override void DoExecute(TestContext context)
    {
        if (actions == null || actions.Count == 0)
        {
            return;
        }

        Log.LogInformation("Entering before test block");

        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Executing " + actions.Count + " actions before test");
            Log.LogDebug("");
        }

        foreach (var action in actions.Select(actionBuilder => actionBuilder.Build()))
        {
            action.Execute(context);
        }
    }

    /// <summary>
    ///     Builder class for constructing instances of SequenceBeforeTest.
    /// </summary>
    public class Builder : AbstractTestBoundaryContainerBuilder<SequenceBeforeTest, Builder>
    {
        /// Fluent API action building entry method used in C# DSL.
        /// @return A new instance of the Builder class for constructing SequenceBeforeTest instances.
        /// /
        public static Builder BeforeTest()
        {
            return new Builder();
        }

        protected override SequenceBeforeTest DoBuild()
        {
            return new SequenceBeforeTest();
        }
    }
}
