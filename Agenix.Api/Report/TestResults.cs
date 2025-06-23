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
