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
        foreach (var listener in _testActionListeners) listener.OnTestActionFinish(testCase, testAction);
    }

    /// Notifies all registered listeners that a test action has been skipped.
    /// <param name="testCase">The test case that is executing the test action.</param>
    /// <param name="testAction">The test action that has been skipped.</param>
    public void OnTestActionSkipped(ITestCase testCase, ITestAction testAction)
    {
        foreach (var listener in _testActionListeners) listener.OnTestActionSkipped(testCase, testAction);
    }

    /// Notifies all registered listeners that a test action has started.
    /// <param name="testCase">The test case that is executing the test action.</param>
    /// <param name="testAction">The test action that has started.</param>
    public void OnTestActionStart(ITestCase testCase, ITestAction testAction)
    {
        foreach (var listener in _testActionListeners) listener.OnTestActionStart(testCase, testAction);
    }

    /// Obtains the TestActionListeners.
    /// @return A read-only list of test action listeners.
    /// /
    public IReadOnlyList<ITestActionListener> GetTestActionListeners()
    {
        return _testActionListeners.AsReadOnly();
    }
}