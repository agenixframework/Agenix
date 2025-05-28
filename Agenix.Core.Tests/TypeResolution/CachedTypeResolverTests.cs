using System;
using FakeItEasy;
using NUnit.Framework;
using Spring.Core.TypeResolution;

namespace Agenix.Core.Tests.TypeResolution;

/// <summary>
///     Unit tests for the CachedTypeResolver class.
/// </summary>
[TestFixture]
public sealed class CachedTypeResolverTests
{
    [SetUp]
    public void SetUp()
    {
    }

    [Test]
    public void ResolveWithNullTypeName()
    {
        var mockResolver = A.Fake<ITypeResolver>();

        var resolver = new CachedTypeResolver(mockResolver);
        Assert.Throws<TypeLoadException>(() => resolver.Resolve(null));
    }

    [Test]
    public void InstantiateWithNullTypeResolver()
    {
        Assert.Throws<ArgumentNullException>(() => new CachedTypeResolver(null));
    }
}
