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
///     Unit tests for the FileSystemResourceTest class.
/// </summary>
/// <author>Rick Evans</author>
[Platform("Win")]
public class FileSystemResourceTests : FileSystemResourceCommonTests
{
    protected override FileSystemResource CreateResourceInstance(string resourceName)
    {
        return new FileSystemResource(resourceName);
    }

    [Test]
    public void LeadingProtocolIsNotTreatedRelative()
    {
        var res = new FileSystemResource(@"file://\\server\share\samples\artfair\");
        var res2 = (FileSystemResource)res.CreateRelative(@"file://./index.html");
        ClassicAssert.AreEqual(new Uri(Path.Combine(Environment.CurrentDirectory, "index.html")).AbsolutePath,
            res2.Uri.AbsolutePath);
    }

    [Test]
    public void RelativeUncResourceTooManyBackLevels()
    {
        var res = new FileSystemResource(@"\\server\share\samples\artfair\dummy.txt");
        Assert.Throws<UriFormatException>(() => res.CreateRelative(@"..\..\..\index.html"));
    }

    [Test(Description = "SPRNET-89")]
    public void SPRNET_89_SupportUrlEncodedLocationsWithSpaces()
    {
        var path = Path.GetFullPath("spaced dir");
        Directory.CreateDirectory(path);
        var filePath = Path.Combine(path, "spaced file.txt");
        using (var text = File.CreateText(filePath))
        {
            text.WriteLine("hello world");
        }

        var res = new FileSystemResource(new Uri(filePath).AbsoluteUri);
        using (var s = res.InputStream)
        {
            using (TextReader reader = new StreamReader(s))
            {
                ClassicAssert.AreEqual("hello world", reader.ReadLine());
            }
        }
    }

    [Test(Description = "http://opensource.atlassian.com/projects/spring/browse/SPRNET-320")]
    public void SupportsSpecialUriCharacter()
    {
        var path = Path.GetFullPath("dir#");
        Directory.CreateDirectory(path);
        var filePath = Path.Combine(path, "file.txt");
        using (var text = File.CreateText(filePath))
        {
            text.WriteLine("hello world");
        }

        var res = new FileSystemResource(new Uri(filePath).AbsoluteUri);
        using (var s = res.InputStream)
        {
            using (TextReader reader = new StreamReader(s))
            {
                ClassicAssert.AreEqual("hello world", reader.ReadLine());
            }
        }
    }

    [Test]
    public void Resolution_WithProtocolAndSpecialHomeCharacter()
    {
        var file = CreateFileForTheCurrentDirectory();
        IResource res = new FileSystemResource("file://~/" + TemporaryFileName);
        ClassicAssert.AreEqual(file.FullName, res.File.FullName,
            "The file name with file://~ must have resolved to a file " +
            "in the current directory of the currently executing domain.");
    }

    [Test]
    public void Resolution_WithProtocolAndSpecialHomeCharacterParentDirectory()
    {
        var file = new FileInfo(Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TemporaryFileName)));
        IResource res = new FileSystemResource("file://~/../" + TemporaryFileName);
        var fileNameOneDirectoryUp = file.Directory.Parent.FullName + "\\" + TemporaryFileName;
        ClassicAssert.AreEqual(fileNameOneDirectoryUp, res.File.FullName,
            "The file name with file://~/.. must have resolved to a file " +
            "in the parent directory of the currently executing domain.");
    }

    [Test]
    public void CreateRelativeWithParent()
    {
        // use the filename of the declaring assembly as the root...
        var rootpath = GetType().Assembly.Location;
        var rootdir = new DirectoryInfo(Path.GetDirectoryName(rootpath));
        _testCreateRelative(rootpath, rootdir.Parent.FullName, @"../");
    }

    [Test]
    public void CreateRelativeWithSuperParent()
    {
        // use the filename of the declaring assembly as the root...
        var rootpath = GetType().Assembly.Location;
        var rootdir = new DirectoryInfo(Path.GetDirectoryName(rootpath));
        _testCreateRelative(rootpath, rootdir.Parent.Parent.FullName, @"..\..\");
    }

    [Test]
    public void CreateRelativeInSameDirectory()
    {
        // use the filename of the declaring assembly as the root...
        var rootpath = GetType().Assembly.Location;
        var rootdir = new DirectoryInfo(Path.GetDirectoryName(rootpath));
        _testCreateRelative(rootpath, rootdir.FullName, @".\");
    }

    [Test]
    public void CreateRelativeInSubdirectoryDirectory()
    {
        var subdirname = "Stuff";
        // use the filename of the declaring assembly as the root...
        var rootpath = GetType().Assembly.Location;
        var rootdir = new DirectoryInfo(Path.GetDirectoryName(rootpath));
        var subdir = rootdir.CreateSubdirectory(subdirname);
        try
        {
            _testCreateRelative(rootpath, subdir.FullName, subdirname + "/");
            // let's get obtuse... specify the parent directory of the subdirectory (i.e. the root directory again)
            _testCreateRelative(rootpath, rootdir.FullName, subdirname + "/../");
        }
        finally
        {
            if (subdir.Exists)
            {
                try
                {
                    subdir.Delete();
                }
                catch (IOException)
                {
                }
            }
        }
    }

    /// <summary>
    ///     Helper method...
    /// </summary>
    private void _testCreateRelative(string rootPath, string targetPath, string relativePath)
    {
        var filename = "stuff.txt";
        var file = new FileInfo(Path.GetFullPath(Path.Combine(targetPath, filename)));
        // create a temporary file in whatever 'targetpath' dir we've been passed...
        var writer = file.CreateText();
        // test that the CreateRelative () method works with the supplied 'relativePath'
        IResource resource = new FileSystemResource(rootPath);
        var relative = resource.CreateRelative(relativePath + filename);
        ClassicAssert.IsTrue(relative.Exists);
        if (file.Exists)
        {
            try
            {
                writer.Close();
                file.Delete();
            }
            catch (IOException)
            {
            }
        }
    }

    [Test]
    public void RelativeLocalFileSystemResourceWhenNotRelative()
    {
        var res = new FileSystemResource(@"C:\dummy.txt");

        var rel0 = res.CreateRelative(@"c:\index.html");
        ClassicAssert.IsTrue(rel0 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [c:\index.html]", rel0.Description);
    }

    [Test]
    public void RelativeLocalFileSystemResourceFromRoot()
    {
        var res = new FileSystemResource(@"c:\dummy.txt");

        var rel0 = res.CreateRelative(@"\index.html");
        ClassicAssert.IsTrue(rel0 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [c:\index.html]", rel0.Description);

        var rel1 = res.CreateRelative(@"index.html");
        ClassicAssert.IsTrue(rel1 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [c:\index.html]", rel1.Description);

        var rel2 = res.CreateRelative(@"samples/artfair/index.html");
        ClassicAssert.IsTrue(rel2 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [c:\samples\artfair\index.html]", rel2.Description);

        var rel3 = res.CreateRelative(@".\samples\artfair\index.html");
        ClassicAssert.IsTrue(rel3 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [c:\samples\artfair\index.html]", rel3.Description);
    }

    [Test]
    public void RelativeLocalFileSystemResourceFromSubfolder()
    {
        var res = new FileSystemResource(@"c:\samples\artfair\dummy.txt");

        var rel0 = res.CreateRelative(@"/index.html");
        ClassicAssert.IsTrue(rel0 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [c:\index.html]", rel0.Description);

        var rel1 = res.CreateRelative(@"index.html");
        ClassicAssert.IsTrue(rel1 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [c:\samples\artfair\index.html]", rel1.Description);

        var rel2 = res.CreateRelative(@"demo\index.html");
        ClassicAssert.IsTrue(rel2 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [c:\samples\artfair\demo\index.html]", rel2.Description);

        var rel3 = res.CreateRelative(@"./demo/index.html");
        ClassicAssert.IsTrue(rel3 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [c:\samples\artfair\demo\index.html]", rel3.Description);

        var rel4 = res.CreateRelative(@"../calculator/index.html");
        ClassicAssert.IsTrue(rel4 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [c:\samples\calculator\index.html]", rel4.Description);

        var rel5 = res.CreateRelative(@"..\..\index.html");
        ClassicAssert.IsTrue(rel5 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [c:\index.html]", rel5.Description);
    }

    [Test]
    public void RelativeUncResourceWhenNotRelative()
    {
        var res = new FileSystemResource(@"\\server\share\dummy.txt");

        var rel0 = res.CreateRelative(@"\\server\share\index.html");
        ClassicAssert.IsTrue(rel0 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [\\server\share\index.html]", rel0.Description);
    }

    [Test]
    public void RelativeUncResourceFromRoot()
    {
        var res = new FileSystemResource(@"\\server\share\dummy.txt");

        var rel0 = res.CreateRelative(@"\index.html");
        ClassicAssert.IsTrue(rel0 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [\\server\share\index.html]", rel0.Description);

        var rel1 = res.CreateRelative(@"index.html");
        ClassicAssert.IsTrue(rel1 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [\\server\share\index.html]", rel1.Description);

        var rel2 = res.CreateRelative(@"samples/artfair/index.html");
        ClassicAssert.IsTrue(rel2 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [\\server\share\samples\artfair\index.html]", rel2.Description);

        var rel3 = res.CreateRelative(@".\samples\artfair\index.html");
        ClassicAssert.IsTrue(rel3 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [\\server\share\samples\artfair\index.html]", rel3.Description);
    }

    [Test]
    public void RelativeUncResourceFromSubfolder()
    {
        var res = new FileSystemResource(@"\\server\share\samples\artfair\dummy.txt");

        var rel0 = res.CreateRelative(@"/index.html");
        ClassicAssert.IsTrue(rel0 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [\\server\share\index.html]", rel0.Description);

        var rel1 = res.CreateRelative(@"index.html");
        ClassicAssert.IsTrue(rel1 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [\\server\share\samples\artfair\index.html]", rel1.Description);

        var rel2 = res.CreateRelative(@"demo\index.html");
        ClassicAssert.IsTrue(rel2 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [\\server\share\samples\artfair\demo\index.html]", rel2.Description);

        var rel3 = res.CreateRelative(@"./demo/index.html");
        ClassicAssert.IsTrue(rel3 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [\\server\share\samples\artfair\demo\index.html]", rel3.Description);

        var rel4 = res.CreateRelative(@"../calculator/index.html");
        ClassicAssert.IsTrue(rel4 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [\\server\share\samples\calculator\index.html]", rel4.Description);

        var rel5 = res.CreateRelative(@"..\..\index.html");
        ClassicAssert.IsTrue(rel5 is FileSystemResource);
        ClassicAssert.AreEqual(@"file [\\server\share\index.html]", rel5.Description);
    }
}
