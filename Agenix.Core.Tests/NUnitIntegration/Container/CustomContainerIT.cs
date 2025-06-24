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

using Agenix.Api.Annotations;
using Agenix.Api.Container;
using Agenix.Core.Container;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.CreateVariablesAction.Builder;
using static Agenix.Core.Actions.EchoAction.Builder;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class CustomContainerIT
{
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void ShouldExecuteReverseContainer()
    {
        _gherkin.When(Reverse().Actions(
            Echo("${text}"),
            Echo("Does it work"),
            CreateVariable("text", "Hello Agenix!")
        ));
    }

    /// <summary>
    ///     Creates and initializes a new instance of a container in which actions can be executed in reverse order.
    /// </summary>
    /// <returns>A builder object for the ReverseActionContainer, allowing for further configuration and execution of actions.</returns>
    private AbstractTestContainerBuilder<ReverseActionContainer, dynamic> Reverse()
    {
        return CustomTestContainerBuilder<ITestActionContainer>.Container(new ReverseActionContainer());
    }

    /// <summary>
    ///     ReverseActionContainer is a specialized implementation of the AbstractActionContainer that executes actions in
    ///     reverse order.
    /// </summary>
    /// <remarks>
    ///     This container overrides the DoExecute method to iterate through the list of actions in reverse, ensuring that the
    ///     last action
    ///     added is executed first and the first action added is executed last.
    /// </remarks>
    public class ReverseActionContainer : AbstractActionContainer
    {
        /// <summary>
        ///     Executes the list of actions in reverse order, where the last added action is executed first.
        /// </summary>
        /// <param name="context">The context in which the actions are to be executed, providing necessary runtime information.</param>
        public override void DoExecute(TestContext context)
        {
            for (var i = GetActions().Count; i > 0; i--)
            {
                ExecuteAction(GetActions()[i - 1], context);
            }
        }
    }
}
