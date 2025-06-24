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
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Core.Actions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Actions;

/// <summary>
///     Represents a unit test class for testing asynchronous actions derived from AbstractAsyncTestAction.
/// </summary>
public class AbstractAsyncTestActionTest : AbstractNUnitSetUp
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AbstractAsyncTestActionTest));

    [Test]
    public async Task TestOnSuccess()
    {
        var result = new TaskCompletionSource<bool>();

        var action = new CustomAsyncTestActionSuccess(result);

        action.Execute(Context);

        ClassicAssert.IsTrue(await TaskExtensions.TimeoutAfter(result.Task, TimeSpan.FromMilliseconds(1000)));
    }

    [Test]
    public void TestOnError()
    {
        var result = new TaskCompletionSource<bool>();

        var action = new CustomAsyncTestActionError(result);

        // Execute the action
        var exception = Assert.ThrowsAsync<AgenixSystemException>(async () =>
            {
                action.Execute(Context);
                await TaskExtensions.TimeoutAfter(result.Task, TimeSpan.FromMilliseconds(1000));
            },
            "Expected CoreSystemException with specific message.");

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Failed!"));
    }

    private static class TaskExtensions
    {
        public static async Task<T> TimeoutAfter<T>(Task<T> task, TimeSpan timeout)
        {
            if (task == await Task.WhenAny(task, Task.Delay(timeout)))
            {
                return await task;
            }

            throw new TimeoutException();
        }
    }

    private class CustomAsyncTestActionError(TaskCompletionSource<bool> result) : AbstractAsyncTestAction
    {
        public override Task DoExecuteAsync(TestContext context)
        {
            return Task.Run(() => throw new AgenixSystemException("Failed!"));
        }

        public override void OnSuccess(TestContext context)
        {
            result.SetResult(false);
        }

        public override void OnError(TestContext context, Exception error)
        {
            result.SetException(error);
        }
    }

    private class CustomAsyncTestActionSuccess(TaskCompletionSource<bool> result) : AbstractAsyncTestAction
    {
        public override Task DoExecuteAsync(TestContext context)
        {
            return Task.Run(() => { Log.LogInformation("Success!"); });
        }

        public override void OnSuccess(TestContext context)
        {
            result.SetResult(true);
        }

        public override void OnError(TestContext context, Exception error)
        {
            result.SetException(error);
        }
    }
}
