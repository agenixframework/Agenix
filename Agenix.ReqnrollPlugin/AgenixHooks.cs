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
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Core;
using Agenix.Core.Annotations;
using Agenix.ReqnrollPlugin.EventArguments;
using Microsoft.Extensions.Logging;
using Reqnroll;

namespace Agenix.ReqnrollPlugin;

/// <summary>
///     Represents the hooks for handling lifecycle events within the test framework
///     integration provided by the Agenix Reqnroll plugin.
/// </summary>
[Binding]
public class AgenixHooks
{
    private const string SuiteName = "cucumber-suite";
    private static Core.Agenix _agenix;

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AgenixAddIn));

    /// <summary>
    ///     Executes the setup logic required before a test run starts. This method initializes the
    ///     <see cref="Core.Agenix" /> instance, triggers the necessary setup events, and configures
    ///     the runtime test context.
    /// </summary>
    [BeforeTestRun(Order = -20000)]
    public static void BeforeTestRun()
    {
        try
        {
            _agenix = Initialize();

            var eventArg = new RunStartedEventArgs(_agenix.AgenixContext);
            AgenixAddIn.OnBeforeRunStarted(typeof(AgenixHooks), eventArg);

            if (!eventArg.Canceled)
            {
                AgenixAddIn.OnAfterRunStarted(null, new RunStartedEventArgs(_agenix.AgenixContext));
            }
        }
        catch (Exception exp)
        {
            Log.LogError(exp, exp.Message);
        }
    }

    /// <summary>
    ///     Executes the teardown logic required after a test run completes. This method finalizes the
    ///     <see cref="Core.Agenix" /> instance, invokes cleanup events, and ensures proper disposal
    ///     of the runtime test context and related resources.
    /// </summary>
    [AfterTestRun(Order = 20000)]
    public static void AfterTestRun()
    {
        try
        {
            if (_agenix.AgenixContext != null)
            {
                var eventArg = new RunFinishedEventArgs(_agenix.AgenixContext);
                AgenixAddIn.OnBeforeRunFinished(null, eventArg);

                if (!eventArg.Canceled)
                {
                    _agenix.AfterSuite(SuiteName).ConfigureAwait(false).GetAwaiter().GetResult();
                    _agenix.AgenixContext.Close();
                    AgenixInstanceManager.Reset();

                    AgenixAddIn.OnAfterRunFinished(null,
                        new RunFinishedEventArgs(_agenix.AgenixContext));
                }
                else
                {
                    _agenix.AfterSuite(SuiteName).ConfigureAwait(false).GetAwaiter().GetResult();
                    _agenix.AgenixContext.Close();
                    AgenixInstanceManager.Reset();
                }
            }
        }
        catch (Exception exp)
        {
            Log.LogError(exp, exp.Message);
        }
    }

    /// <summary>
    ///     Executes the setup logic required before a scenario starts. This method prepares the necessary
    ///     test context, initializes resources specific to the scenario, and triggers the associated lifecycle
    ///     event for processing.
    /// </summary>
    /// <param name="featureContext">The context containing information about the feature being executed.</param>
    /// <param name="scenarioContext">The context containing information about the scenario being executed.</param>
    [BeforeScenario(Order = -20000)]
    public void BeforeScenario(FeatureContext featureContext, ScenarioContext scenarioContext)
    {
        try
        {
            if (_agenix != null)
            {
                var testContext = _agenix.AgenixContext.CreateTestContext();
                var currentTestCaseRunner = TestCaseRunnerFactory.CreateRunner(testContext);
                var eventArg = new TestCaseStartedEventArgs(currentTestCaseRunner, featureContext, scenarioContext);
                AgenixAddIn.OnBeforeScenarioStarted(this, eventArg);

                if (!eventArg.Canceled)
                {
                    var bindingInstanceProvider = scenarioContext.ScenarioContainer.Resolve<BindingInstanceProvider>();

                    foreach (var instance in bindingInstanceProvider.GetAllCachedBindings().Values)
                    {
                        Log.LogDebug("Found binding instance: {FullName}", instance.GetType().FullName);
                        AgenixAnnotations.InjectAll(instance, _agenix, testContext);
                        AgenixAnnotations.InjectTestRunner(instance, currentTestCaseRunner);
                    }

                    currentTestCaseRunner.SetName(scenarioContext.ScenarioInfo.Title);
                    currentTestCaseRunner.SetDescription(scenarioContext.ScenarioInfo.Description);
                    currentTestCaseRunner.SetNamespaceName(featureContext.FeatureInfo.FolderPath);
                    currentTestCaseRunner.Start();
                    AgenixAddIn.SetScenarioTestReporter(scenarioContext, currentTestCaseRunner);

                    AgenixAddIn.OnAfterScenarioStarted(this,
                        new TestCaseStartedEventArgs(currentTestCaseRunner, featureContext, scenarioContext));
                }
            }
        }
        catch (Exception exp)
        {
            Log.LogError(exp, exp.Message);
        }
    }

    /// <summary>
    ///     Executes the cleanup logic after a scenario completes. This method evaluates the scenario's execution status,
    ///     marks the test case with appropriate results, and raises any relevant events for handling the scenario completion.
    /// </summary>
    /// <param name="featureContext">
    ///     Provides the context of the feature within which the scenario was executed, enabling access to shared data and
    ///     metadata.
    /// </param>
    /// <param name="scenarioContext">
    ///     Provides the context of the scenario being executed, including execution status, test results, and shared
    ///     scenario-specific data.
    /// </param>
    [AfterScenario(Order = 20000)]
    public void AfterScenario(FeatureContext featureContext, ScenarioContext scenarioContext)
    {
        try
        {
            var currentTestCaseRunner = AgenixAddIn.GetScenarioTestCaseRunner(scenarioContext);

            if (currentTestCaseRunner?.GetTestCase() != null)
            {
                switch (scenarioContext.ScenarioExecutionStatus)
                {
                    case ScenarioExecutionStatus.TestError or ScenarioExecutionStatus.BindingError:
                        currentTestCaseRunner.GetTestCase().Fail(
                            new AgenixSystemException(GetScenarioStatusMessage(scenarioContext)));
                        break;
                    case ScenarioExecutionStatus.UndefinedStep:
                        currentTestCaseRunner.GetTestCase().Fail(
                            new AgenixSystemException(
                                GetScenarioStatusMessage(scenarioContext),
                                new MissingStepDefinitionException()));
                        break;
                }

                // handle well-known unit framework's ignoring exceptions
                if (scenarioContext.TestError != null)
                {
                    var testErrorException = scenarioContext.TestError.GetType();

                    if (testErrorException.FullName != null &&
                        (testErrorException.FullName.Equals("NUnit.Framework.IgnoreException")
                         || testErrorException.FullName.Equals("NUnit.Framework.InconclusiveException")
                         || testErrorException.FullName.Equals(
                             "Microsoft.VisualStudio.TestTools.UnitTesting.AssertInconclusiveException")
                         || testErrorException.FullName.Equals("Xunit.SkipException")))
                    {
                        currentTestCaseRunner.GetTestCase().Fail(
                            new AgenixSystemException(GetScenarioStatusMessage(scenarioContext)));
                    }
                }

                var eventArg = new TestCaseFinishedEventArgs(currentTestCaseRunner, featureContext, scenarioContext);
                AgenixAddIn.OnBeforeScenarioFinished(this, eventArg);

                if (!eventArg.Canceled)
                {
                    currentTestCaseRunner.Stop();

                    AgenixAddIn.OnAfterScenarioFinished(null,
                        new TestCaseFinishedEventArgs(currentTestCaseRunner, featureContext, scenarioContext));
                    AgenixAddIn.RemoveScenarioTestReporter(scenarioContext, ref currentTestCaseRunner);
                }
            }
        }
        catch (Exception exp)
        {
            Log.LogError(exp, exp.Message);
        }
    }

    private string GetScenarioStatusMessage(ScenarioContext scenarioContext)
    {
        var status = scenarioContext.ScenarioExecutionStatus switch
        {
            ScenarioExecutionStatus.OK => TestResult.RESULT.SUCCESS,
            ScenarioExecutionStatus.Skipped => TestResult.RESULT.SKIP,
            _ => TestResult.RESULT.FAILURE
        };

        var baseMessage = $"Scenario '{scenarioContext.ScenarioInfo.Title}' finished with the status '{status}'";

        return scenarioContext.ScenarioExecutionStatus switch
        {
            ScenarioExecutionStatus.OK => $"{baseMessage} - All steps passed successfully.",
            ScenarioExecutionStatus.StepDefinitionPending => $"{baseMessage} - Implementation pending.",
            ScenarioExecutionStatus.UndefinedStep => $"{baseMessage} - One or more steps not defined.",
            ScenarioExecutionStatus.BindingError => $"{baseMessage} - Error in step binding.",
            ScenarioExecutionStatus.TestError => $"{baseMessage} - Test execution error occurred => " +
                                                 scenarioContext.TestError,
            ScenarioExecutionStatus.Skipped => $"{baseMessage} - Execution was skipped => " + scenarioContext.TestError,
            _ => baseMessage
        };
    }


    /// <summary>
    ///     Initializes the <see cref="Agenix" /> instance by invoking the required setup
    ///     processes and triggering the initialization event through the ReportPortal add-in.
    /// </summary>
    /// <returns>The initialized instance of <see cref="Agenix" />.</returns>
    private static Core.Agenix Initialize()
    {
        var args = new InitializingEventArgs(AgenixInstanceManager.GetOrDefault());
        args.Agenix.BeforeSuite(SuiteName).ConfigureAwait(false).GetAwaiter().GetResult();
        AgenixAddIn.OnInitializing(typeof(AgenixHooks), args);

        return args.Agenix;
    }
}
