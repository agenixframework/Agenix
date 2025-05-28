using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Core.Functions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Functions;

public class FunctionUtilsTest : AbstractNUnitSetUp
{
    [Test]
    public void TestResolveFunction()
    {
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:Concat('Hello',' TestFramework!')", Context),
            "Hello TestFramework!");

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:Concat('core',':core')", Context), "core:core");

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:Concat('core:core')", Context), "core:core");
    }

    [Test]
    public void TestWithVariables()
    {
        Context.SetVariable("greeting", "Hello");
        Context.SetVariable("text", "TestFramework!");

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:Concat('Hello',' ', ${text})", Context),
            "Hello TestFramework!");

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:Concat(${greeting},' ', ${text})", Context),
            "Hello TestFramework!");
    }

    [Test]
    public void TestWithCommaValue()
    {
        Context.SetVariable("greeting", "HeLl0");

        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction("core:UpperCase(core:Concat(${greeting},'W0rld'))", Context),
            "HELL0W0RLD");
        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction("core:Concat(core:UpperCase('Yes'),' ',core:UpperCase('I like W0rld!'))",
                Context), "YES I LIKE W0RLD!");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:UpperCase('Monday, Tuesday, wednesday')", Context),
            "MONDAY, TUESDAY, WEDNESDAY");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:Concat('Monday, Tuesday',' wednesday')", Context),
            "Monday, Tuesday wednesday");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:UpperCase('Yes, I like W0rld!)", Context),
            "'YES, I LIKE W0RLD!");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:UpperCase(''Yes, I like W0rld!')", Context),
            "'YES, I LIKE W0RLD!");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:UpperCase('Yes, I like W0rld!')", Context),
            "YES, I LIKE W0RLD!");
        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction("core:UpperCase('Yes, I like W0rld, and this is great!')", Context),
            "YES, I LIKE W0RLD, AND THIS IS GREAT!");
        //Assert.AreEqual(FunctionUtils.ResolveFunction("core:UpperCase('Yes,I like W0rld!')", Context), "YES,I LIKE W0RLD!");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:UpperCase('Yes', 'I like W0rld!')", Context), "YES");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:Concat('Hello Yes, I like W0rld!')", Context),
            "Hello Yes, I like W0rld!");
        //Assert.AreEqual(FunctionUtils.ResolveFunction("core:Concat('Hello Yes,I like W0rld!')", Context), "Hello Yes,I like W0rld!");
        //Assert.AreEqual(FunctionUtils.ResolveFunction("core:Concat('Hello Yes,I like W0rld!, and this is great')", Context), "Hello Yes,I like W0rld!, and this is great!");
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:Concat('Hello Yes , I like W0rld!')", Context),
            "Hello Yes , I like W0rld!");
        //Assert.AreEqual(FunctionUtils.ResolveFunction("core:Concat('Hello Yes, I like W0rld!', 'Hello Yes,we like W0rld!')", Context), "Hello Yes, I like W0rld!Hello Yes,we like W0rld!");

        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction("core:EscapeXml('<Message>Hello Yes, I like W0rld!</Message>')", Context),
            "&lt;Message&gt;Hello Yes, I like W0rld!&lt;/Message&gt;");
        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction("core:EscapeXml('<Message>Hello Yes , I like W0rld!</Message>')",
                Context), "&lt;Message&gt;Hello Yes , I like W0rld!&lt;/Message&gt;");
        // Assert.AreEqual(FunctionUtils.ResolveFunction("core:EscapeXml('<Message>Hello Yes,I like W0rld, and this is great!</Message>')", Context), "&lt;Message&gt;Hello Yes,I like W0rld, and this is great!&lt;/Message&gt;");
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
                "core:functiondoesnotexist('<Message>Hello Yes, I like W0rld!</Message>')", Context)
        );
    }

    [Test]
    public void TestInvalidFunction()
    {
        Assert.Throws<InvalidFunctionUsageException>(() =>
            FunctionUtils.ResolveFunction(
                "core:core", Context)
        );
    }

    [Test]
    public void TestWithNestedFunctions()
    {
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:Concat(core:CurrentDate('yyyy-mm-dd'))", Context),
            new CurrentDateFunction().Execute(new List<string> { "yyyy-mm-dd" }, Context));

        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction("core:Concat('Now is: ', core:CurrentDate('yyyy-mm-dd'))", Context),
            "Now is: " + new CurrentDateFunction().Execute(new List<string> { "yyyy-mm-dd" }, Context));

        ClassicAssert.AreEqual(
            FunctionUtils.ResolveFunction(
                "core:Concat(core:CurrentDate('yyyy-mm-dd'),' ', core:Concat('Hello', ' Test Framework!'))",
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
                "core:Concat(core:CurrentDate('${dateFormat}'),' ', core:Concat('${greeting}', ' TestFramework!'))",
                Context),
            new CurrentDateFunction().Execute(new List<string> { "yyyy-mm-dd" }, Context) + " Hello TestFramework!");
    }

    [Test]
    public void TestCurrentDateFunction()

    {
        // check the default date format 'dd.MM.yyyy'
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:CurrentDate()", Context),
            new CurrentDateFunction().Execute(new List<string> { "dd.MM.yyyy" }, Context));

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:CurrentDate('MM/dd/yyyy')", Context),
            new CurrentDateFunction().Execute(new List<string> { "MM/dd/yyyy" }, Context));

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:CurrentDate('MMMMM dd')", Context),
            new CurrentDateFunction().Execute(new List<string> { "MMMMM dd" }, Context));

        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:CurrentDate('dddd, dd MMMMM yyyy')", Context),
            new CurrentDateFunction().Execute(new List<string> { "dddd, dd MMMMM yyyy" }, Context));
    }

    [Test]
    public void TestEncodeBase64Function()
    {
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:EncodeBase64('foo')", Context),
            new EncodeBase64Function().Execute(new List<string> { "foo" }, Context));
    }

    [Test]
    public void TestDecodeBase64Function()
    {
        ClassicAssert.AreEqual(FunctionUtils.ResolveFunction("core:DecodeBase64('Zm9v')", Context),
            new DecodeBase64Function().Execute(new List<string> { "Zm9v" }, Context));
    }
}
