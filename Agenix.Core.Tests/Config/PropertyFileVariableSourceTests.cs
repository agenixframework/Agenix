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

#region

using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.IO;
using Spring.Objects.Factory.Config;

#endregion

namespace Agenix.Core.Tests.Config;

/// <summary>
///     Unit tests for the PropertyFileVariableSource class.
/// </summary>
[TestFixture]
public sealed class PropertyFileVariableSourceTests
{
    [Test]
    public void TestVariablesResolutionWithSingleLocation()
    {
        var vs = new PropertyFileVariableSource
        {
            Location = new AssemblyResource(
                $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/one.properties")
        };

        // existing vars
        ClassicAssert.AreEqual("Aleks Seovic", vs.ResolveVariable("name"));
        ClassicAssert.AreEqual("32", vs.ResolveVariable("age"));

        // non-existent variable
        ClassicAssert.IsNull(vs.ResolveVariable("dummy"));
    }

    [Test]
    public void TestMissingResourceLocation()
    {
        var vs = new PropertyFileVariableSource
        {
            IgnoreMissingResources = true,
            Locations =
            [
                new AssemblyResource(
                    $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/non-existent.properties"),
                new AssemblyResource(
                    $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/one.properties")
            ]
        };

        // existing vars
        ClassicAssert.AreEqual("Aleks Seovic", vs.ResolveVariable("name"));
        ClassicAssert.AreEqual("32", vs.ResolveVariable("age"));
    }


    [Test]
    public void TestVariablesResolutionWithTwoLocations()
    {
        var vs = new PropertyFileVariableSource
        {
            Locations =
            [
                new AssemblyResource(
                    $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/one.properties"),
                new AssemblyResource(
                    $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/two.properties")
            ]
        };

        // existing vars
        ClassicAssert.AreEqual("Aleksandar Seovic",
            vs.ResolveVariable("name")); // should be overriden by the second file
        ClassicAssert.AreEqual("32", vs.ResolveVariable("age"));
        ClassicAssert.AreEqual("Marija,Ana,Nadja", vs.ResolveVariable("family"));

        // non-existant variable
        ClassicAssert.IsNull(vs.ResolveVariable("dummy"));
    }
}
