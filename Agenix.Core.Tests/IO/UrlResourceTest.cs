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
///     Unit tests for the UrlResource class.
/// </summary>
[TestFixture]
public sealed class UrlResourceTests
{
    private const string FILE_PROTOCOL_PREFIX = "file:///";

    [Test]
    public void CreateUrlResourceWithGivenPath()
    {
        var urlResource = new UrlResource(FILE_PROTOCOL_PREFIX + "C:/temp");
        ClassicAssert.AreEqual("C:/temp", urlResource.Uri.AbsolutePath);
    }

    [Test]
    public void CreateInvalidUrlResource()
    {
        string uri = null;
        ClassicAssert.Throws<ArgumentNullException>(() => new UrlResource(uri));
    }

    [Test]
    [Platform("Win")]
    public void GetValidFileInfo()
    {
        var urlResource = new UrlResource(FILE_PROTOCOL_PREFIX + "C:/temp");
        ClassicAssert.AreEqual("C:\\temp", urlResource.File.FullName);
    }

    [Test]
    [Explicit]
    public void ExistsValidHttp()
    {
        var urlResource = new UrlResource("https://www.springframework.net/");
        ClassicAssert.IsTrue(urlResource.Exists);
    }

    [Test]
    public void GetInvalidFileInfo()
    {
        var urlResource = new UrlResource("http://www.springframework.net/");
        FileInfo file;
        Assert.Throws<FileNotFoundException>(() => file = urlResource.File);
    }

    [Test]
    public void GetInvalidFileInfoWithOddPort()
    {
        var urlResource = new UrlResource("http://www.springframework.net:76/");
        FileInfo temp;
        Assert.Throws<FileNotFoundException>(() => temp = urlResource.File);
    }

    [Test]
    public void GetDescription()
    {
        var urlResource = new UrlResource(FILE_PROTOCOL_PREFIX + "C:/temp");
        ClassicAssert.AreEqual("URL [file:///C:/temp]", urlResource.Description);
    }

    [Test]
    public void GetValidInputStreamForFileProtocol()
    {
        var fileName = Path.GetTempFileName();
        var fs = File.Create(fileName);
        fs.Close();
        using (var inputStream = new UrlResource(FILE_PROTOCOL_PREFIX + fileName).InputStream)
        {
            ClassicAssert.IsTrue(inputStream.CanRead);
        }
    }

    [Test]
    public void RelativeResourceFromRoot()
    {
        var res = new UrlResource("http://www.springframework.net/documentation.html");

        var rel0 = res.CreateRelative("/index.html");
        ClassicAssert.IsTrue(rel0 is UrlResource);
        ClassicAssert.AreEqual("URL [http://www.springframework.net/index.html]", rel0.Description);

        var rel1 = res.CreateRelative("index.html");
        ClassicAssert.IsTrue(rel1 is UrlResource);
        ClassicAssert.AreEqual("URL [http://www.springframework.net/index.html]", rel1.Description);

        var rel2 = res.CreateRelative("samples/artfair/index.html");
        ClassicAssert.IsTrue(rel2 is UrlResource);
        ClassicAssert.AreEqual("URL [http://www.springframework.net/samples/artfair/index.html]", rel2.Description);

        var rel3 = res.CreateRelative("./samples/artfair/index.html");
        ClassicAssert.IsTrue(rel3 is UrlResource);
        ClassicAssert.AreEqual("URL [http://www.springframework.net/samples/artfair/index.html]", rel3.Description);
    }

    [Test]
    public void RelativeResourceFromSubfolder()
    {
        var res = new UrlResource("http://www.springframework.net/samples/artfair/download.html");

        var rel0 = res.CreateRelative("/index.html");
        ClassicAssert.IsTrue(rel0 is UrlResource);
        ClassicAssert.AreEqual("URL [http://www.springframework.net/index.html]", rel0.Description);

        var rel1 = res.CreateRelative("index.html");
        ClassicAssert.IsTrue(rel1 is UrlResource);
        ClassicAssert.AreEqual("URL [http://www.springframework.net/samples/artfair/index.html]", rel1.Description);

        var rel2 = res.CreateRelative("demo/index.html");
        ClassicAssert.IsTrue(rel2 is UrlResource);
        ClassicAssert.AreEqual("URL [http://www.springframework.net/samples/artfair/demo/index.html]",
            rel2.Description);

        var rel3 = res.CreateRelative("./demo/index.html");
        ClassicAssert.IsTrue(rel3 is UrlResource);
        ClassicAssert.AreEqual("URL [http://www.springframework.net/samples/artfair/demo/index.html]",
            rel3.Description);

        var rel4 = res.CreateRelative("../calculator/index.html");
        ClassicAssert.IsTrue(rel4 is UrlResource);
        ClassicAssert.AreEqual("URL [http://www.springframework.net/samples/calculator/index.html]", rel4.Description);

        var rel5 = res.CreateRelative("../../index.html");
        ClassicAssert.IsTrue(rel5 is UrlResource);
        ClassicAssert.AreEqual("URL [http://www.springframework.net/index.html]", rel5.Description);
    }

    [Test]
    public void RelativeResourceTooManyBackLevels()
    {
        var res = new UrlResource("http://www.springframework.net/samples/artfair/download.html");
        Assert.Throws<UriFormatException>(() => res.CreateRelative("../../../index.html"));
    }
}
