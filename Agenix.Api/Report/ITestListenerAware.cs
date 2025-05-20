namespace Agenix.Api.Report;

/// <summary>
///     Interface for classes wanting to be aware of test listeners.
/// </summary>
public interface ITestListenerAware
{
    /// <summary>
    ///     Adds a new test listener
    /// </summary>
    /// <param name="testListener"></param>
    void AddTestListener(ITestListener testListener);
}