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

using System;
using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Validation.Matcher;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Container;

/// Represents an action container specifically designed to manage assertions
/// within test contexts. This class handles the execution of test actions
/// and validates expected exceptions during testing.
public class AssertContainer(AssertContainer.Builder builder) : AbstractActionContainer(builder.GetName() ?? "assert",
    builder.GetDescription(), builder.GetActions())
{
    /// Static logger instance for the AssertException class.
    /// /
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AssertContainer));

    /// Represents a nested test action builder specifically for test actions.
    /// /
    private readonly ITestActionBuilder<ITestAction> _action = builder._action;

    /// Represents the exception type that is subject to assertion.
    private readonly Type _exception = builder._exception;

    /// Stores a localized exception message for control purposes.
    private readonly string _message = builder._message;

    public override void DoExecute(TestContext context)
    {
        if (Log.IsEnabled(LogLevel.Debug))
        {
            Log.LogDebug("Assert container asserting exceptions of type {ExceptionName}", _exception.Name);
        }

        try
        {
            ExecuteAction(_action.Build(), context);
        }
        catch (Exception e)
        {
            Log.LogDebug(e, "Validating caught exception: ");

            if (!_exception.IsAssignableFrom(e.GetType()))
            {
                throw new ValidationException("Validation failed for asserted exception type - expected: '" +
                                              _exception.Name + "' but was: '" + e.GetType().Name + "'", e);
            }

            if (_message != null)
            {
                if (ValidationMatcherUtils.IsValidationMatcherExpression(_message))
                {
                    ValidationMatcherUtils.ResolveValidationMatcher("message", e.Message, _message, context);
                }
                else if (!context.ReplaceDynamicContentInString(_message).Equals(e.Message))
                {
                    throw new ValidationException("Validation failed for asserted exception message - expected: '" +
                                                  _message + "' but was: '" + e.Message + "'", e);
                }
            }

            Log.LogDebug("Asserted exception is as expected ({S}): {EMessage}", e.GetType().Name, e.Message);

            Log.LogDebug("Assert exception validation successful: All values OK");

            return;
        }

        throw new ValidationException("Missing asserted exception '" + _exception + "'");
    }

    /// Gets the action.
    /// @return the action
    /// /
    public ITestAction GetAction()
    {
        return _action.Build();
    }

    /// Gets the message to send.
    /// <return>the message</return>
    public string GetMessage()
    {
        return _message;
    }

    /// Gets the exception.
    /// <return>the exception</return>
    public Type GetException()
    {
        return _exception;
    }

    /// <summary>
    ///     Retrieves a test action from the assert exception container.
    /// </summary>
    /// <param name="index">The index of the test action to retrieve.</param>
    /// <returns>The test action associated with the specified index.</returns>
    public override ITestAction GetTestAction(int index)
    {
        return GetAction();
    }

    /// Gets the list of actions.
    /// <returns>List of actions in the form of ITestAction objects.</returns>
    public override List<ITestAction> GetActions()
    {
        return [GetAction()];
    }

    /// Builder class used for creating and configuring instances of AssertException.
    /// This class provides a fluent API for specifying actions and expected exceptions
    /// in the context of test execution.
    /// /
    public class Builder : AbstractExceptionContainerBuilder<AssertContainer, Builder>
    {
        internal ITestActionBuilder<ITestAction> _action;
        internal Type _exception = typeof(AgenixSystemException);
        internal string _message;

        /// Fluent API action building entry method used in C# DSL.
        /// @return A Builder instance for configuring the assert action.
        /// /
        public static Builder Assert()
        {
            return new Builder();
        }

        public override Builder Actions(params ITestActionBuilder<ITestAction>[] actions)
        {
            _action = actions[0];
            return base.Actions(actions[0]);
        }

        /// Specifies the type of exception that is expected to be caught during the execution of the test.
        /// <param name="exception">The Type of the exception to be caught.</param>
        /// <return>A Builder instance for further configuring the assert action.</return>
        public Builder Exception(Type exception)
        {
            _exception = exception;
            return this;
        }

        /// Represents an exception that can be used within the context of a test action in the Agenix framework.
        /// Provides mechanisms to configure the action and the expected exceptions.
        public Builder Exception(string type)
        {
            _exception = Type.GetType(type);
            if (_exception == null)
            {
                throw new AgenixSystemException($"Failed to instantiate exception class of type '{type}'");
            }

            return this;
        }

        /// Specifies the expected error message that should be contained in the exception during the execution of the test action.
        /// <param name="message">The expected error message in the exception.</param>
        /// <return>A Builder instance for further configuration of the assert action.</return>
        public Builder Message(string message)
        {
            _message = message;
            return this;
        }

        /// Sets the test action to execute during assert.
        /// <param name="action">The test action to execute.</param>
        /// <return>A Builder instance for further configuring the assert action.</return>
        public Builder Action(ITestAction action)
        {
            return Action(new FuncITestActionBuilder<ITestAction>(() => action));
        }

        /// Configures the action to be used within the AssertContainer.
        /// <param name="action">The delegate that defines the test action to configure.</param>
        /// <returns>The builder instance for chaining further configurations.</returns>
        public Builder Action(TestAction action)
        {
            return Action(new DelegatingTestAction(action));
        }

        /// Fluent API action building entry method used in C# DSL.
        /// <returns>A Builder instance for configuring the assert action.</returns>
        public Builder Action(ITestActionBuilder<ITestAction> builder)
        {
            return Actions(builder);
        }

        protected override AssertContainer DoBuild()
        {
            return new AssertContainer(this);
        }
    }
}
