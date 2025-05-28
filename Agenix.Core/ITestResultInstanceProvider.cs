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

namespace Agenix.Core;

/// <summary>
///     Provides methods to create instances of TestResult for different outcomes of a test case.
///     Implementors may decide to create TestResults with specific parameters derived from the TestCase.
/// </summary>
public interface ITestResultInstanceProvider
{
    /// <summary>
    ///     Creates a successful <see cref="TestResult" /> instance for the given test case.
    /// </summary>
    /// <param name="testCase">The test case for which the success result should be created.</param>
    /// <returns>A <see cref="TestResult" /> instance indicating the test case has succeeded.</returns>
    TestResult CreateSuccess(ITestCase testCase);

    /// <summary>
    ///     Creates a failed <see cref="TestResult" /> instance for the given test case.
    /// </summary>
    /// <param name="testCase">The test case for which the failure result should be created.</param>
    /// <param name="ex">The exception that caused the test case to fail.</param>
    /// <returns>A <see cref="TestResult" /> instance indicating the test case has failed.</returns>
    TestResult CreateFailed(ITestCase testCase, Exception ex);

    /// <summary>
    ///     Creates a skipped <see cref="TestResult" /> instance for the given test case.
    /// </summary>
    /// <param name="testCase">The test case for which the skipped result should be created.</param>
    /// <returns>A <see cref="TestResult" /> instance indicating the test case has been skipped.</returns>
    TestResult CreateSkipped(ITestCase testCase);
}
