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
using System.Threading.Tasks;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using NUnit.Framework;

namespace Agenix.Core.Tests.Actions;

public class LoadAppSettingsActionTest : AbstractNUnitSetUp
{
    [Test]
    public async Task TestLoadProperties()
    {
        var resourceName =
            $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/app.config";

        var loadProperties = new LoadAppSettingsAction.Builder()
            .WithResourceName(resourceName)
            .Build();

        await loadProperties.ExecuteAsync(Context);

        Assert.That(Context.GetVariable("${myVariable}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${myVariable}"), Is.EqualTo("test"));

        Assert.That(Context.GetVariable("${user}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${user}"), Is.EqualTo("Agenix"));

        Assert.That(Context.GetVariable("${welcomeText}"), Is.Not.Null);
        Assert.That(Context.GetVariable("${welcomeText}"), Is.EqualTo("Hello Agenix!"));

        Assert.That(Context.GetVariable("${todayDate}"), Is.Not.Null);
        var expectedDate = "Today is " + DateTime.Now.ToString("yyyy-MM-dd") + "!";
        Assert.That(Context.GetVariable("${todayDate}"), Is.EqualTo(expectedDate));
    }

    [Test]
    public void TestUnknownVariableInLoadProperties()
    {
        var resourceName = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException(),
            "ResourcesTest", "app-error.config");
        resourceName = $"file://{resourceName.Replace("\\", "/")}";

        var loadProperties = new LoadAppSettingsAction.Builder()
            .WithResourceName(resourceName)
            .Build();

        var exception = Assert.ThrowsAsync<AgenixSystemException>(() => loadProperties.ExecuteAsync(Context));

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Unknown variable 'unknownVar'"));
    }
}
