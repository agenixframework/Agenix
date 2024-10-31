using System.Collections.Generic;

namespace Agenix.Core.Container;

/// <summary>
///     Container interface describing all test action containers that hold several embedded test actions to execute.
/// </summary>
public interface ITestActionContainer : ITestAction
{
    /**
     * Sets the embedded test actions to execute within this container.
     */
    ITestActionContainer SetActions(List<ITestAction> actions);

    /**
     * Get the embedded test actions within this container.
     */
    List<ITestAction> GetActions();

    /**
     * Get the number of embedded actions in this container.
     */
    long GetActionCount();

    /**
     * Adds one to many test actions to the nested action list.
     */
    ITestActionContainer AddTestActions(params ITestAction[] action);

    /**
     * Adds a test action to the nested action list.
     */
    ITestActionContainer AddTestAction(ITestAction action);

    /**
     * Returns the index in the action chain for provided action instance.
     */
    int GetActionIndex(ITestAction action);

    /**
    * Sets the current active action executed.
    */
    void SetActiveAction(ITestAction action);

    /**
    * Sets the last action that has been executed.
    */
    void SetExecutedAction(ITestAction action);

    /**
    * Get the action that was executed most recently.
    */
    ITestAction GetActiveAction();

    /**
    * Gets all nested actions that have been executed in the container.
    */
    List<ITestAction> GetExecutedActions();

    /**
    * Get the test action with given index in list.
    */
    ITestAction GetTestAction(int index);
}