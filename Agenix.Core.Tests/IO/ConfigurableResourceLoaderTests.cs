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
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.IO;

#endregion

namespace Agenix.Core.Tests.IO;

/// <summary>
///     Unit tests for the ConfigurableResourceLoader class.
/// </summary>
[TestFixture]
public sealed class ConfigurableResourceLoaderTests
{
    [SetUp]
    public void SetUp()
    {
        loader = new ConfigurableResourceLoader();
    }

    private ConfigurableResourceLoader loader;

    /// <summary>
    ///     Tests that loader correctly loads files specified by absolute name, regardless
    ///     of the fact whether protocol name is specified or not.
    /// </summary>
    [Test]
    public void GetAbsoluteFileSystemResource()
    {
        var fileName = Path.GetTempFileName();
        try
        {
            var withoutProtocol = loader.GetResource(fileName);
            ClassicAssert.IsNotNull(withoutProtocol, "Resource should not be null");
            ClassicAssert.IsTrue(withoutProtocol is FileSystemResource, "Expected FileSystemResource");
            ClassicAssert.IsTrue(withoutProtocol.Exists, "Resource should exist but it does not");

            var withProtocol = loader.GetResource("file:///" + fileName);
            ClassicAssert.IsNotNull(withProtocol, "Resource should not be null");
            ClassicAssert.IsTrue(withProtocol is FileSystemResource, "Expected FileSystemResource");
            ClassicAssert.IsTrue(withProtocol.Exists, "Resource should exist but it does not");
        }
        finally
        {
            new FileInfo(fileName).Delete();
        }
    }

    [Test]
    public void GetResourceThatSupportsTheSpecialHomeCharacter()
    {
        var filename = "foo.txt";
        var expectedFile =
            new FileInfo(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)));
        var writer = expectedFile.CreateText();
        var res = (FileSystemResource)loader.GetResource("~/" + filename);
        ClassicAssert.AreEqual(expectedFile.FullName, res.File.FullName);
        try
        {
            writer.Close();
        }
        catch (IOException)
        {
        }

        try
        {
            expectedFile.Delete();
        }
        catch (IOException)
        {
        }
    }

    [Test]
    public void GetResourceThatSupportsTheSpecialHomeCharacter_WithLeadingWhitespace()
    {
        var filename = "foo.txt";
        var expectedFile =
            new FileInfo(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename)));
        var writer = expectedFile.CreateText();
        var res = (FileSystemResource)loader.GetResource("   ~/" + filename);
        ClassicAssert.AreEqual(expectedFile.FullName, res.File.FullName);
        try
        {
            writer.Close();
        }
        catch (IOException)
        {
        }

        try
        {
            expectedFile.Delete();
        }
        catch (IOException)
        {
        }
    }

    /// <summary>
    ///     Tests that loader correctly loads files specified by relative name, regardless
    ///     of the fact whether protocol name is specified or not.
    /// </summary>
    [Test]
    public void GetRelativeFileSystemResource()
    {
        var fileName = "test.tmp";
        var fi = new FileInfo(fileName);
        var fs = fi.Create();
        fs.Close();

        try
        {
            var withoutProtocol = loader.GetResource(fileName);
            ClassicAssert.IsNotNull(withoutProtocol, "Resource should not be null");
            ClassicAssert.IsTrue(withoutProtocol is FileSystemResource, "Expected FileSystemResource");
            ClassicAssert.IsTrue(withoutProtocol.Exists, "Resource should exist but it does not");

            var withProtocol = loader.GetResource("file://" + fileName);
            ClassicAssert.IsNotNull(withProtocol, "Resource should not be null");
            ClassicAssert.IsTrue(withProtocol is FileSystemResource, "Expected FileSystemResource");
            ClassicAssert.IsTrue(withProtocol.Exists, "Resource should exist but it does not");
        }
        finally
        {
            fi.Delete();
        }
    }

    /// <summary>
    ///     Tests that loader can load UrlResource over HTTP protocol
    /// </summary>
    [Test]
    [Explicit]
    public void GetHttpUrlResource()
    {
        var res = loader.GetResource("http://www.springframework.net/license.html");
        ClassicAssert.IsNotNull(res, "Resource should not be null");
        ClassicAssert.AreEqual(typeof(UrlResource), res.GetType());
    }

    /// <summary>
    ///     Tests that loader can load UrlResource over assembly pseudo protocol
    /// </summary>
    [Test]
    public void GetAssemblyResource()
    {
        var res = loader.GetResource("assembly://Agenix.Core.Tests/Agenix.Core.Tests/TestResource.txt");
        ClassicAssert.IsNotNull(res, "Resource should not be null");
        ClassicAssert.AreEqual(typeof(AssemblyResource), res.GetType());
    }

    [Test]
    public void GetResourceForNonMappedProtocol()
    {
        ClassicAssert.Throws<UriFormatException>(() => new ConfigurableResourceLoader().GetResource("beep://foo.xml"));
    }
}
