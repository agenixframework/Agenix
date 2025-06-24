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
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.IO;

#endregion

namespace Agenix.Core.Tests.IO;

/// <summary>
///     Unit tests for the <see cref="StringResource" /> class, ensuring proper functionality and behavior under various
///     scenarios.
///     These tests verify attributes such as content retrieval, encoding, stream support, and resource existence.
/// </summary>
[TestFixture]
public class StringResourceTests
{
    [Test]
    public void EnsureDefaults()
    {
        var enc = Encoding.Default;
        var FOO_CONTENT = "foo";
        var FOO_DESCRIPTION = "foo description";

        var r = new StringResource(FOO_CONTENT);
        ClassicAssert.AreEqual(FOO_CONTENT, r.Content);
        ClassicAssert.AreEqual(enc, r.Encoding);
        ClassicAssert.AreEqual(string.Empty, r.Description);

#pragma warning disable SYSLIB0001
        enc = new UTF7Encoding();
#pragma warning restore SYSLIB0001
        r = new StringResource(FOO_CONTENT, enc, FOO_DESCRIPTION);
        ClassicAssert.AreEqual(FOO_CONTENT, r.Content);
        ClassicAssert.AreEqual(enc, r.Encoding);
        ClassicAssert.AreEqual(FOO_DESCRIPTION, r.Description);
    }

    [Test]
    public void ReturnsCorrectEncodedStream()
    {
        var FOO_CONTENT = "foo\u4567";
        var r = new StringResource(FOO_CONTENT, Encoding.GetEncoding("utf-16"));
        ClassicAssert.AreEqual(FOO_CONTENT, r.Content);
        var istm = r.InputStream;
        ClassicAssert.IsTrue(istm.CanRead);

        var chars = new byte[istm.Length];
        istm.Read(chars, 0, chars.Length);
        istm.Close();
        var result = Encoding.GetEncoding("utf-16").GetString(chars);
        ClassicAssert.AreEqual(FOO_CONTENT, result);
    }

    [Test]
    public void DoesntSupportRelativeResources()
    {
        var r = new StringResource(string.Empty);
        ClassicAssert.Throws<NotSupportedException>(() => r.CreateRelative("foo"));
    }

    [Test]
    public void AcceptsNullContent()
    {
#pragma warning disable SYSLIB0001
        Encoding utf7 = new UTF7Encoding();
#pragma warning restore SYSLIB0001
        var r = new StringResource(null, utf7);
        ClassicAssert.AreEqual(string.Empty, r.Content);
        var stm = r.InputStream;
        ClassicAssert.IsTrue(stm.CanRead);
        ClassicAssert.IsNotNull(stm);
        ClassicAssert.AreEqual(0, stm.Length);
        stm.Close();
    }

    [Test]
    public void AlwaysExists()
    {
        var r = new StringResource(null);
        ClassicAssert.IsTrue(r.Exists);
        r = new StringResource(string.Empty);
        ClassicAssert.IsTrue(r.Exists);
        r = new StringResource("foo");
        ClassicAssert.IsTrue(r.Exists);
    }
}
