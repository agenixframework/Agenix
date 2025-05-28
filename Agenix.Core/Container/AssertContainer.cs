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
            Log.LogDebug($"Assert container asserting exceptions of type {_exception.Name}");

        try
        {
            ExecuteAction(_action.Build(), context);
        }
        catch (Exception e)
        {
            Log.LogDebug("Validating caught exception: {0}", e);

            if (!_exception.IsAssignableFrom(e.GetType()))
                throw new ValidationException("Validation failed for asserted exception type - expected: '" +
                                              _exception.Name + "' but was: '" + e.GetType().Name + "'", e);

            if (_message != null)
            {
                if (ValidationMatcherUtils.IsValidationMatcherExpression(_message))
                    ValidationMatcherUtils.ResolveValidationMatcher("message", e.Message, _message, context);
                else if (!context.ReplaceDynamicContentInString(_message).Equals(e.Message))
                    throw new ValidationException("Validation failed for asserted exception message - expected: '" +
                                                  _message + "' but was: '" + e.Message + "'", e);
            }

            Log.LogDebug($"Asserted exception is as expected ({e.GetType().Name}): {e.Message}");

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
                throw new AgenixSystemException($"Failed to instantiate exception class of type '{type}'");
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
