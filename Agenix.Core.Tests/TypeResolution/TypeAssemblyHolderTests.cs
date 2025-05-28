#region Imports

using Agenix.Core.Tests.TypeResolution;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.TypeResolution;

#endregion

namespace Agenix.Core.Tests.TypeResolution;

/// <summary>
///     Test class that contains unit tests for verifying the behavior of the <c>TypeAssemblyHolder</c> class.
/// </summary>
/// <remarks>
///     The tests validate the proper handling of assembly-qualified and non-assembly-qualified type names,
///     as well as handling of generic types.
/// </remarks>
[TestFixture]
public class TypeAssemblyHolderTests
{
    [Test]
    public void CanTakeQualifiedType()
    {
        var testType = typeof(TestObject);
        var tah = new TypeAssemblyHolder(testType.AssemblyQualifiedName);
        ClassicAssert.IsTrue(tah.IsAssemblyQualified);
        ClassicAssert.AreEqual(testType.FullName, tah.TypeName);
        ClassicAssert.AreEqual(testType.Assembly.FullName, tah.AssemblyName);
    }

    [Test]
    public void CanTakeUnqualifiedType()
    {
        var testType = typeof(TestObject);
        var tah = new TypeAssemblyHolder(testType.FullName);
        ClassicAssert.IsFalse(tah.IsAssemblyQualified);
        ClassicAssert.AreEqual(testType.FullName, tah.TypeName);
        ClassicAssert.AreEqual(null, tah.AssemblyName);
    }

    [Test]
    public void CanTakeUnqualifiedGenericType()
    {
        var testType = typeof(TestGenericObject<int, string>);
        var tah = new TypeAssemblyHolder(testType.FullName);
        ClassicAssert.IsFalse(tah.IsAssemblyQualified);
        ClassicAssert.AreEqual(testType.FullName, tah.TypeName);
        ClassicAssert.AreEqual(null, tah.AssemblyName);
    }

    [Test]
    public void CanTakeQualifiedGenericType()
    {
        var testType = typeof(TestGenericObject<int, string>);
        var tah = new TypeAssemblyHolder(testType.AssemblyQualifiedName);
        ClassicAssert.IsTrue(tah.IsAssemblyQualified);
        ClassicAssert.AreEqual(testType.FullName, tah.TypeName);
        ClassicAssert.AreEqual(testType.Assembly.FullName, tah.AssemblyName);
    }
}
