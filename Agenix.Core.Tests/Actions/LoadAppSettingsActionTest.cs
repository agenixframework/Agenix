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

using System;
using System.IO;
using System.Reflection;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Actions;

public class LoadAppSettingsActionTest : AbstractNUnitSetUp
{
    [Test]
    public void TestLoadProperties()
    {
        var resourceName =
            $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/app.config";

        var loadProperties = new LoadAppSettingsAction.Builder()
            .ResourceName(resourceName)
            .Build();

        loadProperties.Execute(Context);

        ClassicAssert.IsNotNull(Context.GetVariable("${myVariable}"));
        ClassicAssert.AreEqual("test", Context.GetVariable("${myVariable}"));

        ClassicAssert.IsNotNull(Context.GetVariable("${user}"));
        ClassicAssert.AreEqual("Agenix", Context.GetVariable("${user}"));

        ClassicAssert.IsNotNull(Context.GetVariable("${welcomeText}"));
        ClassicAssert.AreEqual("Hello Agenix!", Context.GetVariable("${welcomeText}"));

        ClassicAssert.IsNotNull(Context.GetVariable("${todayDate}"));
        var expectedDate = "Today is " + DateTime.Now.ToString("yyyy-MM-dd") + "!";
        ClassicAssert.AreEqual(expectedDate, Context.GetVariable("${todayDate}"));
    }

    [Test]
    public void TestUnknownVariableInLoadProperties()
    {
        var resourceName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            "ResourcesTest", "app-error.config");
        resourceName = $"file://{resourceName.Replace("\\", "/")}";

        var loadProperties = new LoadAppSettingsAction.Builder()
            .ResourceName(resourceName)
            .Build();

        try
        {
            loadProperties.Execute(Context);
        }
        catch (AgenixSystemException e)
        {
            ClassicAssert.AreEqual("Unknown variable 'unknownVar'", e.Message);
            return;
        }

        Assert.Fail("Missing exception for unknown variable in config file");
    }
}
