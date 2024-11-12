namespace Agenix.Core.Report;

/// <summary>
///     Defines an interface for classes that are aware of test action listeners.
/// </summary>
public interface ITestActionListenerAware
{
    /// Adds a new test action listener.
    /// @param listener The test action listener to be added.
    /// /
    void AddTestActionListener(ITestActionListener listener);
}