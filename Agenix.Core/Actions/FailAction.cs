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
using Agenix.Api.Exceptions;

namespace Agenix.Core.Actions;

/// <summary>
///     Represents an action that intentionally fails and interrupts test execution.
/// </summary>
/// <remarks>
///     The FailAction class is designed to generate an error in tests, using a predefined or customized message.
///     This can be useful for testing error handling and response mechanisms.
/// </remarks>
public class FailAction(FailAction.Builder builder) : AbstractTestActionAsync("fail", builder)
{
    /// <summary>
    ///     Retrieves the message associated with the FailAction.
    /// </summary>
    /// <returns>
    ///     A string representing the error message configured for the FailAction.
    /// </returns>
    public string Message { get; } = builder.Message;

    /// <summary>
    ///     Executes the FailAction, throwing an exception with a dynamically replaced message.
    /// </summary>
    /// <param name="context">The test context containing dynamic content to be replaced in the message.</param>
    /// <param name="cancellationToken">An optional token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation execution.</returns>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when the FailAction is executed, containing the dynamically replaced
    ///     message.
    /// </exception>
    public override Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        throw new AgenixSystemException(context.ReplaceDynamicContentInString(Message));
    }

    /// <summary>
    ///     A builder class for constructing instances of the FailAction class.
    /// </summary>
    /// <remarks>
    ///     This class allows for the configuration of a fail action, which is intended to interrupt test execution
    ///     with a generated error.
    /// </remarks>
    public class Builder : AbstractAsyncTestActionBuilder<IAsyncTestAction, dynamic>
    {
        /// <summary>
        ///     Represents the message associated with an action, typically used to convey information
        ///     or describe errors during the execution of the action.
        /// </summary>
        internal string Message = "Generated error to interrupt test execution";

        /// <summary>
        ///     Configures and initializes a builder for constructing a fail action that interrupts test execution with an error.
        /// </summary>
        /// <returns>
        ///     Returns a builder instance configured to create a fail action for test interruption.
        /// </returns>
        public static Builder Fail()
        {
            return new Builder();
        }

        /// <summary>
        ///     Executes an action that forces a failure in the test sequence by throwing an exception.
        /// </summary>
        /// <param name="message">The failure message that describes the reason for the test failure.</param>
        /// <returns>A configured instance of the FailAction builder.</returns>
        /// <exception cref="AgenixSystemException">
        ///     Thrown to indicate the failure as part of the test execution process, including
        ///     the provided message.
        /// </exception>
        public static Builder Fail(string message)
        {
            var builder = new Builder { Message = message };
            return builder;
        }

        /// <summary>
        ///     Updates the message content for the builder.
        /// </summary>
        /// <param name="newMessage">The new content of the message to be set.</param>
        /// <returns>A reference to the updated builder instance.</returns>
        public Builder WithMessage(string newMessage)
        {
            Message = newMessage;
            return this;
        }

        /// <summary>
        ///     Builds and returns a new instance of the FailAction class.
        /// </summary>
        /// <returns>
        ///     A new instance of the <see cref="FailAction" /> class with any specified properties.
        /// </returns>
        public override FailAction Build()
        {
            return new FailAction(this);
        }
    }
}
