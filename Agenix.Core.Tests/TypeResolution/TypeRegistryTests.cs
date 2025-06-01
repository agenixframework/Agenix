#region Imports

using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.TypeResolution;

#endregion

namespace Agenix.Core.Tests.TypeResolution;

/// <summary>
///     Unit tests for the TypeRegistry class.
/// </summary>
[TestFixture]
public sealed class TypeRegistryTests
{
    [Test]
    public void ResolveTypeWithNullAliasArg()
    {
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.ResolveType(null));
    }

    [Test]
    public void ResolveTypeWithEmptyAliasArg()
    {
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.ResolveType(string.Empty));
    }

    [Test]
    public void ResolveTypeWithWhitespacedAliasArg()
    {
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.ResolveType("   "));
    }

    [Test]
    public void RegisterTypeWithNullAliasArg()
    {
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType(null, typeof(TestObject)));
    }

    [Test]
    public void RegisterTypeWithEmptyAliasArg()
    {
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType(string.Empty, typeof(TestObject)));
    }

    [Test]
    public void RegisterTypeWithWhitespacedAliasArg()
    {
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType("   ", typeof(TestObject)));
    }

    [Test]
    public void RegisterTypeWithNullTypeArg()
    {
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType("foo", (Type)null));
    }

    [Test]
    public void RegisterTypeWithNullTypeStringArg()
    {
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType("foo", (string)null));
    }

    [Test]
    public void RegisterTypeWithEmptyTypeStringArg()
    {
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType("foo", string.Empty));
    }

    [Test]
    public void RegisterTypeWithWhitespacedTypeStringArg()
    {
        Assert.Throws<ArgumentNullException>(() => TypeRegistry.RegisterType("foo", "   "));
    }

    [Test]
    public void ReturnsNullIfNoTypeAliasRegistered()
    {
        var type = TypeRegistry.ResolveType("panko");
        ClassicAssert.IsNull(type, "Must return null if no Type is registered under the supplied alias.");
    }

    [Test]
    public void RegisteringAnAliasTwiceDoesNotThrowException()
    {
        const string Alias = "foo";

        TypeRegistry.RegisterType(Alias, typeof(TestObject));
        TypeRegistry.RegisterType(Alias, GetType());

        var type = TypeRegistry.ResolveType(Alias);
        ClassicAssert.AreEqual(GetType(), type, "Overriding Type was not registered.");
    }

    [Test]
    public void ResolveIntegerByName()
    {
        ClassicAssert.AreEqual(typeof(int),
            TypeRegistry.ResolveType("int"));
    }

    [Test]
    public void ResolveChar()
    {
        ClassicAssert.AreEqual(typeof(char),
            TypeRegistry.ResolveType(TypeRegistry.CharAlias));
    }

    [Test]
    public void ResolveInteger()
    {
        ClassicAssert.AreEqual(typeof(int),
            TypeRegistry.ResolveType(TypeRegistry.Int32Alias));
    }

    [Test]
    public void ResolveDecimal()
    {
        ClassicAssert.AreEqual(typeof(decimal),
            TypeRegistry.ResolveType(TypeRegistry.DecimalAlias));
    }

    [Test]
    public void ResolveUnsignedIntegerByName()
    {
        ClassicAssert.AreEqual(typeof(uint),
            TypeRegistry.ResolveType("uint"));
    }

    [Test]
    public void ResolveUnsignedInteger()
    {
        ClassicAssert.AreEqual(typeof(uint),
            TypeRegistry.ResolveType(TypeRegistry.UInt32Alias));
    }

    [Test]
    public void ResolveFloatByName()
    {
        ClassicAssert.AreEqual(typeof(float),
            TypeRegistry.ResolveType("float"));
    }

    [Test]
    public void ResolveFloat()
    {
        ClassicAssert.AreEqual(typeof(float),
            TypeRegistry.ResolveType(TypeRegistry.FloatAlias));
    }

    [Test]
    public void ResolveDoubleByName()
    {
        ClassicAssert.AreEqual(typeof(double),
            TypeRegistry.ResolveType("double"));
    }

    [Test]
    public void ResolveDouble()
    {
        ClassicAssert.AreEqual(typeof(double),
            TypeRegistry.ResolveType(TypeRegistry.DoubleAlias));
    }

    [Test]
    public void ResolveLongByName()
    {
        ClassicAssert.AreEqual(typeof(long),
            TypeRegistry.ResolveType("long"));
    }

    [Test]
    public void ResolveLong()
    {
        ClassicAssert.AreEqual(typeof(long),
            TypeRegistry.ResolveType(TypeRegistry.Int64Alias));
    }

    [Test]
    public void ResolveUnsignedLongByName()
    {
        ClassicAssert.AreEqual(typeof(ulong),
            TypeRegistry.ResolveType("ulong"));
    }

    [Test]
    public void ResolveUnsignedLong()
    {
        ClassicAssert.AreEqual(typeof(ulong),
            TypeRegistry.ResolveType(TypeRegistry.UInt64Alias));
    }

    [Test]
    public void ResolveShortByName()
    {
        ClassicAssert.AreEqual(typeof(short),
            TypeRegistry.ResolveType("short"));
    }

    [Test]
    public void ResolveShort()
    {
        ClassicAssert.AreEqual(typeof(short),
            TypeRegistry.ResolveType(TypeRegistry.Int16Alias));
    }

    [Test]
    public void ResolveUnsignedShortByName()
    {
        ClassicAssert.AreEqual(typeof(ushort),
            TypeRegistry.ResolveType("ushort"));
    }

    [Test]
    public void ResolveUnsignedShort()
    {
        ClassicAssert.AreEqual(typeof(ushort),
            TypeRegistry.ResolveType(TypeRegistry.UInt16Alias));
    }

    [Test]
    public void ResolveDate()
    {
        ClassicAssert.AreEqual(typeof(DateTime),
            TypeRegistry.ResolveType(TypeRegistry.DateAlias));
    }

    [Test]
    public void ResolveBool()
    {
        ClassicAssert.AreEqual(typeof(bool),
            TypeRegistry.ResolveType(TypeRegistry.BoolAlias));
    }

    [Test]
    public void ResolveIntegerByVBName()
    {
        ClassicAssert.AreEqual(typeof(int),
            TypeRegistry.ResolveType("Integer"));
    }

    [Test]
    public void ResolveVBInteger()
    {
        ClassicAssert.AreEqual(typeof(int),
            TypeRegistry.ResolveType(TypeRegistry.Int32AliasVB));
    }

    [Test]
    public void ResolveVBDecimal()
    {
        ClassicAssert.AreEqual(typeof(decimal),
            TypeRegistry.ResolveType(TypeRegistry.DecimalAliasVB));
    }

    [Test]
    public void ResolveSingleByName()
    {
        ClassicAssert.AreEqual(typeof(float),
            TypeRegistry.ResolveType("Single"));
    }

    [Test]
    public void ResolveSingle()
    {
        ClassicAssert.AreEqual(typeof(float),
            TypeRegistry.ResolveType(TypeRegistry.SingleAlias));
    }

    [Test]
    public void ResolveVBDouble()
    {
        ClassicAssert.AreEqual(typeof(double),
            TypeRegistry.ResolveType(TypeRegistry.DoubleAliasVB));
    }

    [Test]
    public void ResolveVBLong()
    {
        ClassicAssert.AreEqual(typeof(long),
            TypeRegistry.ResolveType(TypeRegistry.Int64AliasVB));
    }

    [Test]
    public void ResolveVBShort()
    {
        ClassicAssert.AreEqual(typeof(short),
            TypeRegistry.ResolveType(TypeRegistry.Int16AliasVB));
    }

    [Test]
    public void ResolveVBDate()
    {
        ClassicAssert.AreEqual(typeof(DateTime),
            TypeRegistry.ResolveType(TypeRegistry.DateAliasVB));
    }

    [Test]
    public void ResolveVBBool()
    {
        ClassicAssert.AreEqual(typeof(bool),
            TypeRegistry.ResolveType(TypeRegistry.BoolAliasVB));
    }

    [Test]
    public void ResolveString()
    {
        ClassicAssert.AreEqual(typeof(string),
            TypeRegistry.ResolveType(TypeRegistry.StringAlias));
    }

    [Test]
    public void ResolveVBString()
    {
        ClassicAssert.AreEqual(typeof(string),
            TypeRegistry.ResolveType(TypeRegistry.StringAliasVB));
    }

    [Test]
    public void ResolveStringArray()
    {
        ClassicAssert.AreEqual(typeof(string[]),
            TypeRegistry.ResolveType(TypeRegistry.StringArrayAlias));
    }

    [Test]
    public void ResolveVBStringArray()
    {
        ClassicAssert.AreEqual(typeof(string[]),
            TypeRegistry.ResolveType(TypeRegistry.StringArrayAliasVB));
    }

    [Test]
    public void ResolveObject()
    {
        ClassicAssert.AreEqual(typeof(object),
            TypeRegistry.ResolveType(TypeRegistry.ObjectAlias));
    }

    [Test]
    public void ResolveVBObject()
    {
        ClassicAssert.AreEqual(typeof(object),
            TypeRegistry.ResolveType(TypeRegistry.ObjectAliasVB));
    }

    [Test]
    public void ResolveObjectArray()
    {
        ClassicAssert.AreEqual(typeof(object[]),
            TypeRegistry.ResolveType(TypeRegistry.ObjectArrayAlias));
    }

    [Test]
    public void ResolveVBObjectArray()
    {
        ClassicAssert.AreEqual(typeof(object[]),
            TypeRegistry.ResolveType(TypeRegistry.ObjectArrayAliasVB));
    }

    [Test]
    public void ResolveCharArray()
    {
        ClassicAssert.AreEqual(typeof(char[]),
            TypeRegistry.ResolveType(TypeRegistry.CharArrayAlias));
    }

    [Test]
    public void ResolveVBCharArray()
    {
        ClassicAssert.AreEqual(typeof(char[]),
            TypeRegistry.ResolveType(TypeRegistry.CharArrayAliasVB));
    }

    [Test]
    public void ResolveInt32Array()
    {
        ClassicAssert.AreEqual(typeof(int[]),
            TypeRegistry.ResolveType(TypeRegistry.Int32ArrayAlias));
    }

    [Test]
    public void ResolveVBInt32Array()
    {
        ClassicAssert.AreEqual(typeof(int[]),
            TypeRegistry.ResolveType(TypeRegistry.Int32ArrayAliasVB));
    }

    [Test]
    public void ResolveInt16Array()
    {
        ClassicAssert.AreEqual(typeof(short[]),
            TypeRegistry.ResolveType(TypeRegistry.Int16ArrayAlias));
    }

    [Test]
    public void ResolveVBInt16Array()
    {
        ClassicAssert.AreEqual(typeof(short[]),
            TypeRegistry.ResolveType(TypeRegistry.Int16ArrayAliasVB));
    }

    [Test]
    public void ResolveInt64Array()
    {
        ClassicAssert.AreEqual(typeof(long[]),
            TypeRegistry.ResolveType(TypeRegistry.Int64ArrayAlias));
    }

    [Test]
    public void ResolveVBInt64Array()
    {
        ClassicAssert.AreEqual(typeof(long[]),
            TypeRegistry.ResolveType(TypeRegistry.Int64ArrayAliasVB));
    }

    [Test]
    public void ResolveUInt16Array()
    {
        ClassicAssert.AreEqual(typeof(ushort[]),
            TypeRegistry.ResolveType(TypeRegistry.UInt16ArrayAlias));
    }

    [Test]
    public void ResolveUInt32Array()
    {
        ClassicAssert.AreEqual(typeof(uint[]),
            TypeRegistry.ResolveType(TypeRegistry.UInt32ArrayAlias));
    }

    [Test]
    public void ResolveUInt64Array()
    {
        ClassicAssert.AreEqual(typeof(ulong[]),
            TypeRegistry.ResolveType(TypeRegistry.UInt64ArrayAlias));
    }

    [Test]
    public void ResolveBoolArray()
    {
        ClassicAssert.AreEqual(typeof(bool[]),
            TypeRegistry.ResolveType(TypeRegistry.BoolArrayAlias));
    }

    [Test]
    public void ResolveVBBoolArray()
    {
        ClassicAssert.AreEqual(typeof(bool[]),
            TypeRegistry.ResolveType(TypeRegistry.BoolArrayAliasVB));
    }

    [Test]
    public void ResolveDateArray()
    {
        ClassicAssert.AreEqual(typeof(DateTime[]),
            TypeRegistry.ResolveType(TypeRegistry.DateTimeArrayAlias));
    }

    [Test]
    public void ResolveVBDateArray()
    {
        ClassicAssert.AreEqual(typeof(DateTime[]),
            TypeRegistry.ResolveType(TypeRegistry.DateTimeArrayAliasVB));
    }

    [Test]
    public void ResolveFloatArray()
    {
        ClassicAssert.AreEqual(typeof(float[]),
            TypeRegistry.ResolveType(TypeRegistry.FloatArrayAlias));
    }

    [Test]
    public void ResolveVBSingleArray()
    {
        ClassicAssert.AreEqual(typeof(float[]),
            TypeRegistry.ResolveType(TypeRegistry.SingleArrayAliasVB));
    }

    [Test]
    public void ResolveDoubleArray()
    {
        ClassicAssert.AreEqual(typeof(double[]),
            TypeRegistry.ResolveType(TypeRegistry.DoubleArrayAlias));
    }

    [Test]
    public void ResolveVBDoubleArray()
    {
        ClassicAssert.AreEqual(typeof(double[]),
            TypeRegistry.ResolveType(TypeRegistry.DoubleArrayAliasVB));
    }

    [Test]
    public void ResolveNullableChar()
    {
        ClassicAssert.AreEqual(typeof(char?),
            TypeRegistry.ResolveType(TypeRegistry.NullableCharAlias));
        ClassicAssert.AreEqual(typeof(char?),
            TypeRegistry.ResolveType(TypeRegistry.NullableCharAlias));
    }

    [Test]
    public void ResolveNullableInteger()
    {
        ClassicAssert.AreEqual(typeof(int?),
            TypeRegistry.ResolveType(TypeRegistry.NullableInt32Alias));
        ClassicAssert.AreEqual(typeof(int?),
            TypeRegistry.ResolveType(TypeRegistry.NullableInt32Alias));
    }

    [Test]
    public void ResolveNullableDecimal()
    {
        ClassicAssert.AreEqual(typeof(decimal?),
            TypeRegistry.ResolveType(TypeRegistry.NullableDecimalAlias));
        ClassicAssert.AreEqual(typeof(decimal?),
            TypeRegistry.ResolveType(TypeRegistry.NullableDecimalAlias));
    }

    [Test]
    public void ResolveNullableUnsignedInteger()
    {
        ClassicAssert.AreEqual(typeof(uint?),
            TypeRegistry.ResolveType(TypeRegistry.NullableUInt32Alias));
        ClassicAssert.AreEqual(typeof(uint?),
            TypeRegistry.ResolveType(TypeRegistry.NullableUInt32Alias));
    }

    [Test]
    public void ResolveNullableFloat()
    {
        ClassicAssert.AreEqual(typeof(float?),
            TypeRegistry.ResolveType(TypeRegistry.NullableFloatAlias));
        ClassicAssert.AreEqual(typeof(float?),
            TypeRegistry.ResolveType(TypeRegistry.NullableFloatAlias));
    }

    [Test]
    public void ResolveNullableDouble()
    {
        ClassicAssert.AreEqual(typeof(double?),
            TypeRegistry.ResolveType(TypeRegistry.NullableDoubleAlias));
        ClassicAssert.AreEqual(typeof(double?),
            TypeRegistry.ResolveType(TypeRegistry.NullableDoubleAlias));
    }

    [Test]
    public void ResolveNullableLong()
    {
        ClassicAssert.AreEqual(typeof(long?),
            TypeRegistry.ResolveType(TypeRegistry.NullableInt64Alias));
        ClassicAssert.AreEqual(typeof(long?),
            TypeRegistry.ResolveType(TypeRegistry.NullableInt64Alias));
    }

    [Test]
    public void ResolveNullableUnsignedLong()
    {
        ClassicAssert.AreEqual(typeof(ulong?),
            TypeRegistry.ResolveType(TypeRegistry.NullableUInt64Alias));
        ClassicAssert.AreEqual(typeof(ulong?),
            TypeRegistry.ResolveType(TypeRegistry.NullableUInt64Alias));
    }

    [Test]
    public void ResolveNullableShort()
    {
        ClassicAssert.AreEqual(typeof(short?),
            TypeRegistry.ResolveType(TypeRegistry.NullableInt16Alias));
        ClassicAssert.AreEqual(typeof(short?),
            TypeRegistry.ResolveType(TypeRegistry.NullableInt16Alias));
    }

    [Test]
    public void ResolveNullableUnsignedShort()
    {
        ClassicAssert.AreEqual(typeof(ushort?),
            TypeRegistry.ResolveType(TypeRegistry.NullableUInt16Alias));
        ClassicAssert.AreEqual(typeof(ushort?),
            TypeRegistry.ResolveType(TypeRegistry.NullableUInt16Alias));
    }

    [Test]
    public void ResolveNullableBool()
    {
        ClassicAssert.AreEqual(typeof(bool?),
            TypeRegistry.ResolveType(TypeRegistry.NullableBoolAlias));
        ClassicAssert.AreEqual(typeof(bool?),
            TypeRegistry.ResolveType(TypeRegistry.NullableBoolAlias));
    }

    [Test]
    public void ResolveNullableCharArray()
    {
        ClassicAssert.AreEqual(typeof(char?[]),
            TypeRegistry.ResolveType(TypeRegistry.NullableCharArrayAlias));
    }

    [Test]
    public void ResolveNullableInt32Array()
    {
        ClassicAssert.AreEqual(typeof(int?[]),
            TypeRegistry.ResolveType(TypeRegistry.NullableInt32ArrayAlias));
    }

    [Test]
    public void ResolveNullableDecimalArray()
    {
        ClassicAssert.AreEqual(typeof(decimal?[]),
            TypeRegistry.ResolveType(TypeRegistry.NullableDecimalArrayAlias));
    }

    [Test]
    public void ResolveNullableInt16Array()
    {
        ClassicAssert.AreEqual(typeof(short?[]),
            TypeRegistry.ResolveType(TypeRegistry.NullableInt16ArrayAlias));
    }

    [Test]
    public void ResolveNullableInt64Array()
    {
        ClassicAssert.AreEqual(typeof(long?[]),
            TypeRegistry.ResolveType(TypeRegistry.NullableInt64ArrayAlias));
    }

    [Test]
    public void ResolveNullableUInt16Array()
    {
        ClassicAssert.AreEqual(typeof(ushort?[]),
            TypeRegistry.ResolveType(TypeRegistry.NullableUInt16ArrayAlias));
    }

    [Test]
    public void ResolveNullableUInt32Array()
    {
        ClassicAssert.AreEqual(typeof(uint?[]),
            TypeRegistry.ResolveType(TypeRegistry.NullableUInt32ArrayAlias));
    }

    [Test]
    public void ResolveNullableUInt64Array()
    {
        ClassicAssert.AreEqual(typeof(ulong?[]),
            TypeRegistry.ResolveType(TypeRegistry.NullableUInt64ArrayAlias));
    }

    [Test]
    public void ResolveNullableBoolArray()
    {
        ClassicAssert.AreEqual(typeof(bool?[]),
            TypeRegistry.ResolveType(TypeRegistry.NullableBoolArrayAlias));
    }

    [Test]
    public void ResolveNullableFloatArray()
    {
        ClassicAssert.AreEqual(typeof(float?[]),
            TypeRegistry.ResolveType(TypeRegistry.NullableFloatArrayAlias));
    }

    [Test]
    public void ResolveNullableDoubleArray()
    {
        ClassicAssert.AreEqual(typeof(double?[]),
            TypeRegistry.ResolveType(TypeRegistry.NullableDoubleArrayAlias));
    }
}

internal class Foo
{
    public Type Bar { get; set; }
}
