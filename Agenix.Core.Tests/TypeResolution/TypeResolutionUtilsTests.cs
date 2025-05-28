#region Imports

using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.TypeResolution;

#endregion

namespace Agenix.Core.Tests.TypeResolution;

/// <summary>
///     Unit tests for the TypeResolutionUtils class.
/// </summary>
[TestFixture]
public sealed class TypeResolutionUtilsTests
{
    [Test]
    public void ResolveFromAssemblyQualifiedName()
    {
        var testObjectType =
            TypeResolutionUtils.ResolveType(
                "Agenix.Core.Tests.TypeResolution.TestObject, Agenix.Core.Tests");
        ClassicAssert.IsNotNull(testObjectType);
        ClassicAssert.IsTrue(testObjectType.Equals(typeof(TestObject)));
    }

    [Test]
    public void ResolveFromBadAssemblyQualifiedName()
    {
        Assert.Throws<TypeLoadException>(() =>
            TypeResolutionUtils.ResolveType(
                "Agenix.Core.Tests.TypeResolution.TestObject, Agenix.Core.FooTests"));
    }

    [Test]
    public void ResolveFromShortName()
    {
        var testObjectType = TypeResolutionUtils.ResolveType("Agenix.Core.Tests.TypeResolution.TestObject");
        ClassicAssert.IsNotNull(testObjectType);
        ClassicAssert.IsTrue(testObjectType.Equals(typeof(TestObject)));
    }

    [Test]
    public void ResolveFromBadShortName()
    {
        Assert.Throws<TypeLoadException>(() =>
            TypeResolutionUtils.ResolveType("Agenix.Core.Tests.TypeResolution.FooBarTestObject"));
    }

    [Test]
    public void ResolveInterfaceArrayFromStringArray()
    {
        Type[] expected = [typeof(IFoo)];
        string[] input = [typeof(IFoo).AssemblyQualifiedName];
        var actual = TypeResolutionUtils.ResolveInterfaceArray(input);
        ClassicAssert.IsNotNull(actual);
        ClassicAssert.AreEqual(expected.Length, actual.Count);
        ClassicAssert.AreEqual(expected[0], actual[0]);
    }

    [Test]
    public void ResolveInterfaceArrayFromStringArrayWithNonInterfaceTypes()
    {
        string[] input = [GetType().AssemblyQualifiedName];
        Assert.Throws<ArgumentException>(() => TypeResolutionUtils.ResolveInterfaceArray(input));
    }

    [Test]
    public void MethodMatch()
    {
        var absquatulateMethod = typeof(TestObject).GetMethod("Absquatulate");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("*", absquatulateMethod), "Should match '*'");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("*tulate", absquatulateMethod), "Should match '*tulate'");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("Absqua*", absquatulateMethod), "Should match 'Absqua*'");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("*quatul*", absquatulateMethod),
            "Should match '*quatul*'");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("Absquatulate", absquatulateMethod),
            "Should match 'Absquatulate'");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("Absquatulate()", absquatulateMethod),
            "Should match 'Absquatulate()'");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("Absquatulate()", absquatulateMethod),
            "Should match 'Absquatulate()'");
        ClassicAssert.IsFalse(TypeResolutionUtils.MethodMatch("Absquatulate(string)", absquatulateMethod),
            "Should not match 'Absquatulate(string)'");

        var addPeriodicElementMethod = typeof(TestObject).GetMethod("AddPeriodicElement");
        ClassicAssert.IsTrue(TypeResolutionUtils.MethodMatch("AddPeriodicElement", addPeriodicElementMethod),
            "Should match 'AddPeriodicElement'");
        ClassicAssert.IsFalse(TypeResolutionUtils.MethodMatch("AddPeriodicElement()", addPeriodicElementMethod),
            "Should not match 'AddPeriodicElement()'");
        ClassicAssert.IsFalse(TypeResolutionUtils.MethodMatch("AddPeriodicElement(string)", addPeriodicElementMethod),
            "Should not match 'AddPeriodicElement(string)'");
        ClassicAssert.IsTrue(
            TypeResolutionUtils.MethodMatch("AddPeriodicElement(string, string)", addPeriodicElementMethod),
            "Should match 'AddPeriodicElement(string, string)'");
    }

    internal interface IFoo
    {
        bool Spanglish(string foo, object[] args);
    }
}
