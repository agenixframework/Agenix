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

using Agenix.Api;
using Agenix.Api.Context;

namespace Agenix.Core.Actions;

/// Builder for creating default test actions.
/// This class provides a concrete implementation for building test actions using a delegate TestAction.
public class DefaultTestActionBuilder(TestAction action)
    : AbstractTestActionBuilder<ITestAction, DefaultTestActionBuilder>
{
    /// Static fluent entry method for creating a DefaultTestActionBuilder.
    /// @param action The ITestAction instance used to create the builder.
    /// @return A new instance of DefaultTestActionBuilder.
    /// /
    public static DefaultTestActionBuilder Action(TestAction action)
    {
        return new DefaultTestActionBuilder(action);
    }

    /// Builds an instance of ConcreteTestAction using the provided delegate.
    /// Sets the name and description for the constructed test action.
    /// @return An instance of AbstractTestAction populated with the assigned Name and Description.
    public override AbstractTestAction Build()
    {
        AbstractTestAction testAction = new ConcreteTestAction(action);

        testAction.SetName(GetName());
        testAction.SetDescription(GetDescription());

        return testAction;
    }

    /// Concrete implementation of AbstractTestAction.
    /// This class is responsible for executing a delegate instance of ITestAction.
    private class ConcreteTestAction(TestAction delegateInstance) : AbstractTestAction
    {
        public override void Execute(TestContext context)
        {
            delegateInstance?.Invoke(context);
        }

        public override void DoExecute(TestContext context)
        {
            Execute(context);
        }
    }
}
