namespace Agenix.Core.Common;

/**
 * Interface indicates that test action is aware of its completed state. This is used when test case needs to wait for nested actions to finish
 * working before closing the test case. Asynchronous test action execution may implement this interface in order to publish the completed state of
 * forked processes.
 */
public interface ICompletable
{
    /**
     * Checks for test action to be finished.
     * @return
     */
    bool IsDone(TestContext context);
}