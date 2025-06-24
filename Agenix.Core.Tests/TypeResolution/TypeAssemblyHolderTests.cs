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

using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.TypeResolution;

#endregion

namespace Agenix.Core.Tests.TypeResolution;

/// <summary>
///     Test class that contains unit tests for verifying the behavior of the <c>TypeAssemblyHolder</c> class.
/// </summary>
/// <remarks>
///     The tests validate the proper handling of assembly-qualified and non-assembly-qualified type names,
///     as well as handling of generic types.
/// </remarks>
[TestFixture]
public class TypeAssemblyHolderTests
{
    [Test]
    public void CanTakeQualifiedType()
    {
        var testType = typeof(TestObject);
        var tah = new TypeAssemblyHolder(testType.AssemblyQualifiedName);
        ClassicAssert.IsTrue(tah.IsAssemblyQualified);
        ClassicAssert.AreEqual(testType.FullName, tah.TypeName);
        ClassicAssert.AreEqual(testType.Assembly.FullName, tah.AssemblyName);
    }

    [Test]
    public void CanTakeUnqualifiedType()
    {
        var testType = typeof(TestObject);
        var tah = new TypeAssemblyHolder(testType.FullName);
        ClassicAssert.IsFalse(tah.IsAssemblyQualified);
        ClassicAssert.AreEqual(testType.FullName, tah.TypeName);
        ClassicAssert.AreEqual(null, tah.AssemblyName);
    }

    [Test]
    public void CanTakeUnqualifiedGenericType()
    {
        var testType = typeof(TestGenericObject<int, string>);
        var tah = new TypeAssemblyHolder(testType.FullName);
        ClassicAssert.IsFalse(tah.IsAssemblyQualified);
        ClassicAssert.AreEqual(testType.FullName, tah.TypeName);
        ClassicAssert.AreEqual(null, tah.AssemblyName);
    }

    [Test]
    public void CanTakeQualifiedGenericType()
    {
        var testType = typeof(TestGenericObject<int, string>);
        var tah = new TypeAssemblyHolder(testType.AssemblyQualifiedName);
        ClassicAssert.IsTrue(tah.IsAssemblyQualified);
        ClassicAssert.AreEqual(testType.FullName, tah.TypeName);
        ClassicAssert.AreEqual(testType.Assembly.FullName, tah.AssemblyName);
    }
}
