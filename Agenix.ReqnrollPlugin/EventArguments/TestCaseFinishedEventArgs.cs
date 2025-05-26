using System;
using Agenix.Api;
using Reqnroll;

namespace Agenix.ReqnrollPlugin.EventArguments;

/// <summary>
///     Provides data for an event that is triggered when a test case has finished execution.
/// </summary>
public class TestCaseFinishedEventArgs(ITestCaseRunner testCaseRunner) : EventArgs
{
    public TestCaseFinishedEventArgs(ITestCaseRunner testCaseRunner, FeatureContext featureContext,
        ScenarioContext scenarioContext)
        : this(testCaseRunner)
    {
        FeatureContext = featureContext;
        ScenarioContext = scenarioContext;
    }

    public ITestCaseRunner TestCaseRunner { get; } = testCaseRunner;

    public FeatureContext FeatureContext { get; }

    public ScenarioContext ScenarioContext { get; }

    public bool Canceled { get; set; }
}