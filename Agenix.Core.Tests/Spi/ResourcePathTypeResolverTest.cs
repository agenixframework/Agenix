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

using Agenix.Api.Spi;
using Agenix.Core.Tests.Spi.mocks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Spi;

/// <summary>
///     A test class for verifying the behavior and functionality of the <see cref="ResourcePathTypeResolver" /> class.
///     This class contains unit tests to ensure the resolution of resource paths to types and properties works as
///     expected.
/// </summary>
public class ResourcePathTypeResolverTest
{
    [Test]
    public void TestResolveProperty()
    {
        ClassicAssert.AreEqual(new ResourcePathTypeResolver().ResolveProperty("mocks/foo", "name"), "fooMock");
        ClassicAssert.AreEqual(
            new ResourcePathTypeResolver().ResolveProperty("mocks/foo", ITypeResolver.DEFAULT_TYPE_PROPERTY),
            typeof(Foo).FullName);
        ClassicAssert.AreEqual(new ResourcePathTypeResolver().ResolveProperty("Extension/mocks/foo", "name"),
            "fooMock");
        ClassicAssert.AreEqual(
            new ResourcePathTypeResolver("Extension/mocks").ResolveProperty("foo", ITypeResolver.DEFAULT_TYPE_PROPERTY),
            typeof(Foo).FullName);
        ClassicAssert.AreEqual(new ResourcePathTypeResolver("Extension/mocks").ResolveProperty("foo", "name"),
            "fooMock");
        ClassicAssert.AreEqual(
            new ResourcePathTypeResolver("Extension/mocks").ResolveProperty("Extension/mocks/foo", "name"), "fooMock");
        ClassicAssert.AreEqual(new ResourcePathTypeResolver("Extension/mocks").ResolveProperty("bar", "name"),
            "barMock");
        ClassicAssert.AreEqual(
            new ResourcePathTypeResolver().ResolveProperty("mocks/foo", ITypeResolver.DEFAULT_TYPE_PROPERTY),
            typeof(Foo).FullName);
    }

    [Test]
    public void TestResolve()
    {
        ClassicAssert.AreEqual(new ResourcePathTypeResolver("Extension/mocks").Resolve<dynamic>("foo").GetType(),
            typeof(Foo));
        ClassicAssert.AreEqual(new ResourcePathTypeResolver("Extension/mocks").Resolve<dynamic>("bar").GetType(),
            typeof(Bar));
        ClassicAssert.AreEqual(
            new ResourcePathTypeResolver("Extension/mocks").Resolve<dynamic>("foo", ITypeResolver.DEFAULT_TYPE_PROPERTY)
                .GetType(), typeof(Foo));

        ClassicAssert.AreEqual(
            new ResourcePathTypeResolver("Extension/mocks").Resolve<dynamic>("params", 1, (short)2, 3d, 4f, 'c', true,
                new[] { 1, 2, 3 }, "StringParam").GetType(), typeof(FooWithParams));
    }

    [Test]
    public void TestResolveAll()
    {
        var resolved = new ResourcePathTypeResolver().ResolveAll<dynamic>("mocks");

        ClassicAssert.AreEqual(resolved.Count, 4);
        ClassicAssert.IsNotNull(resolved["foo"]);
        ClassicAssert.AreEqual(resolved["foo"].GetType(), typeof(Foo));
        ClassicAssert.IsNotNull(resolved["bar"]);
        ClassicAssert.AreEqual(resolved["bar"].GetType(), typeof(Bar));
        ClassicAssert.IsNotNull(resolved["singletonFoo"]);
        ClassicAssert.AreEqual(resolved["singletonFoo"].GetType(), typeof(SingletonFoo));

        resolved = new ResourcePathTypeResolver("Extension/mocks").ResolveAll<dynamic>();

        ClassicAssert.AreEqual(resolved.Count, 4);
        ClassicAssert.IsNotNull(resolved["foo"]);
        ClassicAssert.AreEqual(resolved["foo"].GetType(), typeof(Foo));
        ClassicAssert.IsNotNull(resolved["bar"]);
        ClassicAssert.AreEqual(resolved["bar"].GetType(), typeof(Bar));
        ClassicAssert.IsNotNull(resolved["singletonFoo"]);
        ClassicAssert.AreEqual(resolved["singletonFoo"].GetType(), typeof(SingletonFoo));

        resolved = new ResourcePathTypeResolver().ResolveAll<dynamic>("mocks", ITypeResolver.DEFAULT_TYPE_PROPERTY,
            "name");

        ClassicAssert.AreEqual(resolved.Count, 4);
        ClassicAssert.IsNotNull(resolved["fooMock"]);
        ClassicAssert.AreEqual(resolved["fooMock"].GetType(), typeof(Foo));
        ClassicAssert.IsNotNull(resolved["barMock"]);
        ClassicAssert.AreEqual(resolved["barMock"].GetType(), typeof(Bar));
        ClassicAssert.IsNotNull(resolved["singletonFooMock"]);
        ClassicAssert.AreEqual(resolved["singletonFooMock"].GetType(), typeof(SingletonFoo));

        resolved = new ResourcePathTypeResolver().ResolveAll<dynamic>("all", ITypeResolver.TYPE_PROPERTY_WILDCARD);

        ClassicAssert.AreEqual(resolved.Count, 3);
        ClassicAssert.IsNotNull(resolved["mocks.foo"]);
        ClassicAssert.AreEqual(resolved["mocks.foo"].GetType(), typeof(Foo));
        ClassicAssert.IsNotNull(resolved["mocks.bar"]);
        ClassicAssert.AreEqual(resolved["mocks.bar"].GetType(), typeof(Bar));
        ClassicAssert.IsNotNull(resolved["mocks.singletonFoo"]);
        ClassicAssert.AreEqual(resolved["mocks.singletonFoo"].GetType(), typeof(SingletonFoo));
    }
}
