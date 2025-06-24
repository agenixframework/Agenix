#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
