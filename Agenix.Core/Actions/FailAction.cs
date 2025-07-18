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
public class FailAction(FailAction.Builder builder) : AbstractTestAction("fail", builder)
{
    private readonly string _message = builder.message;

    /// <summary>
    ///     Executes the specified FailAction in the given TestContext.
    /// </summary>
    /// <param name="context">The TestContext in which the FailAction is executed.</param>
    public override void DoExecute(TestContext context)
    {
        throw new AgenixSystemException(context.ReplaceDynamicContentInString(_message));
    }

    /// <summary>
    ///     Retrieves the message associated with the FailAction.
    /// </summary>
    /// <returns>
    ///     A string representing the error message configured for the FailAction.
    /// </returns>
    public string GetMessage()
    {
        return _message;
    }

    /// <summary>
    ///     A builder class for constructing instances of the FailAction class.
    /// </summary>
    /// <remarks>
    ///     This class allows for the configuration of a fail action, which is intended to interrupt test execution
    ///     with a generated error.
    /// </remarks>
    public class Builder : AbstractTestActionBuilder<ITestAction, dynamic>
    {
        public string message = "Generated error to interrupt test execution";

        /// <summary>
        ///     Represents an action that causes a failure in test execution.
        /// </summary>
        /// <param name="builder">The builder used to configure the fail action.</param>
        /// <returns>
        ///     An instance of the <see cref="FailAction" /> class configured by the provided builder.
        /// </returns>
        public static Builder Fail()
        {
            return new Builder();
        }

        /// <summary>
        ///     Represents an action that causes a failure in test execution.
        /// </summary>
        /// <param name="builder">The builder used to configure the fail action.</param>
        /// <returns>
        ///     An instance of the <see cref="FailAction" /> class configured by the provided builder.
        /// </returns>
        /// <param name="context">The context in which the fail action is executed.</param>
        /// <returns>
        ///     No return value.
        /// </returns>
        /// <param name="message">The failure message to be retrieved.</param>
        /// <returns>
        ///     The failure message associated with the fail action.
        /// </returns>
        public static Builder Fail(string message)
        {
            var builder = new Builder { message = message };
            return builder;
        }

        /// <summary>
        ///     Represents a message with specific properties.
        /// </summary>
        /// <param name="message">The content of the message.</param>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="timestamp">The time when the message was sent.</param>
        /// <returns>
        ///     A new instance of the <see cref="Message" /> class.
        /// </returns>
        public Builder Message(string message)
        {
            this.message = message;
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
