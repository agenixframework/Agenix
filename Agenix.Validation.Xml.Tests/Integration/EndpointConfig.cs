using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Api.Xml.Namespace;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message;

namespace Agenix.Validation.Xml.Tests.Integration;

/// Represents the configuration for endpoints used in the application.
/// The class provides methods to configure and bind various objects necessary for
/// routing messages and managing namespaces as part of the messaging system.
public class EndpointConfig
{
    [BindToRegistry(Name = "hello.queue")]
    public IMessageQueue HelloQueue()
    {
        return new DefaultMessageQueue("helloQueue");
    }

    [BindToRegistry(Name = "hello.endpoint")]
    public DirectEndpoint HelloEndpoint()
    {
        return new DirectEndpointBuilder()
            .Queue(HelloQueue())
            .Build();
    }

    [BindToRegistry(Name = "namespaceContextBuilder")]
    public NamespaceContextBuilder NamespaceContextBuilder()
    {
        var builder = new NamespaceContextBuilder();
        builder.NamespaceMappings.Add("def", "http://agenix.org/schemas/samples/HelloService.xsd");
        return builder;
    }
}
