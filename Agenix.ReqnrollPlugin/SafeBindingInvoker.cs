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
using System.Reflection;
using System.Threading.Tasks;
using Reqnroll;
using Reqnroll.Bindings;
using Reqnroll.Configuration;
using Reqnroll.ErrorHandling;
using Reqnroll.Infrastructure;
using Reqnroll.Tracing;

namespace Agenix.ReqnrollPlugin;

/// <summary>
///     Provides a safe invocation mechanism for binding executions within the Reqnroll framework.
///     Handles exceptions during invocation and manages test state for hook bindings.
/// </summary>
internal class SafeBindingInvoker : BindingInvoker
{
    public SafeBindingInvoker(ReqnrollConfiguration reqnrollConfiguration, IErrorProvider errorProvider,
        IBindingDelegateInvoker bindingDelegateInvoker)
        : base(reqnrollConfiguration, errorProvider, bindingDelegateInvoker)
    {
    }

    /// <summary>
    ///     Asynchronously invokes a binding within the Reqnroll framework while handling exceptions
    ///     and managing scenario context for hook bindings.
    /// </summary>
    /// <param name="binding">
    ///     The binding to invoke, which represents the behavior to be executed.
    /// </param>
    /// <param name="contextManager">
    ///     The context manager responsible for managing scenario and step contexts during execution.
    /// </param>
    /// <param name="arguments">
    ///     An array of arguments to be passed to the binding during invocation.
    /// </param>
    /// <param name="testTracer">
    ///     The test tracer used for logging and tracing the execution of bindings.
    /// </param>
    /// <param name="durationHolder">
    ///     The duration holder for tracking the execution time of the binding invocation.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation. The result of the task is the output
    ///     of the invoked binding, or null if the binding does not produce a value.
    /// </returns>
    public override async Task<object> InvokeBindingAsync(IBinding binding, IContextManager contextManager,
        object[] arguments, ITestTracer testTracer, DurationHolder durationHolder)
    {
        object result = null;

        try
        {
            result = await base.InvokeBindingAsync(binding, contextManager, arguments, testTracer, durationHolder);
        }
        catch (Exception ex)
        {
            PreserveStackTrace(ex);

            if (binding is IHookBinding == false)
            {
                throw;
            }

            var hookBinding = binding as IHookBinding;

            if (hookBinding.HookType == HookType.BeforeScenario
                || hookBinding.HookType == HookType.BeforeScenarioBlock
                || hookBinding.HookType == HookType.BeforeScenario
                || hookBinding.HookType == HookType.BeforeStep
                || hookBinding.HookType == HookType.AfterStep
                || hookBinding.HookType == HookType.AfterScenario
                || hookBinding.HookType == HookType.AfterScenarioBlock)
            {
                SetTestError(contextManager.ScenarioContext, ex);
            }
        }

        return result;
    }

    /// <summary>
    ///     Sets the test error information for the provided scenario context to reflect an encountered exception.
    ///     Updates the scenario execution status and associates the exception with the scenario context.
    /// </summary>
    /// <param name="context">
    ///     The scenario context in which the test error occurred. This context tracks the state of the scenario
    ///     being executed, including its execution status and any associated errors.
    /// </param>
    /// <param name="ex">
    ///     The exception that represents the error encountered during the execution of the scenario.
    /// </param>
    private static void SetTestError(ScenarioContext context, Exception ex)
    {
        if (context != null && context.TestError == null)
        {
            context.GetType().GetProperty("ScenarioExecutionStatus")
                ?.SetValue(context, ScenarioExecutionStatus.TestError);

            context.GetType().GetProperty("TestError")
                ?.SetValue(context, ex);
        }
    }

    /// <summary>
    ///     Preserves the stack trace of the specified exception by invoking the internal method
    ///     <c>InternalPreserveStackTrace</c> on the exception instance. This is used to retain
    ///     the original stack trace when re-throwing exceptions.
    /// </summary>
    /// <param name="ex">
    ///     The exception whose stack trace is to be preserved.
    /// </param>
    private static void PreserveStackTrace(Exception ex)
    {
        typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic)
            ?.Invoke(ex, new object[0]);
    }
}
