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

#region Imports

using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.TypeResolution;

#endregion

namespace Agenix.Core.Tests.TypeResolution;

/// <summary>
///     Unit tests for the TypeResolutionUtils class.
/// </summary>
[TestFixture]
public sealed class TypeResolutionUtilsTests
{
    [Test]
    public void ResolveFromAssemblyQualifiedName()
    {
        var testObjectType =
            TypeResolutionUtils.ResolveType(
                "Agenix.Core.Tests.TypeResolution.TestObject, Agenix.Core.Tests");
        ClassicAssert.IsNotNull(testObjectType);
        ClassicAssert.IsTrue(testObjectType.Equals(typeof(TestObject)));
    }

    [Test]
    public void ResolveFromBadAssemblyQualifiedName()
    {
        Assert.Throws<TypeLoadException>(() =>
            TypeResolutionUtils.ResolveType(
                "Agenix.Core.Tests.TypeResolution.TestObject, Agenix.Core.FooTests"));
    }

    [Test]
    public void ResolveFromShortName()
    {
        var testObjectType = TypeResolutionUtils.ResolveType("Agenix.Core.Tests.TypeResolution.TestObject");
        ClassicAssert.IsNotNull(testObjectType);
        ClassicAssert.IsTrue(testObjectType.Equals(typeof(TestObject)));
    }

    [Test]
    public void ResolveFromBadShortName()
    {
        Assert.Throws<TypeLoadException>(() =>
            TypeResolutionUtils.ResolveType("Agenix.Core.Tests.TypeResolution.FooBarTestObject"));
    }

    [Test]
    public void ResolveInterfaceArrayFromStringArray()
    {
        Type[] expected = [typeof(IFoo)];
        string[] input = [typeof(IFoo).AssemblyQualifiedName];
        var actual = TypeResolutionUtils.ResolveInterfaceArray(input);
        ClassicAssert.IsNotNull(actual);
        ClassicAssert.AreEqual(expected.Length, actual.Count);
        ClassicAssert.AreEqual(expected[0], actual[0]);
    }

    [Test]
    public void ResolveInterfaceArrayFromStringArrayWithNonInterfaceTypes()
    {
        string[] input = [GetType().AssemblyQualifiedName];
        Assert.Throws<ArgumentException>(() => TypeResolutionUtils.ResolveInterfaceArray(input));
    }

    [Test]
    public void MethodMatch()
    {
        var absquatulateMethod = typeof(TestObject).GetMethod("Absquatulate");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("*", absquatulateMethod), "Should match '*'");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("*tulate", absquatulateMethod), "Should match '*tulate'");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("Absqua*", absquatulateMethod), "Should match 'Absqua*'");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("*quatul*", absquatulateMethod),
            "Should match '*quatul*'");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("Absquatulate", absquatulateMethod),
            "Should match 'Absquatulate'");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("Absquatulate()", absquatulateMethod),
            "Should match 'Absquatulate()'");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("Absquatulate()", absquatulateMethod),
            "Should match 'Absquatulate()'");
        ClassicAssert.IsFalse(TypeResolutionUtils.MethodMatch("Absquatulate(string)", absquatulateMethod),
            "Should not match 'Absquatulate(string)'");

        var addPeriodicElementMethod = typeof(TestObject).GetMethod("AddPeriodicElement");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("AddPeriodicElement", addPeriodicElementMethod),
            "Should match 'AddPeriodicElement'");
        ClassicAssert.IsFalse(TypeResolutionUtils.MethodMatch("AddPeriodicElement()", addPeriodicElementMethod),
            "Should not match 'AddPeriodicElement()'");
        ClassicAssert.IsFalse(TypeResolutionUtils.MethodMatch("AddPeriodicElement(string)", addPeriodicElementMethod),
            "Should not match 'AddPeriodicElement(string)'");
        ClassicAssert.IsTrue(
            TypeResolutionUtils.MethodMatch("AddPeriodicElement(string, string)", addPeriodicElementMethod),
            "Should match 'AddPeriodicElement(string, string)'");
    }

    internal interface IFoo
    {
        bool Spanglish(string foo, object[] args);
    }
}
