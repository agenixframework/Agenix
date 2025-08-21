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
using Agenix.Api;
using Agenix.Api.Report;

namespace Agenix.Core.Report;

/// <summary>
///     Basic implementation of <see cref="IAsyncTestListener" /> interface so that subclasses must not implement
///     all methods but only overwrite some listener methods.
/// </summary>
public abstract class AbstractAsyncTestListener : IAsyncTestListener
{
    /// <summary>
    ///     Invoked when a test case fails during execution. This method can be overridden
    ///     to provide custom behavior when a test fails.
    /// </summary>
    /// <param name="test">The instance of the test case that failed.</param>
    /// <param name="cause">The exception that caused the failure.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnTestFailure(IAsyncTestCase test, Exception cause)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Invoked when a test case is skipped during execution. This method can be overridden
    ///     to provide custom behavior when a test is skipped.
    /// </summary>
    /// <param name="test">The instance of the test case that was skipped.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnTestSkipped(IAsyncTestCase test)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Invoked when a test case starts execution. This method can be overridden
    ///     to provide custom behavior when a test begins running.
    /// </summary>
    /// <param name="test">The instance of the test case that is starting execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnTestStart(IAsyncTestCase test)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Invoked when a test case finishes its execution. This method can be overridden
    ///     to define custom behavior upon the completion of a test.
    /// </summary>
    /// <param name="test">The instance of the test case that has completed.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task OnTestFinish(IAsyncTestCase test)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Invoked when a test case succeeds during execution. This method can be overridden
    ///     to provide custom behavior when a test succeeds.
    /// </summary>
    /// <param name="test">The instance of the test case that succeeded.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnTestSuccess(IAsyncTestCase test)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    ///     Invoked when the execution of a test case ends. This method can be overridden
    ///     to implement custom behavior when a test case's execution is completed.
    /// </summary>
    /// <param name="test">The instance of the test case that has finished execution.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public virtual Task OnTestExecutionEnd(IAsyncTestCase test)
    {
        return Task.CompletedTask;
    }
}
