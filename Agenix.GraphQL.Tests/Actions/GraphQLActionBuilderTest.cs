using Agenix.Api;
using Agenix.Api.Spi;
using Agenix.GraphQL.Actions;
using ITestAction = Agenix.Api.ITestAction;


namespace Agenix.GraphQL.Tests.Actions;

public class GraphQLActionBuilderTest
{
    private GraphQLActionBuilder? _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new GraphQLActionBuilder();
    }

    [Test]
    public void ShouldLookupTestActionBuilder()
    {
        var endpointBuilders = ITestActionBuilder<ITestAction>.Lookup();
        Assert.That(endpointBuilders.ContainsKey("graphql"), Is.True);

        var graphqlBuilder = ITestActionBuilder<ITestAction>.Lookup("graphql");
        Assert.That(graphqlBuilder.IsPresent, Is.True);
        Assert.That(graphqlBuilder.Value.GetType(), Is.EqualTo(typeof(GraphQLActionBuilder)));
    }

    [Test]
    public void IsReferenceResolverAwareTestActionBuilder()
    {
        Assert.That(_fixture, Is.InstanceOf<AbstractReferenceResolverAwareTestActionBuilder<ITestAction>>(),
            "Is instanceof AbstractReferenceResolverAwareTestActionBuilder");
    }
}
