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
using Agenix.Api.Annotations;
using Agenix.Api.Log;
using Agenix.Core.Actions;
using Microsoft.Extensions.Logging;
using Reqnroll;

namespace Agenix.Reqroll.Plugin.Tests;

[Binding]
public sealed class StepDefinition
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(Hooks));
    [AgenixResource] private ITestCaseRunner _testCaseRunner;

    [Given("I have entered (.*) into the calculator")]
    public void GivenIHaveEnteredSomethingIntoTheCalculator(int number)
    {
        _testCaseRunner.Given(EchoAction.Builder.Echo(number.ToString()));
    }

    [When("I press add")]
    public void WhenIPressAdd()
    {
    }

    [Then("the result should be (.*) on the screen")]
    public void ThenTheResultShouldBe(int result)
    {
    }

    [Then(@"I execute failed step")]
    public void ThenIExecuteFailedTest()
    {
        throw new Exception("This step raises an exception.");
    }
}
