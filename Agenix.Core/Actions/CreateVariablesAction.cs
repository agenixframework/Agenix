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
using System.Threading;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Actions;

/// <summary>
///     Action creating new test variables during a test. Existing test variables are overwritten by new values.
/// </summary>
public class CreateVariablesAction : AbstractTestActionAsync
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(CreateVariablesAction));

    private CreateVariablesAction(Builder builder) : base("create-variables", builder)
    {
        Variables = builder.Variables;
    }

    /// <summary>
    ///     Dictionary of variables where the key is the variable name and the value is the variable content.
    /// </summary>
    public IDictionary<string, string> Variables { get; }

    /// <summary>
    ///     Executes the action to create and set variables in the provided test context.
    /// </summary>
    /// <param name="context">The test context in which the variables will be set.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public override Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        foreach (var entry in Variables)
        {
            var key = entry.Key;
            var value = entry.Value;

            //check if the value is variable or function (and resolve it if yes)
            value = context.ReplaceDynamicContentInString(value);

            Log.LogInformation("Setting variable: {Key} to value: {Value}", key, value);

            context.SetVariable(key, value);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///     Builder class for creating instances of <see cref="CreateVariablesAction" />.
    ///     Provides methods for adding variables and their values to the action.
    /// </summary>
    public sealed class Builder : AbstractAsyncTestActionBuilder<IAsyncTestAction, dynamic>
    {
        internal readonly IDictionary<string, string> Variables = new Dictionary<string, string>();

        /// <summary>
        ///     Creates a new Builder with the specified variable and value.
        /// </summary>
        /// <param name="variableName">The name of the variable to create.</param>
        /// <param name="value">The value of the variable to create.</param>
        /// <returns>A new instance of the Builder.</returns>
        public static Builder CreateVariable(string variableName, string value)
        {
            var builder = new Builder();
            builder.Variable(variableName, value);
            return builder;
        }

        /// <summary>
        ///     Creates a new instance of the Builder for creating variables.
        /// </summary>
        /// <returns>A new instance of the Builder.</returns>
        public static Builder CreateVariables()
        {
            return new Builder();
        }

        /// <summary>
        ///     A method for adding a variable to the builder.
        /// </summary>
        /// <param name="variableName">The name of the variable to add.</param>
        /// <param name="value">The value of the variable to add.</param>
        /// <returns>A reference to the builder, for method chaining.</returns>
        public Builder Variable(string variableName, string value)
        {
            Variables[variableName] = value;
            return this;
        }

        /// <summary>
        ///     Builds an instance of the <see cref="CreateVariablesAction" /> class based on the configured parameters.
        /// </summary>
        /// <returns>A new instance of the <see cref="CreateVariablesAction" /> class.</returns>
        public override CreateVariablesAction Build()
        {
            return new CreateVariablesAction(this);
        }
    }
}
