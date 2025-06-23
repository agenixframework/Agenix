using Agenix.Api;
using Agenix.Api.Spi;
using Agenix.Core.Util;
using Agenix.GraphQL.Client;

namespace Agenix.GraphQL.Actions;

/// <summary>
/// Provides functionality to build and configure GraphQL-related test actions fluently.
/// </summary>
/// <remarks>
/// GraphQLActionBuilder is a specialized builder that extends from
/// AbstractReferenceResolverAwareTestActionBuilder, allowing seamless construction
/// and execution of GraphQL test actions with support for reference resolver injection.
/// </remarks>
public class GraphQLActionBuilder : AbstractReferenceResolverAwareTestActionBuilder<ITestAction>
{
    /// Static entrance method for the GraphQL fluent action builder.
    /// <return>Instance of the GraphQL action builder.</return>
    public static GraphQLActionBuilder GraphQL()
    {
        return new GraphQLActionBuilder();
    }

    /// Initiates a GraphQL client action using the specified GraphQL client instance.
    /// Dynamically constructs a fluent API for configuring GraphQL client actions,
    /// including setting reference resolvers and other parameters.
    /// <param name="graphQlClient">The GraphQL client instance to use for this action.</param>
    /// <returns>A builder instance to configure and execute the GraphQL client action.</returns>
    public GraphQLClientActionBuilder Client(GraphQLClient graphQlClient)
    {
        var clientActionBuilder = new GraphQLClientActionBuilder(graphQlClient)
            .WithReferenceResolver(referenceResolver);
        _delegate = clientActionBuilder;
        return clientActionBuilder;
    }

    /// Creates and initializes a new GraphQL client action builder for configuring
    /// GraphQL client operations with the specified GraphQL client identifier.
    /// <param name="graphQlClient">The identifier or URI of the GraphQL client used to construct the action builder.</param>
    /// <returns>An instance of GraphQLClientActionBuilder to configure and build GraphQL client actions.</returns>
    public GraphQLClientActionBuilder Client(string graphQlClient)
    {
        var clientActionBuilder = new GraphQLClientActionBuilder(graphQlClient)
            .WithReferenceResolver(referenceResolver);
        _delegate = clientActionBuilder;
        return clientActionBuilder;
    }

    /// Sets the bean reference resolver to be used for resolving references during the building process.
    /// <param name="referenceResolver">The reference resolver instance to set.</param>
    /// <return>This instance of GraphQLActionBuilder for method chaining.</return>
    public GraphQLActionBuilder WithReferenceResolver(IReferenceResolver referenceResolver)
    {
        this.referenceResolver = referenceResolver;
        return this;
    }

    /// Builds and returns an ITestAction instance.
    /// <return>The built ITestAction instance.</return>
    public override ITestAction Build()
    {
        ObjectHelper.AssertNotNull(_delegate, "Missing delegate action to build");
        return _delegate.Build();
    }
}
