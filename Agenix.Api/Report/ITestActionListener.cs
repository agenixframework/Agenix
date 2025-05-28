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

/// <summary>
///     Interface to listen for actions performed on test cases, including start, finish, and skip events.
/// </summary>
public interface ITestActionListener
{
    /// <summary>
    ///     Invoked when a test action is started.
    /// </summary>
    /// <param name="testCase">The test case that contains the started action.</param>
    /// <param name="testAction">The specific test action that was started.</param>
    void OnTestActionStart(ITestCase testCase, ITestAction testAction);

    /// <summary>
    ///     Invoked when a test action is finished.
    /// </summary>
    /// <param name="testCase">The test case that contains the finished action.</param>
    /// <param name="testAction">The specific test action that was finished.</param>
    void OnTestActionFinish(ITestCase testCase, ITestAction testAction);

    /// <summary>
    ///     Invoked when a test action is skipped.
    /// </summary>
    /// <param name="testCase">The test case that contains the skipped action.</param>
    /// <param name="testAction">The specific test action that was skipped.</param>
    void OnTestActionSkipped(ITestCase testCase, ITestAction testAction);
}
