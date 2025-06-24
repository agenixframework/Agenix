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
using Agenix.Api.Annotations;
using Agenix.Api.Log;
using Agenix.Core.Actions;
using Microsoft.Extensions.Logging;
using Reqnroll;
using Reqnroll.UnitTestProvider;

namespace Agenix.Reqroll.Plugin.Tests;

[Binding]
public sealed class Hooks
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Hooks));

    private readonly IUnitTestRuntimeProvider _unitTestRuntimeProvider;

    [AgenixResource] private ITestCaseRunner _testCaseRunner;

    public Hooks(IUnitTestRuntimeProvider unitTestRuntimeProvider)
    {
        _unitTestRuntimeProvider = unitTestRuntimeProvider;
    }

    [BeforeTestRun(Order = int.MinValue)]
    public static void BeforeTestRun()
    {
        ConfigureLogging();
    }

    /// <summary>
    ///     Configures and initializes the global logging system with standard settings
    /// </summary>
    /// <param name="configPath">Path to the log4net configuration file</param>
    /// <param name="minimumLevel">Minimum log level to capture</param>
    private static void ConfigureLogging(string configPath = "log4net.config", LogLevel minimumLevel = LogLevel.Debug)
    {
        // Create a new LoggerFactory
        var factory = LoggerFactory.Create(builder =>
        {
            // Set the minimum log level
            builder.SetMinimumLevel(minimumLevel);

            // Add log4net provider
            builder.AddLog4Net(configPath);
        });

        // Set the LogManager's LoggerFactory property
        LogManager.LoggerFactory = factory;
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        // all scenarios should not be affected (uncomment it to test)
        //throw new Exception("AfterTestRun fail exception.");
    }

    [BeforeFeature("feature_should_fail_before")]
    public static void BeforeFeatureShouldFail()
    {
        throw new Exception("This feature should fail before.");
    }

    [AfterFeature("feature_should_fail_after")]
    public static void AfterFeatureShouldFail()
    {
        throw new Exception("This feature should fail after.");
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        _testCaseRunner.Run(EchoAction.Builder.Echo("Before scenario from hooks.")
            .Name("Print the message action")
            .Description("Wise words from the wise man"));
    }

    [BeforeScenario("scenario_should_fail_before")]
    public void BeforeScenarioShouldFail()
    {
        throw new Exception("This scenario should fail before.");
    }

    [BeforeScenario("scenario_should_ignore_before_runtime")]
    public void BeforeScenarioShouldIgnore()
    {
        //_unitTestRuntimeProvider.TestIgnore("This scenario should be ignored at runtime.");
        _unitTestRuntimeProvider.TestInconclusive("This scenario should be ignored at runtime.");
    }

    [AfterScenario("scenario_should_fail_after")]
    public void AfterScenarioShouldFail()
    {
        throw new Exception("This scenario should fail after.");
    }

    [BeforeStep("step_should_fail_before")]
    public void BeforeStepShouldFail()
    {
        throw new Exception("This step should fail before.");
    }

    [AfterStep("step_should_fail_after")]
    public void AfterStepShouldFail()
    {
        throw new Exception("This step should fail after.");
    }
}
