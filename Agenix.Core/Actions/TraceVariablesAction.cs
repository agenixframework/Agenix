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

using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Actions;

/// <summary>
///     Action that prints variable values to the console/logger. Action requires a list of variable names. Tries to find
///     the variables in the test context and print its values.
/// </summary>
public class TraceVariablesAction(TraceVariablesAction.Builder builder) : AbstractTestAction("trace", builder)
{
    /// Logger for SleepAction.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(TraceVariablesAction));

    /**
     * List of variable names
     */
    private readonly List<string> _variableNames = builder.variableNames;

    /// <summary>
    ///     Executes the action of tracing variable values by logging them.
    ///     The list of variable names will be used if provided, otherwise, all variables from the context will be traced.
    /// </summary>
    /// <param name="context">The test context containing the variables</param>
    public override void DoExecute(TestContext context)
    {
        Log.LogInformation("Trace variables");

        IEnumerator<string> it;
        if (_variableNames != null && _variableNames.Count > 0)
        {
            it = _variableNames.GetEnumerator();
        }
        else
        {
            it = context.GetVariables().Keys.GetEnumerator();
        }

        while (it.MoveNext())
        {
            var key = it.Current;
            var value = context.GetVariable(key);

            Log.LogInformation("Variable " + context.LogModifier.Mask(key + " = " + value));
        }
    }

    /// Provides a builder for creating instances of TraceVariablesAction.
    /// /
    public class Builder : AbstractTestActionBuilder<ITestAction, dynamic>
    {
        internal readonly List<string> variableNames = [];

        /// Fluent API action building entry method used in C# DSL.
        /// @return A Builder instance for the TraceVariablesAction.
        /// /
        public static Builder Trace()
        {
            return new Builder();
        }

        /// Fluent API action building entry method used in Java DSL.
        /// @return Builder instance for tracing variables.
        /// /
        public static Builder TraceVariables()
        {
            return new Builder();
        }

        /// Fluent API action building entry method used in C# DSL.
        /// @param variable The name of the variable to trace.
        /// @return The builder instance.
        /// /
        public static Builder TraceVariables(params string[] variableNames)
        {
            var builder = new Builder();
            builder.Variables(variableNames);
            return builder;
        }

        /// Fluent API action building entry method used in Java DSL.
        /// @param variable The name of the variable to trace.
        /// @return The builder instance.
        /// /
        public static Builder TraceVariables(string variable)
        {
            var builder = new Builder();
            builder.Variable(variable);
            return builder;
        }

        /// Adds a variable name to the list of variable names to be traced.
        /// @param variable The name of the variable to trace.
        /// @return The builder instance.
        /// /
        public Builder Variable(string variable)
        {
            variableNames.Add(variable);
            return this;
        }

        /// Fluent API action building entry method used in C# DSL.
        /// @param variables The list of variable names to trace.
        /// @return The builder instance.
        /// /
        public Builder Variables(params string[] variables)
        {
            foreach (var variable in variables)
            {
                Variable(variable);
            }

            return this;
        }

        public override TraceVariablesAction Build()
        {
            return new TraceVariablesAction(this);
        }
    }
}
