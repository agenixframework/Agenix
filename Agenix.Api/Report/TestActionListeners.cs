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

/// Responsible for broadcasting test action events to all registered test action listeners
/// managed by Spring's IoC container.
/// /
public class TestActionListeners : ITestActionListenerAware
{
    /** List of test action listeners **/
    private readonly List<ITestActionListener> _testActionListeners = [];

    /// Adds a test action listener to the list of listeners.
    /// <param name="listener">The test action listener to be added.</param>
    public void AddTestActionListener(ITestActionListener listener)
    {
        _testActionListeners.Add(listener);
    }

    /// Notifies all registered listeners that a test action has finished.
    /// <param name="testCase">The test case that is executing the test action.</param>
    /// <param name="testAction">The test action that has finished.</param>
    public void OnTestActionFinish(ITestCase testCase, ITestAction testAction)
    {
        foreach (var listener in _testActionListeners)
        {
            listener.OnTestActionFinish(testCase, testAction);
        }
    }

    /// Notifies all registered listeners that a test action has been skipped.
    /// <param name="testCase">The test case that is executing the test action.</param>
    /// <param name="testAction">The test action that has been skipped.</param>
    public void OnTestActionSkipped(ITestCase testCase, ITestAction testAction)
    {
        foreach (var listener in _testActionListeners)
        {
            listener.OnTestActionSkipped(testCase, testAction);
        }
    }

    /// Notifies all registered listeners that a test action has started.
    /// <param name="testCase">The test case that is executing the test action.</param>
    /// <param name="testAction">The test action that has started.</param>
    public void OnTestActionStart(ITestCase testCase, ITestAction testAction)
    {
        foreach (var listener in _testActionListeners)
        {
            listener.OnTestActionStart(testCase, testAction);
        }
    }

    /// Obtains the TestActionListeners.
    /// @return A read-only list of test action listeners.
    /// /
    public IReadOnlyList<ITestActionListener> GetTestActionListeners()
    {
        return _testActionListeners.AsReadOnly();
    }
}
