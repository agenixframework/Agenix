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

using System;
using System.Collections.Generic;
using System.Text;
using Agenix.Api.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Util;

public class DefaultTypeConverterTest
{
    private readonly DefaultTypeConverter _converter = DefaultTypeConverter.Instance;

    [Test]
    public void TestConvertIfNecessary()
    {
        var payload = "Hello Agenix!";

        ClassicAssert.AreEqual("[foo, bar]",
            _converter.ConvertIfNecessary<string>(new List<string> { "foo", "bar" }, typeof(string)));
        ClassicAssert.AreEqual("[foo, bar]",
            _converter.ConvertIfNecessary<string>(new[] { "foo", "bar" }, typeof(string)));
        ClassicAssert.AreEqual("[foo, bar]",
            _converter.ConvertIfNecessary<string>(new object[] { "foo", "bar" }, typeof(string)));
        ClassicAssert.AreEqual("{foo=bar}",
            _converter.ConvertIfNecessary<string>(new Dictionary<object, object> { { "foo", "bar" } }, typeof(string)));
        ClassicAssert.AreEqual("null", _converter.ConvertIfNecessary<string>(null, typeof(string)));
        ClassicAssert.AreEqual(Convert.ToByte(1), _converter.ConvertIfNecessary<byte>("1", typeof(byte)));
        ClassicAssert.AreEqual(1, _converter.ConvertIfNecessary<byte>(1, typeof(byte)));
        ClassicAssert.AreEqual(payload, _converter.ConvertIfNecessary<string>(payload, typeof(string)));
        ClassicAssert.AreEqual(Encoding.UTF8.GetBytes(payload),
            _converter.ConvertIfNecessary<byte[]>(payload, typeof(byte[])));
        ClassicAssert.AreEqual(1, _converter.ConvertIfNecessary<short>("1", typeof(short)));
        ClassicAssert.AreEqual(1, _converter.ConvertIfNecessary<short>(1, typeof(short)));
        ClassicAssert.AreEqual(1, _converter.ConvertIfNecessary<int>("1", typeof(int)));
        ClassicAssert.AreEqual(1, _converter.ConvertIfNecessary<int>(1, typeof(int)));
        ClassicAssert.AreEqual(1L, _converter.ConvertIfNecessary<long>("1", typeof(long)));
        ClassicAssert.AreEqual(1L, _converter.ConvertIfNecessary<long>(1L, typeof(long)));
        ClassicAssert.AreEqual(1.0F, _converter.ConvertIfNecessary<float>("1", typeof(float)));
        ClassicAssert.AreEqual(1.0F, _converter.ConvertIfNecessary<float>(1.0F, typeof(float)));
        ClassicAssert.AreEqual(1.0D, _converter.ConvertIfNecessary<double>("1", typeof(double)));
        ClassicAssert.AreEqual(1.0D, _converter.ConvertIfNecessary<double>(1.0D, typeof(double)));
        ClassicAssert.AreEqual(true, _converter.ConvertIfNecessary<bool>("true", typeof(bool)));
        ClassicAssert.AreEqual(false, _converter.ConvertIfNecessary<bool>(false, typeof(bool)));
        ClassicAssert.AreEqual(3, _converter.ConvertIfNecessary<string[]>("[a,b,c]", typeof(string[])).Length);
        ClassicAssert.AreEqual(3, _converter.ConvertIfNecessary<string[]>("[a, b, c]", typeof(string[])).Length);
        ClassicAssert.AreEqual(3, _converter.ConvertIfNecessary<List<string>>("[a,b,c]", typeof(List<string>)).Count);
        ClassicAssert.AreEqual(3, _converter.ConvertIfNecessary<List<string>>("[a, b, c]", typeof(List<string>)).Count);
        ClassicAssert.AreEqual("value",
            _converter.ConvertIfNecessary<Dictionary<string, object>>("{key=value}", typeof(Dictionary<string, object>))
                ["key"]);
        ClassicAssert.AreEqual("value2",
            _converter.ConvertIfNecessary<Dictionary<string, object>>("{key1=value1, key2=value2}",
                typeof(Dictionary<string, object>))["key2"]);
        ClassicAssert.AreEqual("value2",
            _converter.ConvertIfNecessary<Dictionary<string, object>>("{key1=value1,key2=value2}",
                typeof(Dictionary<string, object>))["key2"]);
        ClassicAssert.AreEqual("[1, 2]", _converter.ConvertIfNecessary<string>(new[] { 1, 2 }, typeof(string)));

        // TODO: fix below causes
        //ClassicAssert.AreEqual("[1, 2]", _converter.ConvertIfNecessary<string>(new List<int> { 1, 2 }, typeof(string)));
        ClassicAssert.AreEqual("[1, 2]", _converter.ConvertIfNecessary<string>(new[] { 1, 2 }, typeof(string)));
    }
}
