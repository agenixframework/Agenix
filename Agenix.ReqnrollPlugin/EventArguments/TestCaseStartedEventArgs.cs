using System;
using Agenix.Api;
using Reqnroll;

namespace Agenix.ReqnrollPlugin.EventArguments;

public class TestCaseStartedEventArgs(ITestCaseRunner testCaseRunner) : EventArgs
{
    public TestCaseStartedEventArgs(ITestCaseRunner testCaseRunner, FeatureContext featureContext,
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