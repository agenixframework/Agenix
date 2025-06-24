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
using System.Data;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.TypeResolution;

#endregion

namespace Agenix.Core.Tests.TypeResolution;

/// <summary>
///     Unit tests for the TypeResolver class.
/// </summary>
[TestFixture]
public class TypeResolverTests
{
    protected virtual ITypeResolver GetTypeResolver()
    {
        return new TypeResolver();
    }

    [Test]
    public void ResolveLocalAssemblyType()
    {
        var t = GetTypeResolver().Resolve("Agenix.Core.Tests.TypeResolution.TestObject");
        ClassicAssert.AreEqual(typeof(TestObject), t);
    }

    [Test]
    public void ResolveWithPartialAssemblyName()
    {
        var t = GetTypeResolver().Resolve("System.Data.IDbConnection, System.Data");
        ClassicAssert.AreEqual(typeof(IDbConnection), t);
    }

    /// <summary>
    ///     Tests that the resolve method throws the correct exception
    ///     when supplied a load of old rubbish as a type name.
    /// </summary>
    [Test]
    public void ResolveWithNonExistentTypeName()
    {
        Assert.Throws<TypeLoadException>(() => GetTypeResolver().Resolve("RaskolnikovsDilemma, System.StPetersburg"));
    }

    [Test]
    public void ResolveBadArgs()
    {
        Assert.Throws<TypeLoadException>(() => GetTypeResolver().Resolve(null));
    }

    [Test]
    public void ResolveLocalAssemblyTypeWithFullAssemblyQualifiedName()
    {
        var t = GetTypeResolver().Resolve(typeof(TestObject).AssemblyQualifiedName);
        ClassicAssert.AreEqual(typeof(TestObject), t);
    }

    [Test]
    public void LoadTypeFromSystemAssemblySpecifyingOnlyTheAssemblyDisplayName()
    {
        var stringType = typeof(string).FullName + ", System";
        Assert.Throws<TypeLoadException>(() => GetTypeResolver().Resolve(stringType));
    }

    [Test]
    public void LoadTypeFromSystemAssemblySpecifyingTheFullAssemblyName()
    {
        var stringType = typeof(string).AssemblyQualifiedName;
        var t = GetTypeResolver().Resolve(stringType);
        ClassicAssert.AreEqual(typeof(string), t);
    }
}
