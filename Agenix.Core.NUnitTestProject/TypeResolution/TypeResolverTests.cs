#region Imports

using System;
using System.Data;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.TypeResolution;

#endregion

namespace Agenix.Core.NUnitTestProject.TypeResolution;

/// <summary>
///     Unit tests for the TypeResolver class.
/// </summary>
[TestFixture]
public class TypeResolverTests
{
    protected virtual ITypeResolver GetTypeResolver()
    {
        return new TypeResolver();
    }

    [Test]
    public void ResolveLocalAssemblyType()
    {
        var t = GetTypeResolver().Resolve("Agenix.Core.NUnitTestProject.TypeResolution.TestObject");
        ClassicAssert.AreEqual(typeof(TestObject), t);
    }

    [Test]
    public void ResolveWithPartialAssemblyName()
    {
        var t = GetTypeResolver().Resolve("System.Data.IDbConnection, System.Data");
        ClassicAssert.AreEqual(typeof(IDbConnection), t);
    }

    /// <summary>
    ///     Tests that the resolve method throws the correct exception
    ///     when supplied a load of old rubbish as a type name.
    /// </summary>
    [Test]
    public void ResolveWithNonExistentTypeName()
    {
        Assert.Throws<TypeLoadException>(() => GetTypeResolver().Resolve("RaskolnikovsDilemma, System.StPetersburg"));
    }

    [Test]
    public void ResolveBadArgs()
    {
        Assert.Throws<TypeLoadException>(() => GetTypeResolver().Resolve(null));
    }

    [Test]
    public void ResolveLocalAssemblyTypeWithFullAssemblyQualifiedName()
    {
        var t = GetTypeResolver().Resolve(typeof(TestObject).AssemblyQualifiedName);
        ClassicAssert.AreEqual(typeof(TestObject), t);
    }

    [Test]
    public void LoadTypeFromSystemAssemblySpecifyingOnlyTheAssemblyDisplayName()
    {
        var stringType = typeof(string).FullName + ", System";
        Assert.Throws<TypeLoadException>(() => GetTypeResolver().Resolve(stringType));
    }

    [Test]
    public void LoadTypeFromSystemAssemblySpecifyingTheFullAssemblyName()
    {
        var stringType = typeof(string).AssemblyQualifiedName;
        var t = GetTypeResolver().Resolve(stringType);
        ClassicAssert.AreEqual(typeof(string), t);
    }
}