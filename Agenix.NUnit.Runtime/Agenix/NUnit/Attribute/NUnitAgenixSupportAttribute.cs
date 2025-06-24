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

using Agenix.Api;
using Agenix.Core;
using Agenix.Core.Annotations;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using ITestAction = NUnit.Framework.ITestAction;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;

/// An attribute designed to provide custom test actions for NUnit test methods, classes, interfaces, or assemblies.
/// This attribute enables specific actions to be executed before and after test execution, facilitating integration
/// with Agenix test contexts and enhanced configuration capabilities tailored to test scenarios.
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class |
                AttributeTargets.Interface | AttributeTargets.Assembly,
    AllowMultiple = true)]
public class NUnitAgenixSupportAttribute : System.Attribute, ITestAction
{
    [ThreadStatic] private static ITestCaseRunner? _delegate;

    /// Executes actions before the test execution begins.
    /// <param name="test">
    ///     An instance of the test about to be executed, providing context and details of the test case or
    ///     suite.
    /// </param>
    public void BeforeTest(ITest test)
    {
        if (test.IsSuite)
        {
            AgenixInstanceManager.Reset();
            AgenixInstanceManager.GetOrDefault().AgenixContext.ParseConfiguration(test.Fixture);
            AgenixInstanceManager.GetOrDefault().BeforeSuite(test.Name);
            return;
        }

        var context = AgenixInstanceManager.GetOrDefault().AgenixContext.CreateTestContext();
        _delegate = CreateTestRunner(test, context);


        AgenixAnnotations.InjectAll(test.Fixture, AgenixInstanceManager.GetOrDefault(), context);
        AgenixAnnotations.InjectTestRunner(test.Fixture, _delegate);

        _delegate.Start();
    }

    /// Performs actions after the test execution is completed.
    /// <param name="test">The test instance that has been executed, providing context and details of the test case or suite.</param>
    public void AfterTest(ITest test)
    {
        if (test.IsSuite)
        {
            AgenixInstanceManager.GetOrDefault().AfterSuite(test.Name);
            return;
        }

        _delegate?.Stop();
    }

    public ActionTargets Targets => ActionTargets.Test | ActionTargets.Suite;

    /// Creates an instance of a test runner for executing test cases with the provided details.
    /// <param name="testName">The name of the test case to be executed.</param>
    /// <param name="packageName">The package name associated with the test class.</param>
    /// <param name="testClass">The type of the test class containing the test case.</param>
    /// <param name="context">The test context providing necessary configurations and state.</param>
    /// <return>An instance of ITestCaseRunner configured with the specified test case details.</return>
    private static ITestCaseRunner CreateTestRunner(ITest test,
        TestContext context)
    {
        var testCaseRunner = TestCaseRunnerFactory.CreateRunner(new DefaultTestCase(), context);
        testCaseRunner.SetTestClass(test.Fixture?.GetType());
        testCaseRunner.SetName(test.Name);
        testCaseRunner.SetNamespaceName(test.FullName);
        testCaseRunner.SetAuthor(test.Properties.Get("Author")?.ToString() ?? string.Empty);
        testCaseRunner.SetDescription(test.Properties.Get("Description")?.ToString() ?? string.Empty);

        return testCaseRunner;
    }
}
