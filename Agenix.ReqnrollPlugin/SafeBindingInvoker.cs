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
