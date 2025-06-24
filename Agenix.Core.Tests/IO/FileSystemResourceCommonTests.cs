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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.IO;

#endregion

namespace Agenix.Core.Tests.IO;

/// <summary>
///     Common Unit tests for all FileSystemResource derived classes.
/// </summary>
public abstract class FileSystemResourceCommonTests
{
    protected const string TemporaryFileName = "temp.file";

    protected abstract FileSystemResource CreateResourceInstance(string resourceName);

    /// <summary>
    ///     Creates a FileInfo instance representing the original location of the given assembly
    /// </summary>
    /// <remarks>
    ///     Use this instead of the "Assembly.Location" property to get the original location before shadow copying!
    /// </remarks>
    protected static FileInfo GetAssemblyLocation(Assembly assembly)
    {
        return new FileInfo(new Uri(assembly.Location).LocalPath);
    }

    protected static FileInfo CreateFileForTheCurrentDirectory()
    {
        return new FileInfo(Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemporaryFileName)));
    }

    [Test]
    public void CreateFileSystemResourceWithPathName()
    {
        var fileSystemResource = CreateResourceInstance(TemporaryFileName);
        ClassicAssert.AreEqual(TemporaryFileName, fileSystemResource.File.Name);
    }

    [Test]
    public void FileSystemResourceExists()
    {
        var file = GetAssemblyLocation(Assembly.GetExecutingAssembly());
        var fileSystemResource = CreateResourceInstance("~/" + file.Name);
        ClassicAssert.IsTrue(fileSystemResource.Exists);
    }

    [Test]
    public void FileSystemResourceNotExists()
    {
        ClassicAssert.IsFalse(CreateResourceInstance("asdfasfadf").Exists);
    }

    [Test]
    public void FileSystemResourceOpenNonExistanceFile()
    {
        var fileSystemResource = CreateResourceInstance(TemporaryFileName);
        Stream inputStream;
        ClassicAssert.Throws<FileNotFoundException>(() => inputStream = fileSystemResource.InputStream);
    }

    [Test]
    public void FileSystemResourceValidInputStream()
    {
        var file = GetAssemblyLocation(Assembly.GetExecutingAssembly());
        var fileSystemResource = CreateResourceInstance("~/" + file.Name);
        using (var inputStream = fileSystemResource.InputStream)
        {
            ClassicAssert.IsNotNull(inputStream);
        }
    }

    [Test]
    public void FileSystemResourceGivesOpenedInputStream()
    {
        var file = GetAssemblyLocation(Assembly.GetExecutingAssembly());
        var fileSystemResource = CreateResourceInstance("~/" + file.Name);
        using (var inputStream = fileSystemResource.InputStream)
        {
            ClassicAssert.IsTrue(inputStream.CanRead);
        }
    }

    [Test]
    public void GetDescription()
    {
        var fileSystemResource = CreateResourceInstance(TemporaryFileName);
        var expectedDescription = "file [" + fileSystemResource.File.FullName + "]";
        ClassicAssert.AreEqual(expectedDescription, fileSystemResource.Description);
    }

    [Test]
    public void GetURL()
    {
        var fileSystemResource = CreateResourceInstance(TemporaryFileName);
        ClassicAssert.IsNotNull(fileSystemResource.Uri);
    }

    /// <summary>
    ///     Even though the 'root' resource points to a nonexistent subdirectory, surfing 'up' to the parent
    ///     via a relative path should still work...
    /// </summary>
    [Test]
    public void CreateRelativeFromNonExistentOriginalResource()
    {
        // a suitable subdirectory of total pish
        IResource resource = CreateResourceInstance("dork/muller.venken");
        ClassicAssert.IsFalse(resource.Exists,
            "This test needs to feed off of a base resource that explicitly *doesn't* exist; but the resource seems to exist anyway.");
        var relative =
            resource.CreateRelative("../" + GetAssemblyLocation(Assembly.GetExecutingAssembly()).Name);
        ClassicAssert.IsTrue(relative.Exists);
    }

    [Test]
    public void CreateRelativeResourceIsEqualToOriginalAfterBouncingUpAndDownTheDirectoryTree()
    {
        IResource resource = new FileSystemResource(GetAssemblyLocation(Assembly.GetExecutingAssembly()).FullName);
        var relative =
            resource.CreateRelative("foo/bar/../../" + GetAssemblyLocation(Assembly.GetExecutingAssembly()).Name);
        ClassicAssert.IsTrue(relative.Exists);
        ClassicAssert.IsTrue(resource.Equals(relative));
    }

    [Test]
    public void CreateRelativeWithNullRelativePath()
    {
        IResource resource = CreateResourceInstance(".");
        Assert.Throws<ArgumentNullException>(() => resource.CreateRelative(null));
    }

    [Test]
    public void CreateRelativeWithEmptyRelativePath()
    {
        IResource resource = CreateResourceInstance("boba.licious");
        var relative = resource.CreateRelative(string.Empty);
        ClassicAssert.IsFalse(relative.Exists);
    }

    [Test]
    public void RelativeResourceWhenNotRelative()
    {
        IResource res = CreateResourceInstance("dummy.txt");
        IResource res2 = CreateResourceInstance("/index.html");

        var rel0 = res.CreateRelative("/index.html");
        ClassicAssert.IsTrue(rel0 is FileSystemResource);
        ClassicAssert.AreEqual(res2.File.FullName, rel0.File.FullName);
    }

    [Test]
    public void RelativeResourceFromRoot()
    {
        var res = CreateResourceInstance(@"/dummy.txt");
        FileSystemResource res2;

        var rel0 = res.CreateRelative("/index.html");
        ClassicAssert.IsTrue(rel0 is FileSystemResource);
        res2 = CreateResourceInstance("/index.html");
        ClassicAssert.AreEqual(res2.File.FullName, rel0.File.FullName);

        var rel1 = res.CreateRelative(@"index.html");
        ClassicAssert.IsTrue(rel1 is FileSystemResource);
        res2 = CreateResourceInstance("/index.html");
        ClassicAssert.AreEqual(res2.File.FullName, rel1.File.FullName);

        var rel2 = res.CreateRelative(@"samples/artfair/index.html");
        ClassicAssert.IsTrue(rel2 is FileSystemResource);
        res2 = CreateResourceInstance("/samples/artfair/index.html");
        ClassicAssert.AreEqual(res2.File.FullName, rel2.File.FullName);

        var rel3 = res.CreateRelative(@"./samples/artfair/index.html");
        ClassicAssert.IsTrue(rel3 is FileSystemResource);
        res2 = CreateResourceInstance("/samples/artfair/index.html");
        ClassicAssert.AreEqual(res2.File.FullName, rel3.File.FullName);
    }

    [Test]
    public void RelativeResourceFromSubfolder()
    {
        var res = CreateResourceInstance(@"/samples/artfair/dummy.txt");
        FileSystemResource resExpected;

        var rel0 = res.CreateRelative(@"/index.html");
        ClassicAssert.IsTrue(rel0 is FileSystemResource);
        resExpected = CreateResourceInstance("/index.html");
        ClassicAssert.AreEqual(resExpected.File.FullName, rel0.File.FullName);

        var rel1 = res.CreateRelative(@"index.html");
        ClassicAssert.IsTrue(rel1 is FileSystemResource);
        resExpected = CreateResourceInstance("/samples/artfair/index.html");
        ClassicAssert.AreEqual(resExpected.File.FullName, rel1.File.FullName);

        var rel2 = res.CreateRelative(@"demo\index.html");
        ClassicAssert.IsTrue(rel2 is FileSystemResource);
        resExpected = CreateResourceInstance("/samples/artfair/demo/index.html");
        ClassicAssert.AreEqual(resExpected.File.FullName, rel2.File.FullName);

        var rel3 = res.CreateRelative(@"./demo/index.html");
        ClassicAssert.IsTrue(rel3 is FileSystemResource);
        resExpected = CreateResourceInstance("/samples/artfair/demo/index.html");
        ClassicAssert.AreEqual(resExpected.File.FullName, rel3.File.FullName);

        var rel4 = res.CreateRelative(@"../calculator/index.html");
        ClassicAssert.IsTrue(rel4 is FileSystemResource);
        resExpected = CreateResourceInstance("/samples/calculator/index.html");
        ClassicAssert.AreEqual(resExpected.File.FullName, rel4.File.FullName);

        var rel5 = res.CreateRelative(@"..\..\index.html");
        resExpected = CreateResourceInstance("/index.html");
        ClassicAssert.AreEqual(resExpected.File.FullName, rel5.File.FullName);
    }

    [Test]
    public void RelativeResourceTooManyBackLevels()
    {
        var res = CreateResourceInstance("/samples/artfair/dummy.txt");
        Assert.Throws<UriFormatException>(() => res.CreateRelative("../../../index.html"));
    }

    [Test]
    public void SupportsAndResolvesTheSpecialHomeCharacter_SunnyDay()
    {
        var file =
            new FileInfo(Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemporaryFileName)));
        var writer = file.CreateText();
        var res = CreateResourceInstance("~/" + TemporaryFileName);
        ClassicAssert.IsTrue(res.File.Exists);
        ClassicAssert.AreEqual(file.FullName.ToLower(), res.File.FullName.ToLower());
        try
        {
            writer.Close();
        }
        catch (IOException)
        {
        }

        try
        {
            file.Delete();
        }
        catch (IOException)
        {
        }
    }

    [Test]
    [Ignore("problematic between framework versions")]
    public void SupportsAndResolvesTheSpecialHomeCharacter_SunnyDayEvenWithLeadingWhitespace()
    {
        var file =
            new FileInfo(Path.GetFullPath(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemporaryFileName)));
        var writer = file.CreateText();
        var res = CreateResourceInstance("    ~/" + TemporaryFileName);
        ClassicAssert.AreEqual(file.FullName.ToLower(), res.File.FullName.ToLower());
        try
        {
            writer.Close();
        }
        catch (IOException)
        {
        }

        try
        {
            file.Delete();
        }
        catch (IOException)
        {
        }
    }

    [Test]
    public void SupportsAndResolvesTheSpecialHomeCharacter_OnlyIfSpecialHomeCharacterIsFirstCharacter()
    {
        var res = CreateResourceInstance("foo~.txt");
        // must not have replaced ~; its only valid at the start of a resource name...
        ClassicAssert.AreEqual("foo~.txt", res.File.Name);
    }

    [Test]
    public void Resolution_PlainVanilla()
    {
        var file = CreateFileForTheCurrentDirectory();
        IResource res = CreateResourceInstance(TemporaryFileName);
        ClassicAssert.AreEqual(file.FullName.ToLower(), res.File.FullName.ToLower(),
            "The bare file name all by itself must have resolved to a file in the current " +
            "directory of the currently executing domain.");
    }

    [Test]
    public void Resolution_WithSpecialHomeCharacter()
    {
        var file = CreateFileForTheCurrentDirectory();
        IResource res = CreateResourceInstance("~/" + TemporaryFileName);
        ClassicAssert.AreEqual(file.FullName.ToLower(), res.File.FullName.ToLower(),
            "The file name with ~/ must have resolved to a file " +
            "in the current directory of the currently executing domain.");
    }
}
