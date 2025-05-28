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
    : AbstractActionContainer(builder.GetName() ?? "catch", builder.GetDescription(), builder.GetActions())
{
    /// Static logger instance for the Catch class.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Catch));

    /// The type of exception that the Catch container is designed to catch.
    public string Exception { get; } = builder._exception;

    public override void DoExecute(TestContext context)
    {
        if (Log.IsEnabled(LogLevel.Debug)) Log.LogDebug("Catch container catching exceptions of type " + Exception);

        foreach (var actionBuilder in actions)
            try
            {
                ExecuteAction(actionBuilder.Build(), context);
            }
            catch (Exception e)
            {
                if (Exception != null && Exception.Equals(e.GetType().Name))
                {
                    Log.LogInformation("Caught exception " + e.GetType() + ": " + e.Message);
                    continue;
                }

                throw new AgenixSystemException(e.Message, e);
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

        protected override Catch DoBuild()
        {
            return new Catch(this);
        }
    }
}
