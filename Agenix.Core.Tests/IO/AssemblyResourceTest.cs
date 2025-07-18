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
using System.IO;
using Agenix.Api.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;

#endregion

namespace Agenix.Core.Tests.IO;

/// <summary>
///     Unit tests for AssemblyResource
/// </summary>
[TestFixture]
public class AssemblyResourceTest
{
    [SetUp]
    public void SetUp()
    {
    }

    [TearDown]
    public void TearDown()
    {
    }

    /// <summary>
    ///     Use incorrect format for an assembly resource.  Using
    ///     comma delimited instead of '/'.
    /// </summary>
    [Test]
    public void CreateWithMalformedResourceName()
    {
        Assert.Throws<UriFormatException>(() =>
            new AssemblyResource("assembly://Agenix.Core.Tests,Agenix.Core.Tests.TestResource.txt"));
    }

    /// <summary>
    ///     Use old format, no longer supported (actually never publicly released)
    ///     that used 'dot' notation to seperate the namespace and resource name.
    /// </summary>
    [Test]
    public void CreateWithObsoleteResourceName()
    {
        Assert.Throws<UriFormatException>(() =>
            new AssemblyResource("assembly://Agenix.Core.Tests/Agenix.Core.Tests.TestResource.txt"));
    }

    /// <summary>
    ///     Use the correct format but with an invalid assembly name.
    /// </summary>
    [Test]
    public void CreateFromInvalidAssembly()
    {
        Assert.Throws<FileNotFoundException>(() =>
            new AssemblyResource("assembly://Xyz.Invalid.Assembly/Agenix.Core.Tests/TestResource.txt"));
    }

    /// <summary>
    ///     Sunny day scenario that creates IResources and ensures the
    ///     correct contents can be read from them.
    /// </summary>
    [Test]
    public void CreateValidAssemblyResource()
    {
        IResource res = new AssemblyResource("assembly://Agenix.Core.Tests/Agenix.Core.Tests/TestResource.txt");
        AssertResourceContent(res, "Agenix.TestResource.txt");
        IResource res2 = new AssemblyResource("assembly://Agenix.Core.Tests/Agenix.Core.Tests.IO/TestResource.txt");
        AssertResourceContent(res2, "Agenix.Core.IO.TestResource.txt");
    }

    /// <summary>
    ///     Use correct assembly name, but incorrect namespace and resource name.
    /// </summary>
    [Test]
    public void CreateInvalidAssemblyResource()
    {
        IResource res = new AssemblyResource("assembly://Agenix.Core.Tests/Xyz/InvalidResource.txt");
        ClassicAssert.IsFalse(res.Exists, "Exists should return false");
        ClassicAssert.IsNull(res.InputStream, "Stream should be null");
    }

    [Test]
    public void CreateRelativeWhenNotRelative()
    {
        IResource res = new AssemblyResource("assembly://Agenix.Core.Tests/Agenix.Core.Tests/TestResource.txt");
        var res2 = res.CreateRelative("Agenix.Core.Tests/Agenix.Core.Tests.IO/TestResource.txt");
        AssertResourceContent(res2, "Agenix.Core.IO.TestResource.txt");
    }

    /// <summary>
    ///     Test creating a resource relative to the location of the first.
    ///     The first resource is physically located in the root of the project since
    ///     the default namespace of the Agenix.Core.Tests project is
    ///     'Agenix.Core.Tests'.  The notation './IO/TestResource.txt' will navigate
    ///     down to the 'Agenix.Core.Tests.IO' namespace and CreateRelative will
    ///     then retrieve the similarly named TestResource.txt located there.
    /// </summary>
    [Test]
    public void CreateRelativeInChildNamespace()
    {
        IResource res = new AssemblyResource("assembly://Agenix.Core.Tests/Agenix.Core.Tests/TestResource.txt");
        var res2 = res.CreateRelative("./IO/TestResource.txt");
        AssertResourceContent(res2, "Agenix.Core.IO.TestResource.txt");
    }

    /// <summary>
    ///     Test creating a resource relative to the location of the first.
    ///     The first resource is physically located in the root of the project since
    ///     the default namespace of the Agenix.Core.Tests project is
    ///     'Agenix.Core.Tests'.  The notation 'IO/TestResource.txt' will navigate
    ///     down to the 'Agenix.Core.Tests' namespace and CreateRelative will
    ///     then retrieve the similarly named TestResource.txt located there.
    /// </summary>
    [Test]
    public void CreateRelativeInChildNamespaceWithoutPrefix()
    {
        IResource res = new AssemblyResource("assembly://Agenix.Core.Tests/Agenix.Core.Tests/TestResource.txt");
        var res2 = res.CreateRelative("IO/TestResource.txt");
        AssertResourceContent(res2, "Agenix.Core.IO.TestResource.txt");
    }

    /// <summary>
    ///     Test creating a resource relative to the root of the assembly.
    ///     The first resource is physically located in the root of the project since
    ///     the default namespace of the Agenix.Core.Tests project is
    ///     'Agenix.Core.Tests'.  The notation '/Agenix.Core.Tests.IO/TestResource.txt' will navigate
    ///     down to the 'Agenix.Core.Tests.IO' namespace and CreateRelative will
    ///     then retrieve the similarly named TestResource.txt located there.
    /// </summary>
    [Test]
    public void CreateRelativeToRoot()
    {
        IResource res = new AssemblyResource("assembly://Agenix.Core.Tests/Agenix.Core.Tests/TestResource.txt");
        var res2 = res.CreateRelative("/Agenix.Core.Tests.IO/TestResource.txt");
        AssertResourceContent(res2, "Agenix.Core.IO.TestResource.txt");
    }

    [Test]
    public void CreateRelativeInParentNamespace()
    {
        IResource res = new AssemblyResource("assembly://Agenix.Core.Tests/Agenix.Core.Tests.IO/TestResource.txt");
        var res2 = res.CreateRelative("../TestResource.txt");
        AssertResourceContent(res2, "Agenix.TestResource.txt");
    }

    /// <summary>
    ///     Try to create a relative resource, but use too many '..' to navigate
    ///     past the root namespace, off into la-la land.
    /// </summary>
    [Test]
    public void TooMuchParentNamespacesAbove()
    {
        IResource res = new AssemblyResource("assembly://Agenix.Core.Tests/Agenix.Core.Tests.IO/TestResource.txt");
        Assert.Throws<UriFormatException>(() => res.CreateRelative("../../../../TestResource.txt"));
    }

    /// <summary>
    ///     Utility method to compare a resource that contains a single string with
    ///     an exemplar.
    /// </summary>
    /// <param name="res">The resource to read a line from</param>
    /// <param name="expectedContent">the expected value of the line.</param>
    private void AssertResourceContent(IResource res, string expectedContent)
    {
        ClassicAssert.IsTrue(res.Exists);
        using var reader = new StreamReader(res.InputStream);
        ClassicAssert.AreEqual(expectedContent, reader.ReadLine(), "Resource content is not as expected");
    }
}
