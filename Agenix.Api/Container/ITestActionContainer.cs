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

namespace Agenix.Api.Container;

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
     * Returns the index in the action chain for the provided action instance.
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
    ITestAction? GetActiveAction();

    /**
    * Gets all nested actions that have been executed in the container.
    */
    List<ITestAction> GetExecutedActions();

    /**
    * Get the test action with given index in list.
    */
    ITestAction GetTestAction(int index);
}
