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

using Agenix.Core.Actions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Actions;

/// <summary>
///     Unit tests for the CreateVariablesAction class.
/// </summary>
/// <remarks>
///     This class contains several test cases to validate the behavior of the CreateVariablesAction class,
///     specifically focusing on the creation, retrieval, and overwriting of variables within a TestContext.
/// </remarks>
public class CreateVariablesActionTest : AbstractNUnitSetUp
{
    [Test]
    public void TestCreateSingleVariable()
    {
        var createVariablesAction = new CreateVariablesAction.Builder()
            .Variable("myVariable", "value")
            .Build();

        createVariablesAction.Execute(Context);

        ClassicAssert.NotNull(Context.GetVariable("${myVariable}"));
        ClassicAssert.AreEqual("value", Context.GetVariable("${myVariable}"));
    }

    [Test]
    public void TestCreateVariables()
    {
        var createVariablesAction = new CreateVariablesAction.Builder()
            .Variable("myVariable", "value1")
            .Variable("anotherVariable", "value2")
            .Build();

        createVariablesAction.Execute(Context);

        ClassicAssert.NotNull(Context.GetVariable("${myVariable}"));
        ClassicAssert.AreEqual("value1", Context.GetVariable("${myVariable}"));
        ClassicAssert.NotNull(Context.GetVariable("${anotherVariable}"));
        ClassicAssert.AreEqual("value2", Context.GetVariable("${anotherVariable}"));
    }

    [Test]
    public void TestOverwriteVariables()
    {
        Context.SetVariable("myVariable", "initialValue");

        var createVariablesAction = new CreateVariablesAction.Builder()
            .Variable("myVariable", "newValue")
            .Build();

        createVariablesAction.Execute(Context);

        ClassicAssert.NotNull(Context.GetVariable("${myVariable}"));
        ClassicAssert.AreEqual("newValue", Context.GetVariable("${myVariable}"));
    }

    [Test]
    public void TestCreateSingleVariableWithFunctionValue()
    {
        var createVariablesAction = new CreateVariablesAction.Builder()
            .Variable("myVariable", "agenix:Concat('Hello ', 'Agenix')")
            .Build();

        createVariablesAction.Execute(Context);

        ClassicAssert.NotNull(Context.GetVariable("${myVariable}"));
        ClassicAssert.AreEqual("Hello Agenix", Context.GetVariable("${myVariable}"));
    }
}
