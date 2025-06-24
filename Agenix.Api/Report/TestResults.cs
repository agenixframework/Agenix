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

using System.Globalization;

namespace Agenix.Api.Report;

/// Represents a collection of test results and provides various operations on the results.
public class TestResults
{
    private const string ZeroPercentage = "0.0";
    private readonly HashSet<TestResult> _results = [];
    private long _totalDurationMillis;

    /// Converts the collection of test results to a list.
    /// <return>A list containing all the test results.</return>
    public List<TestResult> AsList()
    {
        return [.. _results];
    }

    /// Adds a test result to the collection.
    /// <param name="result">The test result to add.</param>
    /// <return>True if the result was added to the collection; otherwise, false.</return>
    public bool AddResult(TestResult result)
    {
        _totalDurationMillis += (long)result.Duration.TotalMilliseconds;

        return _results.Add(result);
    }

    /// Applies a specified action to each test result in the collection.
    /// <param name="callback">The action to apply to each test result.</param>
    public void DoWithResults(Action<TestResult> callback)
    {
        foreach (var result in _results)
        {
            callback(result);
        }
    }

    public int GetSuccess()
    {
        return _results.Count(r => r.IsSuccess());
    }

    /// Gets the percentage of successful test results formatted as a string.
    /// <return> The percentage of successful test results formatted to one decimal place. </return>
    public string GetSuccessPercentageFormatted()
    {
        if (_results.Count == 0 || GetSuccess() == 0)
        {
            return ZeroPercentage;
        }

        return ((double)GetSuccess() / (GetFailed() + GetSuccess()) * 100).ToString("0.0");
    }

    /// Gets the number of failed test results.
    /// <return> The number of test results that are marked as failed. </return>
    public int GetFailed()
    {
        return _results.Count(r => r.IsFailed());
    }

    /// Gets the percentage of failed test results formatted as a string.
    /// <return> The percentage of failed test results formatted to one decimal place. </return>
    public string GetFailedPercentageFormatted()
    {
        if (_results.Count == 0 || GetFailed() == 0)
        {
            return ZeroPercentage;
        }

        return ((double)GetFailed() / (GetFailed() + GetSuccess()) * 100).ToString("0.0");
    }

    /// Gets the number of skipped test results.
    /// <return> The number of test results that are marked as skipped. </return>
    public int GetSkipped()
    {
        return _results.Count(r => r.IsSkipped());
    }

    /// Gets the percentage of skipped test results formatted as a string.
    /// <return> The percentage of skipped test results formatted to one decimal place. </return>
    public string GetSkippedPercentageFormatted()
    {
        return _results.Count == 0
            ? ZeroPercentage
            : ((double)GetSkipped() / _results.Count * 100).ToString("0.0");
    }

    /// Gets the number of test results.
    /// <return> The total number of test results. </return>
    public int GetSize()
    {
        return _results.Count;
    }

    /// Gets the total duration of all test results combined.
    /// <return> The total duration as a TimeSpan object. </return>
    /// /
    public TimeSpan GetTotalDuration()
    {
        return TimeSpan.FromMilliseconds(_totalDurationMillis);
    }

    /// Creates and returns a new NumberFormatInfo instance with customized decimal and group separators.
    /// <return>A NumberFormatInfo instance with decimal and group separators set to "." and "," respectively.</return>
    private NumberFormatInfo GetNewDecimalFormat()
    {
        var numberFormat = new NumberFormatInfo { NumberDecimalSeparator = ".", NumberGroupSeparator = "," };

        // Set custom format for zero and one decimal place
        var specificCulture = new CultureInfo("en-US") { NumberFormat = numberFormat };

        return numberFormat;
    }
}
