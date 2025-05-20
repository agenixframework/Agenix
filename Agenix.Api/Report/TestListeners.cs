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
        if (!_testListeners.Contains(listener)) _testListeners.Add(listener);
    }

    /// Notifies all test listeners about the failure of a test.
    /// <param name="test">The test case that has failed.</param>
    /// <param name="cause">The exception that caused the failure.</param>
    public void OnTestFailure(ITestCase test, Exception cause)
    {
        foreach (var listener in _testListeners) listener.OnTestFailure(test, cause);
    }

    /// Notifies all test listeners about the completion of a test.
    /// <param name="test">The test case that has finished.</param>
    public void OnTestFinish(ITestCase test)
    {
        foreach (var listener in _testListeners) listener.OnTestFinish(test);
    }

    /// Notifies all test listeners about a skipped test.
    /// <param name="test">The test case that is skipped.</param>
    public void OnTestSkipped(ITestCase test)
    {
        foreach (var listener in _testListeners) listener.OnTestSkipped(test);
    }

    /// Notifies all test listeners about the start of a test.
    /// <param name="test">The test case that is starting.</param>
    public void OnTestStart(ITestCase test)
    {
        foreach (var listener in _testListeners) listener.OnTestStart(test);
    }

    /// Notifies all test listeners about the successful completion of a test.
    /// <param name="test">The test case that completed successfully.</param>
    public void OnTestSuccess(ITestCase test)
    {
        foreach (var listener in _testListeners) listener.OnTestSuccess(test);
    }

    /// Obtains the testListeners.
    /// @return A list of test listeners.
    /// /
    public List<ITestListener> GetTestListeners()
    {
        return _testListeners;
    }
}