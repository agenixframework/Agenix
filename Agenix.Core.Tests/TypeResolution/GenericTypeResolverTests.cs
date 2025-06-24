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
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.TypeResolution;

#endregion

namespace Agenix.Core.Tests.TypeResolution;

/// <summary>
///     Unit tests for the GenericTypeResolver class.
/// </summary>
[TestFixture]
public class GenericTypeResolverTests : TypeResolverTests
{
    protected override ITypeResolver GetTypeResolver()
    {
        return new GenericTypeResolver();
    }

    [Test]
    public void ResolveLocalAssemblyGenericType()
    {
        var t = GetTypeResolver()
            .Resolve("Agenix.Core.Tests.TypeResolution.TestGenericObject< int, string>");
        ClassicAssert.AreEqual(typeof(TestGenericObject<int, string>), t);
    }

    [Test]
    public void ResolveLocalAssemblyGenericTypeDefinition()
    {
        // CLOVER:ON
        var t = GetTypeResolver().Resolve("Agenix.Core.Tests.TypeResolution.TestGenericObject< ,>");
        // CLOVER:OFF
        ClassicAssert.AreEqual(typeof(TestGenericObject<,>), t);
    }

    [Test]
    public void ResolveLocalAssemblyGenericTypeOpen()
    {
        Assert.Throws<TypeLoadException>(() =>
            GetTypeResolver().Resolve("Agenix.Core.Tests.TypeResolution.TestGenericObject<int >"));
    }

    [Test]
    public void ResolveGenericTypeWithAssemblyName()
    {
        var t = GetTypeResolver().Resolve("System.Collections.Generic.Stack<string>, System");
        ClassicAssert.AreEqual(typeof(Stack<string>), t);
    }

    [Test]
    public void ResolveGenericArrayType()
    {
        var t = GetTypeResolver().Resolve("System.Nullable<[System.Int32, mscorlib]>[,]");
        ClassicAssert.AreEqual(typeof(int?[,]), t);
        t = GetTypeResolver().Resolve("System.Nullable`1[int][,]");
        ClassicAssert.AreEqual(typeof(int?[,]), t);
    }

    [Test]
    public void ResolveGenericArrayTypeWithAssemblyName()
    {
        var t = GetTypeResolver().Resolve("System.Nullable<[System.Int32, mscorlib]>[,], mscorlib");
        ClassicAssert.AreEqual(typeof(int?[,]), t);
        t = GetTypeResolver().Resolve("System.Nullable<[System.Int32, mscorlib]>[,], mscorlib");
        ClassicAssert.AreEqual(typeof(int?[,]), t);
        t = GetTypeResolver().Resolve("System.Nullable`1[[System.Int32, mscorlib]][,], mscorlib");
        ClassicAssert.AreEqual(typeof(int?[,]), t);
    }

    [Test]
    public void ResolveAmbiguousGenericTypeWithAssemblyName()
    {
        Assert.Throws<TypeLoadException>(() =>
            GetTypeResolver()
                .Resolve(
                    "Agenix.Core.Tests.TypeResolution.TestGenericObject<System.Collections.Generic.Stack<int>, System, string>"));
    }

    [Test]
    public void ResolveMalformedGenericType()
    {
        Assert.Throws<TypeLoadException>(() =>
            GetTypeResolver().Resolve("Agenix.Core.Tests.TypeResolution.TestGenericObject<int, <string>>"));
    }

    [Test]
    public void ResolveNestedGenericTypeWithAssemblyName()
    {
        var t = GetTypeResolver()
            .Resolve(
                "System.Collections.Generic.Stack<Agenix.Core.Tests.TypeResolution.TestGenericObject<int, string> >, System");
        ClassicAssert.AreEqual(typeof(Stack<TestGenericObject<int, string>>), t);
    }

    [Test]
    public void ResolveClrNotationStyleGenericTypeWithAssemblyName()
    {
        var t = GetTypeResolver()
            .Resolve(
                "System.Collections.Generic.Stack`1[ [Agenix.Core.Tests.TypeResolution.TestGenericObject`2[int, string], Agenix.Core.Tests] ], System");
        ClassicAssert.AreEqual(typeof(Stack<TestGenericObject<int, string>>), t);
    }

    [Test]
    public void ResolveNestedQuotedGenericTypeWithAssemblyName()
    {
        var t = GetTypeResolver()
            .Resolve(
                "System.Collections.Generic.Stack< [Agenix.Core.Tests.TypeResolution.TestGenericObject<int, string>, Agenix.Core.Tests] >, System");
        ClassicAssert.AreEqual(typeof(Stack<TestGenericObject<int, string>>), t);
    }
}
