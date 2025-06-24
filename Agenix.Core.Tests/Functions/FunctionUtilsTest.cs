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

using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Functions;

public class FunctionUtilsTest : AbstractNUnitSetUp
{
    [Test]
    public void TestResolveFunction()
    {
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:Concat('Hello',' TestFramework!')", Context),
            "Hello TestFramework!");

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:Concat('agenix',':core')", Context),
            "agenix:core");

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:Concat('agenix:core')", Context), "agenix:core");
    }

    [Test]
    public void TestWithVariables()
    {
        Context.SetVariable("greeting", "Hello");
        Context.SetVariable("text", "TestFramework!");

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:Concat('Hello',' ', ${text})", Context),
            "Hello TestFramework!");

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:Concat(${greeting},' ', ${text})", Context),
            "Hello TestFramework!");
    }

    [Test]
    public void TestWithCommaValue()
    {
        Context.SetVariable("greeting", "HeLl0");

        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction("agenix:UpperCase(agenix:Concat(${greeting},'W0rld'))", Context),
            "HELL0W0RLD");
        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction(
                "agenix:Concat(agenix:UpperCase('Yes'),' ',agenix:UpperCase('I like W0rld!'))",
                Context), "YES I LIKE W0RLD!");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:UpperCase('Monday, Tuesday, wednesday')", Context),
            "MONDAY, TUESDAY, WEDNESDAY");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:Concat('Monday, Tuesday',' wednesday')", Context),
            "Monday, Tuesday wednesday");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:UpperCase('Yes, I like W0rld!)", Context),
            "'YES, I LIKE W0RLD!");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:UpperCase(''Yes, I like W0rld!')", Context),
            "'YES, I LIKE W0RLD!");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:UpperCase('Yes, I like W0rld!')", Context),
            "YES, I LIKE W0RLD!");
        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction("agenix:UpperCase('Yes, I like W0rld, and this is great!')", Context),
            "YES, I LIKE W0RLD, AND THIS IS GREAT!");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:UpperCase('Yes,I like W0rld!')", Context),
            "YES,I LIKE W0RLD!");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:UpperCase('Yes', 'I like W0rld!')", Context),
            "YES");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:Concat('Hello Yes, I like W0rld!')", Context),
            "Hello Yes, I like W0rld!");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:Concat('Hello Yes,I like W0rld!')", Context),
            "Hello Yes,I like W0rld!");
        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction("agenix:Concat('Hello Yes,I like W0rld!, and this is great!')", Context),
            "Hello Yes,I like W0rld!, and this is great!");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:Concat('Hello Yes , I like W0rld!')", Context),
            "Hello Yes , I like W0rld!");
        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction("agenix:Concat('Hello Yes, I like W0rld!', 'Hello Yes,we like W0rld!')",
                Context), "Hello Yes, I like W0rld!Hello Yes,we like W0rld!");
    }

    [Test]
    public void TestUnknownFunctionLibrary()
    {
        Assert.Throws<NoSuchFunctionLibraryException>(() =>
            FunctionUtils.ResolveFunction("doesnotexist:EscapeXml('<Message>Hello Yes, I like W0rld!</Message>')",
                Context)
        );
    }

    [Test]
    public void TestUnknownFunction()
    {
        Assert.Throws<NoSuchFunctionException>(() =>
            FunctionUtils.ResolveFunction(
                "agenix:functiondoesnotexist('<Message>Hello Yes, I like W0rld!</Message>')", Context)
        );
    }

    [Test]
    public void TestInvalidFunction()
    {
        Assert.Throws<InvalidFunctionUsageException>(() =>
            FunctionUtils.ResolveFunction(
                "agenix:core", Context)
        );
    }

    [Test]
    public void TestWithNestedFunctions()
    {
        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction("agenix:Concat(agenix:CurrentDate('yyyy-mm-dd'))", Context),
            new CurrentDateFunction().Execute(new List<string> { "yyyy-mm-dd" }, Context));

        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction("agenix:Concat('Now is: ', agenix:CurrentDate('yyyy-mm-dd'))", Context),
            "Now is: " + new CurrentDateFunction().Execute(new List<string> { "yyyy-mm-dd" }, Context));

        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction(
                "agenix:Concat(agenix:CurrentDate('yyyy-mm-dd'),' ', agenix:Concat('Hello', ' Test Framework!'))",
                Context),
            new CurrentDateFunction().Execute(new List<string> { "yyyy-mm-dd" }, Context) + " Hello Test Framework!");
    }

    [Test]
    public void TestWithNestedFunctionsAndVariables()
    {
        Context.SetVariable("greeting", "Hello");
        Context.SetVariable("dateFormat", "yyyy-mm-dd");

        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction(
                "agenix:Concat(agenix:CurrentDate('${dateFormat}'),' ', agenix:Concat('${greeting}', ' TestFramework!'))",
                Context),
            new CurrentDateFunction().Execute(new List<string> { "yyyy-mm-dd" }, Context) + " Hello TestFramework!");
    }

    [Test]
    public void TestCurrentDateFunction()

    {
        // check the default date format 'dd.MM.yyyy'
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:CurrentDate()", Context),
            new CurrentDateFunction().Execute(new List<string> { "dd.MM.yyyy" }, Context));

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:CurrentDate('MM/dd/yyyy')", Context),
            new CurrentDateFunction().Execute(new List<string> { "MM/dd/yyyy" }, Context));

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:CurrentDate('MMMMM dd')", Context),
            new CurrentDateFunction().Execute(new List<string> { "MMMMM dd" }, Context));

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:CurrentDate('dddd, dd MMMMM yyyy')", Context),
            new CurrentDateFunction().Execute(new List<string> { "dddd, dd MMMMM yyyy" }, Context));
    }

    [Test]
    public void TestEncodeBase64Function()
    {
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:EncodeBase64('foo')", Context),
            new EncodeBase64Function().Execute(new List<string> { "foo" }, Context));
    }

    [Test]
    public void TestDecodeBase64Function()
    {
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("agenix:DecodeBase64('Zm9v')", Context),
            new DecodeBase64Function().Execute(new List<string> { "Zm9v" }, Context));
    }
}
