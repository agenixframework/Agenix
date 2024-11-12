namespace Agenix.Core.Report;

/*
 * Listener invoked on test action execution with success and failure.
 */
/// <summary>
///     Interface to listen for actions performed on test cases, including start, finish, and skip events.
/// </summary>
public interface ITestActionListener
{
    /*
     * Invoked when test gets started
     */
    /// <summary>
    ///     Invoked when a test action is started.
    /// </summary>
    /// <param name="testCase">The test case that contains the started action.</param>
    /// <param name="testAction">The specific test action that was started.</param>
    void OnTestActionStart(ITestCase testCase, ITestAction testAction);

    /*
     * Invoked when test gets finished
     */
    /// <summary>
    ///     Invoked when a test action is finished.
    /// </summary>
    /// <param name="testCase">The test case that contains the finished action.</param>
    /// <param name="testAction">The specific test action that was finished.</param>
    void OnTestActionFinish(ITestCase testCase, ITestAction testAction);

    /*
     * Invoked when test is skipped
     */
    /// <summary>
    ///     Invoked when a test action is skipped.
    /// </summary>
    /// <param name="testCase">The test case that contains the skipped action.</param>
    /// <param name="testAction">The specific test action that was skipped.</param>
    void OnTestActionSkipped(ITestCase testCase, ITestAction testAction);
}