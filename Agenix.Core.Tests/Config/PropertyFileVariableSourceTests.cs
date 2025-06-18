#region

using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Spring.Core.IO;
using Spring.Objects.Factory.Config;

#endregion

namespace Agenix.Core.Tests.Config;

/// <summary>
///     Unit tests for the PropertyFileVariableSource class.
/// </summary>
[TestFixture]
public sealed class PropertyFileVariableSourceTests
{
    [Test]
    public void TestVariablesResolutionWithSingleLocation()
    {
        var vs = new PropertyFileVariableSource
        {
            Location = new AssemblyResource(
                $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/one.properties")
        };

        // existing vars
        ClassicAssert.AreEqual("Aleks Seovic", vs.ResolveVariable("name"));
        ClassicAssert.AreEqual("32", vs.ResolveVariable("age"));

        // non-existent variable
        ClassicAssert.IsNull(vs.ResolveVariable("dummy"));
    }

    [Test]
    public void TestMissingResourceLocation()
    {
        var vs = new PropertyFileVariableSource
        {
            IgnoreMissingResources = true,
            Locations =
            [
                new AssemblyResource(
                    $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/non-existent.properties"),
                new AssemblyResource(
                    $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/one.properties")
            ]
        };

        // existing vars
        ClassicAssert.AreEqual("Aleks Seovic", vs.ResolveVariable("name"));
        ClassicAssert.AreEqual("32", vs.ResolveVariable("age"));
    }


    [Test]
    public void TestVariablesResolutionWithTwoLocations()
    {
        var vs = new PropertyFileVariableSource
        {
            Locations =
            [
                new AssemblyResource(
                    $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/one.properties"),
                new AssemblyResource(
                    $"assembly://{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Name}.ResourcesTest/two.properties")
            ]
        };

        // existing vars
        ClassicAssert.AreEqual("Aleksandar Seovic",
            vs.ResolveVariable("name")); // should be overriden by the second file
        ClassicAssert.AreEqual("32", vs.ResolveVariable("age"));
        ClassicAssert.AreEqual("Marija,Ana,Nadja", vs.ResolveVariable("family"));

        // non-existant variable
        ClassicAssert.IsNull(vs.ResolveVariable("dummy"));
    }
}
