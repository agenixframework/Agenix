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
using System.Threading;
using System.Threading.Tasks;
using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Actions;

/// <summary>
///     Represents an abstract base class for asynchronous test actions.
///     This class provides the basic structure for executing asynchronous operations
///     within a test and handles the completion logic.
/// </summary>
public abstract class AbstractAsyncTestAction : AbstractTestActionAsync, ICompletable
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AbstractAsyncTestAction));
    private Task _finished;

    /// Determines if the asynchronous test action is completed.
    /// @param context The test context which provides required execution details.
    /// @return true if the action has finished executing or is disabled; otherwise, false.
    /// /
    public bool IsDone(TestContext context)
    {
        return _finished?.IsCompleted ?? false;
    }

    /// Executes the asynchronous test action within the provided test context.
    /// This method sets up a task for the action completion logic and executes the asynchronous operation.
    /// <param name="context">The test context which provides required execution details and tracks exceptions.</param>
    /// <param name="cancellationToken"></param>
    public override async Task DoExecute(TestContext context, CancellationToken cancellationToken = default)
    {
        _finished = ExecuteWithHandling(context);
        await _finished;
    }

    private async Task ExecuteWithHandling(TestContext context)
    {
        try
        {
            await DoExecuteAsync(context);
            await OnSuccess(context);
        }
        catch (Exception e)
        {
            Log.LogWarning(e, "Async test action execution raised error");

            var exception = e is AgenixSystemException ? e : new AgenixSystemException(e.Message, e);
            context.AddException((AgenixSystemException)exception);
            await OnError(context, exception);
            throw new AgenixSystemException(exception.Message, exception);
        }
    }


    /// Executes the asynchronous logic for the defined test action.
    /// <param name="context">The test context providing the necessary execution details for the action.</param>
    /// <return>A task that represents the asynchronous execution of the action.</return>
    public abstract Task DoExecuteAsync(TestContext context);

    /**
     * Optional validation step after async test action performed with success.
     * @param context
     */
    protected virtual async Task OnSuccess(TestContext context)
    {
        await Task.CompletedTask;
    }

    /**
     * Optional validation step after async test action performed with success.
     * @param context
     */
    protected virtual async Task OnError(TestContext context, Exception error)
    {
        await Task.CompletedTask;
    }
}
