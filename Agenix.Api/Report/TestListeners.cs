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

/// Class responsible for spreading test events to all available test listeners
/// injected by Spring's IoC container.
/// /
public class TestListeners : ITestListenerAware
{
    /** List of test listeners **/
    private readonly List<ITestListener> _testListeners = [];

    /// Adds a test listener to the list of test listeners.
    /// <param name="listener">The test listener to add.</param>
    public void AddTestListener(ITestListener listener)
    {
        if (!_testListeners.Contains(listener))
        {
            _testListeners.Add(listener);
        }
    }

    /// Notifies all test listeners about the failure of a test.
    /// <param name="test">The test case that has failed.</param>
    /// <param name="cause">The exception that caused the failure.</param>
    public void OnTestFailure(ITestCase test, Exception cause)
    {
        foreach (var listener in _testListeners)
        {
            listener.OnTestFailure(test, cause);
        }
    }

    /// Notifies all test listeners about the completion of a test.
    /// <param name="test">The test case that has finished.</param>
    public void OnTestFinish(ITestCase test)
    {
        foreach (var listener in _testListeners)
        {
            listener.OnTestFinish(test);
        }
    }

    /// Notifies all test listeners about a skipped test.
    /// <param name="test">The test case that is skipped.</param>
    public void OnTestSkipped(ITestCase test)
    {
        foreach (var listener in _testListeners)
        {
            listener.OnTestSkipped(test);
        }
    }

    /// Notifies all test listeners about the start of a test.
    /// <param name="test">The test case that is starting.</param>
    public void OnTestStart(ITestCase test)
    {
        foreach (var listener in _testListeners)
        {
            listener.OnTestStart(test);
        }
    }

    /// Notifies all test listeners about the successful completion of a test.
    /// <param name="test">The test case that completed successfully.</param>
    public void OnTestSuccess(ITestCase test)
    {
        foreach (var listener in _testListeners)
        {
            listener.OnTestSuccess(test);
        }
    }

    /// Obtains the testListeners.
    /// @return A list of test listeners.
    /// /
    public List<ITestListener> GetTestListeners()
    {
        return _testListeners;
    }
}
