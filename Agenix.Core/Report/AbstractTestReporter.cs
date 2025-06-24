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
using Agenix.Api.Log;
using Agenix.Api.Report;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Report;

/// <summary>
///     Abstract base class for test reporters, providing common functionality and enforcing the implementation of the
///     Generate method.
/// </summary>
public abstract class AbstractTestReporter : ITestReporter
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AbstractTestReporter));

    /// <summary>
    ///     Indicates whether errors should be ignored during test report creation.
    /// </summary>
    public bool IgnoreErrors { get; set; } = TestReporterSettings.IsIgnoreErrors();

    /// <summary>
    ///     Specifies the directory where the test reports will be stored.
    /// </summary>
    public string ReportDirectory { get; set; } = TestReporterSettings.GetReportDirectory();

    /// Generates a report based on the provided test results.
    /// <param name="testResults">The collection of test results to include in the report.</param>
    public void GenerateReport(TestResults testResults)
    {
        try
        {
            Generate(testResults);
        }
        catch (Exception e)
        {
            if (IgnoreErrors)
            {
                Log.LogError(e, "Failed to create test report");
            }
            else
            {
                throw;
            }
        }
    }

    /// Subclasses must implement this method to generate a test report based on the given test results.
    /// <param name="testResults">The test results to be included in the generated report.</param>
    public abstract void Generate(TestResults testResults);
}
