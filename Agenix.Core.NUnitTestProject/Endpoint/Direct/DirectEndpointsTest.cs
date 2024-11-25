using Agenix.Core.Endpoint;
using Agenix.Core.Endpoint.Direct;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Endpoint.Direct;

public class DirectEndpointsTest
{
    [Test]
    public void ShouldLookupEndpoints()
    {
        var endpointBuilders = IEndpointBuilder<DirectEndpoint>.Lookup();
        ClassicAssert.IsTrue(endpointBuilders.ContainsKey("direct.sync"));
        ClassicAssert.IsTrue(endpointBuilders.ContainsKey("direct.async"));
    }

    [Test]
    public void ShouldLookupEndpoint()
    {
        ClassicAssert.IsTrue(IEndpointBuilder<DirectEndpoint>.Lookup("direct.sync").IsPresent);
        ClassicAssert.AreEqual(IEndpointBuilder<DirectEndpoint>.Lookup("direct.sync").Value.GetType(),
            typeof(DirectSyncEndpointBuilder));
        ClassicAssert.IsTrue(IEndpointBuilder<DirectEndpoint>.Lookup("direct.async").IsPresent);
        ClassicAssert.AreEqual(IEndpointBuilder<DirectEndpoint>.Lookup("direct.async").Value.GetType(),
            typeof(DirectEndpointBuilder));
    }
}