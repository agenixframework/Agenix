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
using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Report;
using Moq;
using NUnit.Framework;

namespace Agenix.Core.Tests.Report;

public class TestReportersTest
{
    private TestReporters _fixture;
    private Mock<ITestCase> _testCaseMock;

    [SetUp]
    public void BeforeMethodSetup()
    {
        _testCaseMock = new Mock<ITestCase>();

        _fixture = new TestReporters();
    }

    [Test]
    public void OnStartWithoutAutoClear()
    {
        var testResults = _fixture.GetTestResults();

        _fixture.SetAutoClear(false);
        _fixture.OnStart();

        Assert.That(_fixture.GetTestResults(), Is.Not.Null);
        Assert.That(_fixture.GetTestResults(), Is.EqualTo(testResults));
    }

    [Test]
    public void OnStartWithAutoClear()
    {
        var testResults = _fixture.GetTestResults();

        _fixture.SetAutoClear(true);
        _fixture.OnStart();

        Assert.That(_fixture.GetTestResults(), Is.Not.Null);
        Assert.That(_fixture.GetTestResults(), Is.Not.EqualTo(testResults));
    }

    [Test]
    public void OnFinishSuccessGeneratesReports()
    {
        VerifyGenerateReport(() => _fixture.OnFinishSuccess());
    }

    [Test]
    public void OnFinishFailureGeneratesReports()
    {
        var cause = new Mock<Exception>();

        VerifyGenerateReport(() => _fixture.OnFinishFailure(cause.Object));

        cause.VerifyNoOtherCalls();
    }

    private void VerifyGenerateReport(Action invocation)
    {
        var testReporter1 = new Mock<ITestReporter>();
        _fixture.AddTestReporter(testReporter1.Object);

        var testReporter2 = new Mock<ITestReporter>();
        _fixture.AddTestReporter(testReporter2.Object);

        invocation();

        testReporter1.Verify(tr => tr.GenerateReport(It.IsAny<TestResults>()));
        testReporter2.Verify(tr => tr.GenerateReport(It.IsAny<TestResults>()));
    }

    [Test]
    public void OnTestFinishAddsTestResultToList()
    {
        var testResultMock = new Mock<TestResult>();
        _testCaseMock.Setup(tc => tc.GetTestResult()).Returns(testResultMock.Object);

        _fixture.OnTestFinish(_testCaseMock.Object);

        var testResults = new List<TestResult>(_fixture.GetTestResults().AsList());

        Assert.That(testResults.Count, Is.EqualTo(1));
        Assert.That(testResultMock.Object, Is.EqualTo(testResults[0]));
    }

    [Test]
    public void OnTestFinishIgnoresNullTestResult()
    {
        _testCaseMock.Setup(tc => tc.GetTestResult()).Returns(() => null);

        _fixture.OnTestFinish(_testCaseMock.Object);

        List<TestResult> testResults = [.. _fixture.GetTestResults().AsList()];

        Assert.That(testResults.Count, Is.EqualTo(0));
    }
}
