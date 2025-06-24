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
public abstract class AbstractAsyncTestAction : AbstractTestAction, ICompletable
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AbstractAsyncTestAction));
    private Task _finished;

    /// Determines if the asynchronous test action is completed.
    /// @param context The test context which provides required execution details.
    /// @return true if the action has finished executing or is disabled; otherwise, false.
    /// /
    public bool IsDone(TestContext context)
    {
        return (_finished?.IsCompleted ?? IsDisabled(context)) || IsDisabled(context);
    }

    /// Executes the asynchronous test action within the provided test context.
    /// This method sets up a task for the action completion logic and executes the asynchronous operation.
    /// <param name="context">The test context which provides required execution details and tracks exceptions.</param>
    public override void DoExecute(TestContext context)
    {
        var tcs = new TaskCompletionSource<TestContext>();


        // Set up completion logic
        tcs.Task.ContinueWith(task =>
        {
            if (task.Exception != null)
            {
                OnError(context, task.Exception.GetBaseException());
            }
            else if (context.HasExceptions())
            {
                OnError(context, context.GetExceptions()[0]);
            }
            else
            {
                OnSuccess(context);
            }
        });

        // Execute async action
        _finished = Task.Run(async () =>
        {
            try
            {
                await DoExecuteAsync(context);
            }
            catch (Exception e)
            {
                Log.LogWarning(e, "Async test action execution raised error");
                context.AddException(
                    (AgenixSystemException)(e is AgenixSystemException ? e : new AgenixSystemException(e.Message)));
            }
            finally
            {
                tcs.SetResult(context);
            }
        });
    }

    public abstract Task DoExecuteAsync(TestContext context);

    /**
     * Optional validation step after async test action performed with success.
     * @param context
     */
    public virtual void OnSuccess(TestContext context)
    {
    }

    /**
     * Optional validation step after async test action performed with success.
     * @param context
     */
    public virtual void OnError(TestContext context, Exception error)
    {
    }
}
