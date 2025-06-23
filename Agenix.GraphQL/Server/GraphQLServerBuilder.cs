namespace Agenix.GraphQL.Server
{
    /// <summary>
    /// Builder for GraphQL servers that provides fluent API for configuration.
    /// </summary>
    public class GraphQLServerBuilder : AbstractGraphQLServerBuilder<GraphQLServer, GraphQLServerBuilder>
    {
        /// <summary>
        /// Default constructor that creates a new GraphQLServer instance.
        /// </summary>
        public GraphQLServerBuilder() : this(new GraphQLServer())
        {
        }

        /// <summary>
        /// Constructor with an existing GraphQLServer instance.
        /// </summary>
        /// <param name="server1Old">The GraphQL server instance to build upon</param>
        protected GraphQLServerBuilder(GraphQLServer server1Old) : base(server1Old)
        {
        }

        /// <summary>
        /// Static factory method to create a new GraphQLServerBuilder.
        /// </summary>
        /// <returns>A new GraphQLServerBuilder instance</returns>
        public static GraphQLServerBuilder Create()
        {
            return new GraphQLServerBuilder();
        }

        /// <summary>
        /// Static factory method to create a new GraphQLServerBuilder with an existing server.
        /// </summary>
        /// <param name="server1Old">The GraphQL server instance</param>
        /// <returns>A new GraphQLServerBuilder instance</returns>
        public static GraphQLServerBuilder Create(GraphQLServer server1Old)
        {
            return new GraphQLServerBuilder(server1Old);
        }
    }
}
