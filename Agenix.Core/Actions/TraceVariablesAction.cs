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
