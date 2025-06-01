#region Imports

using System;
using System.IO;
using Agenix.Api.IO;
using NUnit.Framework;
using NUnit.Framework.Legacy;

#endregion

namespace Agenix.Core.Tests.IO;

/// <summary>
///     Unit tests for the ConfigurableResourceLoader class.
/// </summary>
[TestFixture]
public sealed class ResourceHandlerRegistryTests
{
    [Test]
    public void WithNullProtocolName()
    {
        Assert.Throws<ArgumentNullException>(() => ResourceHandlerRegistry.RegisterResourceHandler(null, GetType()));
    }

    [Test]
    public void WithNullIResourceHandlerType()
    {
        Assert.Throws<ArgumentNullException>(() => ResourceHandlerRegistry.RegisterResourceHandler("beep", (Type)null));
    }

    [Test]
    public void WithWhitespacedProtocolName()
    {
        Assert.Throws<ArgumentNullException>(() => ResourceHandlerRegistry.RegisterResourceHandler("\t   ", GetType()));
    }

    [Test]
    public void WithNonIResourceHandlerType()
    {
        Assert.Throws<ArgumentException>(() => ResourceHandlerRegistry.RegisterResourceHandler("beep", GetType()));
    }

    [Test]
    public void WithIResourceHandlerTypeWithNoValidCtor()
    {
        Assert.Throws<ArgumentException>(() =>
            ResourceHandlerRegistry.RegisterResourceHandler("beep", typeof(IncompatibleResource)));
    }

    [Test]
    public void AddProtocolMappingSilentlyOverwritesExistingProtocol()
    {
        ResourceHandlerRegistry.RegisterResourceHandler("beep", typeof(FileSystemResource));
        // overwrite, must not complain...
        ResourceHandlerRegistry.RegisterResourceHandler("beep", typeof(AssemblyResource));
        var res = new ConfigurableResourceLoader().GetResource(
            "beep://Agenix.Core.Tests/Agenix.Core.Tests/TestResource.txt");
        ClassicAssert.IsNotNull(res, "Resource must not be null");
        ClassicAssert.AreEqual(typeof(AssemblyResource), res.GetType(),
            "The original IResource Type associated with the 'beep' protocol " +
            "must have been overwritten; expecting an AssemblyResource 'cos " +
            "we registered it last under the 'beep' protocol.");
    }

    /// <summary>
    ///     Deso <b>not</b> expose a constructor that takes a single string argument.
    /// </summary>
    private sealed class IncompatibleResource : IResource
    {
        public bool IsOpen => throw new NotImplementedException();

        public Uri Uri => throw new NotImplementedException();

        public FileInfo File => throw new NotImplementedException();

        public string Description => throw new NotImplementedException();

        public bool Exists => throw new NotImplementedException();

        public IResource CreateRelative(string relativePath)
        {
            throw new NotImplementedException();
        }

        public Stream InputStream => throw new NotImplementedException();
    }
}
