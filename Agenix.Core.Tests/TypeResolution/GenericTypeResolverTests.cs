#region Imports

using System;
using System.Collections.Generic;
using Agenix.Core.Tests.TypeResolution;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.TypeResolution;

#endregion

namespace Agenix.Core.Tests.TypeResolution;

/// <summary>
///     Unit tests for the GenericTypeResolver class.
/// </summary>
[TestFixture]
public class GenericTypeResolverTests : TypeResolverTests
{
    protected override ITypeResolver GetTypeResolver()
    {
        return new GenericTypeResolver();
    }

    [Test]
    public void ResolveLocalAssemblyGenericType()
    {
        var t = GetTypeResolver()
            .Resolve("Agenix.Core.Tests.TypeResolution.TestGenericObject< int, string>");
        ClassicAssert.AreEqual(typeof(TestGenericObject<int, string>), t);
    }

    [Test]
    public void ResolveLocalAssemblyGenericTypeDefinition()
    {
        // CLOVER:ON
        var t = GetTypeResolver().Resolve("Agenix.Core.Tests.TypeResolution.TestGenericObject< ,>");
        // CLOVER:OFF
        ClassicAssert.AreEqual(typeof(TestGenericObject<,>), t);
    }

    [Test]
    public void ResolveLocalAssemblyGenericTypeOpen()
    {
        Assert.Throws<TypeLoadException>(() =>
            GetTypeResolver().Resolve("Agenix.Core.Tests.TypeResolution.TestGenericObject<int >"));
    }

    [Test]
    public void ResolveGenericTypeWithAssemblyName()
    {
        var t = GetTypeResolver().Resolve("System.Collections.Generic.Stack<string>, System");
        ClassicAssert.AreEqual(typeof(Stack<string>), t);
    }

    [Test]
    public void ResolveGenericArrayType()
    {
        var t = GetTypeResolver().Resolve("System.Nullable<[System.Int32, mscorlib]>[,]");
        ClassicAssert.AreEqual(typeof(int?[,]), t);
        t = GetTypeResolver().Resolve("System.Nullable`1[int][,]");
        ClassicAssert.AreEqual(typeof(int?[,]), t);
    }

    [Test]
    public void ResolveGenericArrayTypeWithAssemblyName()
    {
        var t = GetTypeResolver().Resolve("System.Nullable<[System.Int32, mscorlib]>[,], mscorlib");
        ClassicAssert.AreEqual(typeof(int?[,]), t);
        t = GetTypeResolver().Resolve("System.Nullable<[System.Int32, mscorlib]>[,], mscorlib");
        ClassicAssert.AreEqual(typeof(int?[,]), t);
        t = GetTypeResolver().Resolve("System.Nullable`1[[System.Int32, mscorlib]][,], mscorlib");
        ClassicAssert.AreEqual(typeof(int?[,]), t);
    }

    [Test]
    public void ResolveAmbiguousGenericTypeWithAssemblyName()
    {
        Assert.Throws<TypeLoadException>(() =>
            GetTypeResolver()
                .Resolve(
                    "Agenix.Core.Tests.TypeResolution.TestGenericObject<System.Collections.Generic.Stack<int>, System, string>"));
    }

    [Test]
    public void ResolveMalformedGenericType()
    {
        Assert.Throws<TypeLoadException>(() =>
            GetTypeResolver().Resolve("Agenix.Core.Tests.TypeResolution.TestGenericObject<int, <string>>"));
    }

    [Test]
    public void ResolveNestedGenericTypeWithAssemblyName()
    {
        var t = GetTypeResolver()
            .Resolve(
                "System.Collections.Generic.Stack<Agenix.Core.Tests.TypeResolution.TestGenericObject<int, string> >, System");
        ClassicAssert.AreEqual(typeof(Stack<TestGenericObject<int, string>>), t);
    }

    [Test]
    public void ResolveClrNotationStyleGenericTypeWithAssemblyName()
    {
        var t = GetTypeResolver()
            .Resolve(
                "System.Collections.Generic.Stack`1[ [Agenix.Core.Tests.TypeResolution.TestGenericObject`2[int, string], Agenix.Core.Tests] ], System");
        ClassicAssert.AreEqual(typeof(Stack<TestGenericObject<int, string>>), t);
    }

    [Test]
    public void ResolveNestedQuotedGenericTypeWithAssemblyName()
    {
        var t = GetTypeResolver()
            .Resolve(
                "System.Collections.Generic.Stack< [Agenix.Core.Tests.TypeResolution.TestGenericObject<int, string>, Agenix.Core.Tests] >, System");
        ClassicAssert.AreEqual(typeof(Stack<TestGenericObject<int, string>>), t);
    }
}
