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
using Agenix.Api;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Core;
using Agenix.Core.Annotations;
using Agenix.ReqnrollPlugin.EventArguments;
using Microsoft.Extensions.Logging;
using Reqnroll;

namespace Agenix.ReqnrollPlugin;

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

            if (!eventArg.Canceled) AgenixAddIn.OnAfterRunStarted(null, new RunStartedEventArgs(_agenix.AgenixContext));
        }
        catch (Exception exp)
        {
            Log.LogError(exp.ToString());
        }
    }

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
                    _agenix.AfterSuite(SuiteName);
                    _agenix.AgenixContext.Close();
                    AgenixInstanceManager.Reset();

                    AgenixAddIn.OnAfterRunFinished(null,
                        new RunFinishedEventArgs(_agenix.AgenixContext));
                }
                else
                {
                    _agenix.AfterSuite(SuiteName);
                    _agenix.AgenixContext.Close();
                    AgenixInstanceManager.Reset();
                }
            }
        }
        catch (Exception exp)
        {
            Log.LogError(exp.ToString());
        }
    }

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
            Log.LogError(exp.ToString());
        }
    }

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

                    if (testErrorException.FullName.Equals("NUnit.Framework.IgnoreException")
                        || testErrorException.FullName.Equals("NUnit.Framework.InconclusiveException")
                        || testErrorException.FullName.Equals(
                            "Microsoft.VisualStudio.TestTools.UnitTesting.AssertInconclusiveException")
                        || testErrorException.FullName.Equals("Xunit.SkipException"))
                        currentTestCaseRunner.GetTestCase().Fail(
                            new AgenixSystemException(GetScenarioStatusMessage(scenarioContext)));
                }

                var eventArg = new TestCaseFinishedEventArgs(currentTestCaseRunner, featureContext, scenarioContext);
                AgenixAddIn.OnBeforeScenarioFinished(this, eventArg);

                if (!eventArg.Canceled)
                {
                    currentTestCaseRunner.Stop();

                    AgenixAddIn.OnAfterScenarioFinished(null,
                        new TestCaseFinishedEventArgs(currentTestCaseRunner, featureContext, scenarioContext));
                    AgenixAddIn.RemoveScenarioTestReporter(scenarioContext, currentTestCaseRunner);
                }
            }
        }
        catch (Exception exp)
        {
            Log.LogError(exp.ToString());
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
        args.Agenix.BeforeSuite(SuiteName);
        AgenixAddIn.OnInitializing(typeof(AgenixHooks), args);

        return args.Agenix;
    }
}
