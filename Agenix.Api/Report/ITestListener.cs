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

namespace Agenix.Api.Report;

/// Interface representing a test listener. Invoked at various stages of the test lifecycle.
/// /
public interface ITestListener
{
    /// Invoked when test gets started
    /// @param test Test case to be started
    /// /
    void OnTestStart(ITestCase test);

    /// Invoked when test gets finished
    /// @param test Test case that has finished
    void OnTestFinish(ITestCase test);

    /// Invoked when test finished with success
    /// <param name="test">Test case that finished successfully</param>
    void OnTestSuccess(ITestCase test);

    /// Invoked when test finished with failure
    /// <param name="test">Test case that finished with failure</param>
    /// <param name="cause">Exception that caused the test failure</param>
    void OnTestFailure(ITestCase test, Exception cause);

    /// Invoked when test is skipped
    /// <param name="test">Test case that is being skipped</param>
    void OnTestSkipped(ITestCase test);
}
