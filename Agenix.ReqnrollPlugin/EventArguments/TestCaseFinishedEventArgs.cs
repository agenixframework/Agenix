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
