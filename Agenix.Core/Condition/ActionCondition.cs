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
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Condition;

/// <summary>
///     Represents a condition that executes a specified test action and determines
///     if the condition is satisfied based on the result of the action execution.
/// </summary>
/// <remarks>
///     The <c>ActionCondition</c> class extends <c>AbstractCondition</c> and operates
///     on a provided <c>ITestAction</c>. This action is tested to determine if the
///     condition is met. The class provides methods to set and retrieve the action,
///     evaluate satisfaction, and manage success or error messages. It also allows
///     retrieval and management of any exceptions caught during execution.
/// </remarks>
public class ActionCondition : AbstractCondition
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(ActionCondition));

    /// <summary>
    ///     Represents the action to be executed within the condition.
    /// </summary>
    private ITestAction _action;

    /// <summary>
    ///     Optional exception that is caught during the execution of an action.
    /// </summary>
    private Exception _caughtException;

    public ActionCondition()
    {
    }

    /// Represents a condition that executes a specified test action.
    /// This class extends AbstractCondition and provides functionality
    /// to determine if the condition is satisfied based on the execution
    /// of a test action. It includes methods to manage the test action,
    /// evaluate satisfaction, and provide success or error messages.
    /// /
    public ActionCondition(ITestAction action)
    {
        _action = action;
    }

    /// Evaluates whether the current condition is satisfied by executing a specified test action within the given context.
    /// If the action is successfully executed without exceptions, the condition is considered satisfied.
    /// Any exceptions encountered during execution are logged and result in an unsatisfied condition state.
    /// <param name="context">The context in which the test action is executed, providing necessary data and state.</param>
    /// <returns>
    ///     True if the test action executes successfully without exceptions, indicating the condition is satisfied;
    ///     otherwise, false.
    /// </returns>
    public override bool IsSatisfied(TestContext context)
    {
        if (_action == null)
        {
            return false;
        }

        try
        {
            _action.Execute(context);
        }
        catch (Exception e)
        {
            _caughtException = e;
            Log.LogWarning(
                $"Nested action did not perform as expected - {$"{e.GetType().Name}: {e.Message}"}");
            return false;
        }

        return true;
    }

    /// Retrieves the success message for the test action condition based on the given context.
    /// This message is generated when the test action is successfully executed as expected.
    /// <param name="context">The TestContext within which the test action is performed.</param>
    /// <return>A string indicating the successful execution of the test action.</return>
    public override string GetSuccessMessage(TestContext context)
    {
        return $"Test action condition success - action '{GetActionName()}' did perform as expected";
    }

    /// Returns an error message indicating that the test action condition
    /// was not performed as expected. If an exception was caught during
    /// the execution of the test action, the message will include the
    /// exception type and message. Otherwise, a generic message is returned.
    /// <param name="context">The context in which the test action was executed.</param>
    /// <return>A string containing the error message.</return>
    public override string GetErrorMessage(TestContext context)
    {
        return _caughtException != null
            ? $"Failed to check test action condition - action '{GetActionName()}' did not perform as expected: {_caughtException.GetType().Name}: {_caughtException.Message}"
            : $"Failed to check test action condition - action '{GetActionName()}' did not perform as expected";
    }

    /// Retrieves the name of the action associated with this condition.
    /// <return>The name of the test action if set; otherwise, "unknown".</return>
    private string GetActionName()
    {
        return _action?.Name ?? "unknown";
    }

    /// Retrieves the test action associated with this condition.
    /// <returns>The test action currently set in the condition.</returns>
    public ITestAction GetAction()
    {
        return _action;
    }

    /// Sets the test action for this condition.
    /// <param name="action">The test action to be set.</param>
    public void SetAction(ITestAction action)
    {
        _action = action;
    }

    /// Retrieves the exception that was caught during the execution of the test action if one occurred.
    /// <return>The exception caught during the execution of the action, or null if no exception was caught.</return>
    public Exception GetCaughtException()
    {
        return _caughtException;
    }

    /// Sets the exception that was caught during the execution of a test action for this condition.
    /// This method is used to store the exception encountered during the test action for later retrieval or error reporting.
    /// <param name="caughtException">
    ///     The exception to set, representing any error or issue encountered during the test action
    ///     execution.
    /// </param>
    public void SetCaughtException(Exception caughtException)
    {
        _caughtException = caughtException;
    }

    public override string ToString()
    {
        return $"ActionCondition{{action={_action}, caughtException={_caughtException}, name={GetName()}}}";
    }
}
