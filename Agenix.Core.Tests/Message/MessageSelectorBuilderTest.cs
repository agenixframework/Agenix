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

using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Message;

/// <summary>
///     This class contains unit tests for the MessageSelectorBuilder class.
/// </summary>
public class MessageSelectorBuilderTest
{
    [Test]
    public void TestToKeyValueDictionary()
    {
        var headerMap = MessageSelectorBuilder.WithString("foo = 'bar'").ToKeyValueDictionary();

        ClassicAssert.AreEqual(1, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foo"));
        ClassicAssert.AreEqual("bar", headerMap["foo"]);

        headerMap = MessageSelectorBuilder.WithString("foo = 'bar' AND operation = 'foo'").ToKeyValueDictionary();

        ClassicAssert.AreEqual(2, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foo"));
        ClassicAssert.AreEqual("bar", headerMap["foo"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("operation"));
        ClassicAssert.AreEqual("foo", headerMap["operation"]);

        headerMap = MessageSelectorBuilder.WithString("foo='bar' AND operation='foo'").ToKeyValueDictionary();

        ClassicAssert.AreEqual(2, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foo"));
        ClassicAssert.AreEqual("bar", headerMap["foo"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("operation"));
        ClassicAssert.AreEqual("foo", headerMap["operation"]);

        headerMap = MessageSelectorBuilder.WithString("foo='bar' AND operation='foo' AND foobar='true'")
            .ToKeyValueDictionary();

        ClassicAssert.AreEqual(3, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foo"));
        ClassicAssert.AreEqual("bar", headerMap["foo"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("operation"));
        ClassicAssert.AreEqual("foo", headerMap["operation"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foobar"));
        ClassicAssert.AreEqual("true", headerMap["foobar"]);

        headerMap = MessageSelectorBuilder.WithString("A='Avalue' AND B='Bvalue' AND N='Nvalue'")
            .ToKeyValueDictionary();

        ClassicAssert.AreEqual(3, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("A"));
        ClassicAssert.AreEqual("Avalue", headerMap["A"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("B"));
        ClassicAssert.AreEqual("Bvalue", headerMap["B"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("N"));
        ClassicAssert.AreEqual("Nvalue", headerMap["N"]);

        headerMap = MessageSelectorBuilder.WithString("foo='OPERAND' AND bar='ANDROID'").ToKeyValueDictionary();

        ClassicAssert.AreEqual(2, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foo"));
        ClassicAssert.AreEqual("OPERAND", headerMap["foo"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("bar"));
        ClassicAssert.AreEqual("ANDROID", headerMap["bar"]);

        headerMap = MessageSelectorBuilder.WithString("foo='ANDROID'").ToKeyValueDictionary();

        ClassicAssert.AreEqual(1, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("foo"));
        ClassicAssert.AreEqual("ANDROID", headerMap["foo"]);

        headerMap = MessageSelectorBuilder.WithString("xpath://foo[@key='primary']/value='bar' AND operation='foo'")
            .ToKeyValueDictionary();

        ClassicAssert.AreEqual(2, headerMap.Count);
        ClassicAssert.IsTrue(headerMap.ContainsKey("xpath://foo[@key='primary']/value"));
        ClassicAssert.AreEqual("bar", headerMap["xpath://foo[@key='primary']/value"]);
        ClassicAssert.IsTrue(headerMap.ContainsKey("operation"));
        ClassicAssert.AreEqual("foo", headerMap["operation"]);
    }
}
