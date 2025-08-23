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
using System.Collections.Concurrent;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.ReqnrollPlugin.EventArguments;
using Microsoft.Extensions.Logging;
using Reqnroll;

namespace Agenix.ReqnrollPlugin;

/// <summary>
///     Provides methods, events, and delegates for managing the lifecycle of the Agenix plugin,
///     including initialization and test run start/finish events.
/// </summary>
public class AgenixAddIn
{
    /// <summary>
    ///     Represents a delegate for handling events that occur when a feature has finished executing.
    ///     Provides the sender of the event and the associated event arguments.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data associated with the finished feature, containing details like the test case runner.</param>
    public delegate void FeatureFinishedHandler(object sender, TestCaseFinishedEventArgs e);

    /// <summary>
    ///     Represents the delegate for handling events when a feature starts execution,
    ///     passing the sender information and event arguments containing test case details.
    /// </summary>
    public delegate void FeatureStartedHandler(object sender, TestCaseStartedEventArgs e);

    /// <summary>
    ///     Represents the method that will handle the initializing event in the Agenix Reqnroll plugin.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The event data associated with the initializing process.</param>
    public delegate void InitializingHandler(object sender, InitializingEventArgs e);

    /// <summary>
    ///     Defines a delegate for handling events triggered when a test run is completed.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An instance of <see cref="RunFinishedEventArgs" /> containing run completion event data.</param>
    public delegate void RunFinishedHandler(object sender, RunFinishedEventArgs e);

    /// <summary>
    ///     Represents a delegate that is invoked when a test run starts, providing sender information and run-related event
    ///     arguments.
    /// </summary>
    public delegate void RunStartedHandler(object sender, RunStartedEventArgs e);

    /// <summary>
    ///     Represents a delegate for handling the completion of a scenario during test execution.
    ///     Used to define methods invoked when a scenario is marked as finished,
    ///     passing the sender and relevant test case information.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An instance of <see cref="TestCaseFinishedEventArgs" /> containing data about the finished scenario.</param>
    public delegate void ScenarioFinishedHandler(object sender, TestCaseFinishedEventArgs e);

    /// <summary>
    ///     Represents a delegate that handles the event triggered when a scenario starts.
    ///     Provides event arguments containing information about the initiated test case.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="TestCaseStartedEventArgs" /> containing details of the started scenario.</param>
    public delegate void ScenarioStartedHandler(object sender, TestCaseStartedEventArgs e);

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AgenixAddIn));

    private static ConcurrentDictionary<FeatureInfo, IAsyncTestCaseRunner> FeatureTestCaseRunners { get; } =
        new(new FeatureInfoEqualityComparer());

    private static ConcurrentDictionary<FeatureInfo, int> FeatureThreadCount { get; } =
        new(new FeatureInfoEqualityComparer());

    private static ConcurrentDictionary<ScenarioInfo, IAsyncTestCaseRunner> ScenarioTestCaseRunners { get; } = new();

    /// <summary>
    ///     Represents the shared context for managing test-related configurations,
    ///     dependencies, and runtime information used during the execution of tests.
    /// </summary>
    public static TestContext TestContext { get; internal set; }

    /// <summary>
    ///     Retrieves the asynchronous test case runner associated with the specified feature context.
    /// </summary>
    /// <param name="context">The feature context containing information about the current feature.</param>
    /// <returns>
    ///     The <see cref="IAsyncTestCaseRunner" /> instance associated with the feature context, or <c>null</c> if no
    ///     runner is found.
    /// </returns>
    public static IAsyncTestCaseRunner GetFeatureCaseRunner(FeatureContext context)
    {
        if (context != null && FeatureTestCaseRunners.TryGetValue(context.FeatureInfo, out var reporter))
        {
            return reporter;
        }

        return null;
    }

    /// <summary>
    ///     Associates a specified feature with a test case runner instance for managing test case execution.
    /// </summary>
    /// <param name="context">The context containing information about the current feature.</param>
    /// <param name="testCaseRunner">An instance of <see cref="ITestCaseRunner" /> to be associated with the specified feature.</param>
    internal static void SetFeatureTestCaseRunner(FeatureContext context, IAsyncTestCaseRunner testCaseRunner)
    {
        FeatureTestCaseRunners[context.FeatureInfo] = testCaseRunner;
        FeatureThreadCount[context.FeatureInfo] = 1;
    }

    /// <summary>
    ///     Removes the association of a specified feature with a test case runner instance.
    /// </summary>
    /// <param name="context">The context containing information about the feature whose association is being removed.</param>
    /// <param name="testCaseRunner">
    ///     An instance of <see cref="ITestCaseRunner" /> to be disassociated from the specified
    ///     feature.
    /// </param>
    internal static void RemoveFeatureTestCaseRunner(FeatureContext context, ref IAsyncTestCaseRunner testCaseRunner)
    {
        FeatureTestCaseRunners.TryRemove(context.FeatureInfo, out testCaseRunner);
        FeatureThreadCount.TryRemove(context.FeatureInfo, out _);
    }

    /// <summary>
    ///     Increments the thread count associated with a specific feature in the provided feature context.
    /// </summary>
    /// <param name="context">The feature context containing information about the associated feature.</param>
    /// <returns>The updated thread count for the specified feature.</returns>
    internal static int IncrementFeatureThreadCount(FeatureContext context)
    {
        return FeatureThreadCount[context.FeatureInfo]
            = FeatureThreadCount.TryGetValue(context.FeatureInfo, out var value) ? value + 1 : 1;
    }

    /// <summary>
    ///     Decrements the thread count associated with the specified feature context.
    ///     If the feature context has no current thread count, the value is set to 0.
    /// </summary>
    /// <param name="context">The feature context whose thread count is to be decremented.</param>
    /// <returns>The updated thread count for the specified feature context.</returns>
    internal static int DecrementFeatureThreadCount(FeatureContext context)
    {
        return FeatureThreadCount[context.FeatureInfo]
            = FeatureThreadCount.TryGetValue(context.FeatureInfo, out var value) ? value - 1 : 0;
    }

    /// <summary>
    ///     Retrieves the test case runner instance associated with the specified scenario context.
    /// </summary>
    /// <param name="context">The context containing information about the current scenario.</param>
    /// <returns>
    ///     An instance of <see cref="ITestCaseRunner" /> associated with the specified scenario context, or null if no
    ///     runner is found.
    /// </returns>
    public static IAsyncTestCaseRunner GetScenarioTestCaseRunner(ScenarioContext context)
    {
        if (context != null && ScenarioTestCaseRunners.TryGetValue(context.ScenarioInfo, out var runner))
        {
            return runner;
        }

        return null;
    }

    /// <summary>
    ///     Associates a specified scenario with a test case runner instance for managing test scenario execution.
    /// </summary>
    /// <param name="context">The context containing information about the current scenario.</param>
    /// <param name="testCaseRunner">
    ///     An instance of <see cref="ITestCaseRunner" /> to be associated with the specified
    ///     scenario.
    /// </param>
    internal static void SetScenarioTestReporter(ScenarioContext context, IAsyncTestCaseRunner testCaseRunner)
    {
        ScenarioTestCaseRunners[context.ScenarioInfo] = testCaseRunner;
    }

    /// <summary>
    ///     Removes the association between a specified scenario and its test reporter instance,
    ///     ceasing any further management or execution handling for the scenario.
    /// </summary>
    /// <param name="context">The context providing information about the current scenario.</param>
    /// <param name="testCaseRunner">
    ///     An instance of <see cref="ITestCaseRunner" /> previously associated with the specified
    ///     scenario.
    /// </param>
    internal static void RemoveScenarioTestReporter(ScenarioContext context, ref IAsyncTestCaseRunner testCaseRunner)
    {
        ScenarioTestCaseRunners.TryRemove(context.ScenarioInfo, out testCaseRunner);
    }

    /// <summary>
    ///     Event triggered during the initialization phase of the Agenix Reqnroll plugin.
    /// </summary>
    public static event InitializingHandler Initializing;

    /// <summary>
    ///     Invokes the Initializing event with the provided event data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArg">An instance of <see cref="InitializingEventArgs" /> that contains the event data.</param>
    internal static void OnInitializing(object sender, InitializingEventArgs eventArg)
    {
        try
        {
            Initializing?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnInitializingName} event handler: {Exception}", nameof(OnInitializing), exp);
        }
    }

    /// <summary>
    ///     Event triggered before a test run starts.
    /// </summary>
    public static event RunStartedHandler BeforeRunStarted;

    /// <summary>
    ///     Event that occurs after the test run has started.
    /// </summary>
    public static event RunStartedHandler AfterRunStarted;

    /// <summary>
    ///     Invokes the BeforeRunStarted event with the provided event data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArg">An instance of <see cref="RunStartedEventArgs" /> that contains the event data.</param>
    internal static void OnBeforeRunStarted(object sender, RunStartedEventArgs eventArg)
    {
        try
        {
            BeforeRunStarted?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnBeforeRunStartedName} event handler: {Exception}", nameof(OnBeforeRunStarted),
                exp);
        }
    }

    /// <summary>
    ///     Invokes the AfterRunStarted event with the provided event data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArg">An instance of <see cref="RunStartedEventArgs" /> that contains the event data.</param>
    internal static void OnAfterRunStarted(object sender, RunStartedEventArgs eventArg)
    {
        try
        {
            AfterRunStarted?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnAfterRunStartedName} event handler: {Exception}", nameof(OnAfterRunStarted),
                exp);
        }
    }

    /// <summary>
    ///     Event triggered before a run finishes.
    /// </summary>
    public static event RunFinishedHandler BeforeRunFinished;

    /// <summary>
    ///     Event that is triggered after the execution of a run has been completed.
    /// </summary>
    public static event RunFinishedHandler AfterRunFinished;

    /// <summary>
    ///     Invokes the BeforeRunFinished event with the provided event data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArg">An instance of <see cref="RunFinishedEventArgs" /> that contains the event data.</param>
    internal static void OnBeforeRunFinished(object sender, RunFinishedEventArgs eventArg)
    {
        try
        {
            BeforeRunFinished?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnBeforeRunFinishedName} event handler: {Exception}", nameof(OnBeforeRunFinished)
                , exp);
        }
    }

    /// <summary>
    ///     Invokes the AfterRunFinished event with the provided event data.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="eventArg">An instance of <see cref="RunFinishedEventArgs" /> that contains the event data.</param>
    internal static void OnAfterRunFinished(object sender, RunFinishedEventArgs eventArg)
    {
        try
        {
            AfterRunFinished?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnAfterRunFinishedName} event handler: {Exception}", nameof(OnAfterRunFinished),
                exp);
        }
    }

    /// <summary>
    ///     Represents an event that is triggered before a feature starts execution.
    /// </summary>
    public static event FeatureStartedHandler BeforeFeatureStarted;

    /// <summary>
    ///     Event triggered after a feature has started execution.
    /// </summary>
    public static event FeatureStartedHandler AfterFeatureStarted;

    /// <summary>
    ///     Invokes the event handler for the BeforeFeatureStarted event prior to the start of a feature execution.
    /// </summary>
    /// <param name="sender">The source of the event, typically the object raising the event.</param>
    /// <param name="eventArg">
    ///     An instance of <see cref="TestCaseStartedEventArgs" /> containing the details and context of the
    ///     feature being started.
    /// </param>
    internal static void OnBeforeFeatureStarted(object sender, TestCaseStartedEventArgs eventArg)
    {
        try
        {
            BeforeFeatureStarted?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnBeforeFeatureStartedName} event handler: {Exception}", nameof(
                    OnBeforeFeatureStarted), exp);
        }
    }

    /// <summary>
    ///     Invokes the AfterFeatureStarted event handler after a feature execution is started, allowing custom logic to be
    ///     executed post-feature initialization.
    /// </summary>
    /// <param name="sender">The source of the event, typically null if not directly triggered by a specific source.</param>
    /// <param name="eventArg">
    ///     An instance of <see cref="TestCaseStartedEventArgs" /> containing data related to the feature
    ///     that has just started.
    /// </param>
    internal static void OnAfterFeatureStarted(object sender, TestCaseStartedEventArgs eventArg)
    {
        try
        {
            AfterFeatureStarted?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnAfterFeatureStartedName} event handler: {Exception}", nameof(
                    OnAfterFeatureStarted), exp);
        }
    }

    /// <summary>
    ///     Event triggered before a feature has finished execution.
    /// </summary>
    public static event FeatureFinishedHandler BeforeFeatureFinished;

    /// <summary>
    ///     Event triggered after a feature has finished executing.
    /// </summary>
    public static event FeatureFinishedHandler AfterFeatureFinished;

    /// <summary>
    ///     Triggers the event before a feature has finished executing and handles any associated operations or exceptions.
    /// </summary>
    /// <param name="sender">The source of the event, typically the instance raising the event or null if unavailable.</param>
    /// <param name="eventArg">The event arguments containing information about the feature and its test case execution.</param>
    internal static void OnBeforeFeatureFinished(object sender, TestCaseFinishedEventArgs eventArg)
    {
        try
        {
            BeforeFeatureFinished?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnBeforeFeatureFinishedName} event handler: {Exception}", nameof(
                    OnBeforeFeatureFinished), exp);
        }
    }

    /// <summary>
    ///     Triggers the AfterFeatureFinished event when a feature execution is completed.
    /// </summary>
    /// <param name="sender">The source of the event, typically null when called internally.</param>
    /// <param name="eventArg">
    ///     An instance of <see cref="TestCaseFinishedEventArgs" /> containing information about the
    ///     finished feature test case.
    /// </param>
    internal static void OnAfterFeatureFinished(object sender, TestCaseFinishedEventArgs eventArg)
    {
        try
        {
            AfterFeatureFinished?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnAfterFeatureFinishedName} event handler: {Exception}", nameof(
                    OnAfterFeatureFinished), exp);
        }
    }

    /// <summary>
    ///     Event triggered before a scenario starts.
    /// </summary>
    public static event ScenarioStartedHandler BeforeScenarioStarted;

    /// <summary>
    ///     Event triggered after a scenario has started.
    /// </summary>
    public static event ScenarioStartedHandler AfterScenarioStarted;

    /// <summary>
    ///     Invokes the registered handlers for the event raised before a scenario is started.
    /// </summary>
    /// <param name="sender">The source object of the event, typically the test execution context or framework.</param>
    /// <param name="eventArg">
    ///     An instance of <see cref="TestCaseStartedEventArgs" /> containing information about the scenario
    ///     about to start.
    /// </param>
    internal static void OnBeforeScenarioStarted(object sender, TestCaseStartedEventArgs eventArg)
    {
        try
        {
            BeforeScenarioStarted?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnBeforeScenarioStartedName} event handler: {Exception}", nameof(
                    OnBeforeScenarioStarted), exp);
        }
    }

    /// <summary>
    ///     Invokes the AfterScenarioStarted event after a scenario starts execution. Handles any exceptions that occur during
    ///     event invocation.
    /// </summary>
    /// <param name="sender">The source of the event invocation, typically the object that starts the scenario.</param>
    /// <param name="eventArg">
    ///     The event arguments containing details about the scenario execution, including the associated
    ///     test case runner.
    /// </param>
    internal static void OnAfterScenarioStarted(object sender, TestCaseStartedEventArgs eventArg)
    {
        try
        {
            AfterScenarioStarted?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnAfterScenarioStartedName} event handler: {Exception}", nameof(
                    OnAfterScenarioStarted), exp);
        }
    }

    /// <summary>
    ///     Event triggered before a scenario's execution is finalized.
    /// </summary>
    public static event ScenarioFinishedHandler BeforeScenarioFinished;

    /// <summary>
    ///     Event triggered after a scenario has finished executing.
    /// </summary>
    public static event ScenarioFinishedHandler AfterScenarioFinished;

    /// <summary>
    ///     Invokes the BeforeScenarioFinished event before completing the execution of a scenario.
    /// </summary>
    /// <param name="sender">The source of the event being triggered.</param>
    /// <param name="eventArg">
    ///     An instance of <see cref="TestCaseStartedEventArgs" /> containing information about the scenario
    ///     being processed.
    /// </param>
    internal static void OnBeforeScenarioFinished(object sender, TestCaseFinishedEventArgs eventArg)
    {
        try
        {
            BeforeScenarioFinished?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnBeforeScenarioFinishedName} event handler: {Exception}", nameof(
                    OnBeforeScenarioFinished), exp);
        }
    }

    /// <summary>
    ///     Invokes the AfterScenarioFinished event after a scenario is completed, ensuring event handlers are executed.
    ///     Logs any exceptions that occur during event handler execution.
    /// </summary>
    /// <param name="sender">The source of the event, typically the object that triggered the scenario completion.</param>
    /// <param name="eventArg">
    ///     An instance of <see cref="TestCaseStartedEventArgs" /> containing event data related to the
    ///     scenario execution.
    /// </param>
    internal static void OnAfterScenarioFinished(object sender, TestCaseFinishedEventArgs eventArg)
    {
        try
        {
            AfterScenarioFinished?.Invoke(sender, eventArg);
        }
        catch (Exception exp)
        {
            Log.LogError(exp,
                "Exception occured in {OnAfterScenarioFinishedName} event handler: {Exception}", nameof(
                    OnAfterScenarioFinished), exp);
        }
    }
}
