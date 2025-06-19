using Agenix.Core.Server;

namespace Agenix.GraphQL.Server
{
    /// <summary>
    /// Abstract builder for GraphQL servers that provides fluent API for configuration.
    /// </summary>
    /// <typeparam name="TServer">The type of GraphQL server being built</typeparam>
    /// <typeparam name="TBuilder">The type of the builder itself for fluent chaining</typeparam>
    public abstract class AbstractGraphQLServerBuilder<TServer, TBuilder> : AbstractServerBuilder<TServer, TBuilder>
        where TServer : GraphQLServer
        where TBuilder : AbstractGraphQLServerBuilder<TServer, TBuilder>
    {
        /// <summary>
        /// Endpoint target
        /// </summary>
        private readonly TServer _endpoint;

        /// <summary>
        /// Self reference for fluent chaining
        /// </summary>
        private readonly TBuilder _self;

        /// <summary>
        /// Default GraphQL path
        /// </summary>
        private string _graphQLPath = "/graphql";

        /// <summary>
        /// Default context path
        /// </summary>
        private string _contextPath = "/";

        /// <summary>
        /// Query resolvers
        /// </summary>
        private readonly Dictionary<string, Func<Dictionary<string, object>, object, Task<object>>> _queryResolvers = new();

        /// <summary>
        /// Mutation resolvers
        /// </summary>
        private readonly Dictionary<string, Func<Dictionary<string, object>, object, Task<object>>> _mutationResolvers = new();

        /// <summary>
        /// Subscription resolvers
        /// </summary>
        private readonly Dictionary<string, Func<Dictionary<string, object>, object, Task<object>>> _subscriptionResolvers = new();

        /// <summary>
        /// GraphQL schema definition
        /// </summary>
        private string _schemaDefinition;

        /// <summary>
        /// Enable GraphQL introspection
        /// </summary>
        private bool _introspectionEnabled = true;

        /// <summary>
        /// Maximum query depth
        /// </summary>
        private int _maxQueryDepth = 15;

        /// <summary>
        /// Query complexity limit
        /// </summary>
        private int _queryComplexityLimit = 1000;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="server1Old">The GraphQL server instance</param>
        protected AbstractGraphQLServerBuilder(TServer server1Old)
        {
            _endpoint = server1Old ?? throw new ArgumentNullException(nameof(server1Old));
            _self = (TBuilder)this;
        }

        /// <summary>
        /// Gets the endpoint
        /// </summary>
        /// <returns>The GraphQL server endpoint</returns>
        protected override TServer GetEndpoint()
        {
            return _endpoint;
        }

        /// <summary>
        /// Sets the port property
        /// </summary>
        /// <param name="port">The port number</param>
        /// <returns>The builder instance for fluent chaining</returns>
        public TBuilder Port(int port)
        {
            _endpoint.SetPort(port);
            return _self;
        }

        /// <summary>
        /// Sets the host property
        /// </summary>
        /// <param name="host">The host address</param>
        /// <returns>The builder instance for fluent chaining</returns>
        public TBuilder Host(string host)
        {
            _endpoint.SetHost(host);
            return _self;
        }

        /// <summary>
        /// Sets the GraphQL endpoint path
        /// </summary>
        /// <param name="path">The GraphQL endpoint path</param>
        /// <returns>The builder instance for fluent chaining</returns>
        public TBuilder GraphQLPath(string path)
        {
            _graphQLPath = path ?? throw new ArgumentNullException(nameof(path));
            _endpoint.SetGraphQLPath(_graphQLPath);
            return _self;
        }

        /// <summary>
        /// Sets the context path
        /// </summary>
        /// <param name="contextPath">The context path</param>
        /// <returns>The builder instance for fluent chaining</returns>
        public TBuilder ContextPath(string contextPath)
        {
            _contextPath = contextPath ?? throw new ArgumentNullException(nameof(contextPath));
            _endpoint.SetContextPath(_contextPath);
            return _self;
        }

        /// <summary>
        /// Adds a query resolver
        /// </summary>
        /// <param name="fieldName">The GraphQL field name</param>
        /// <param name="resolver">The resolver function</param>
        /// <returns>The builder instance for fluent chaining</returns>
        public TBuilder AddQueryResolver(string fieldName, Func<object, object, Task<object>> resolver)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            _queryResolvers[fieldName] = resolver;
            _endpoint.AddQueryResolver(fieldName, resolver);
            return _self;
        }

        /// <summary>
        /// Adds a mutation resolver
        /// </summary>
        /// <param name="fieldName">The GraphQL field name</param>
        /// <param name="resolver">The resolver function</param>
        /// <returns>The builder instance for fluent chaining</returns>
        public TBuilder AddMutationResolver(string fieldName, Func<object, object, Task<object>> resolver)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            _mutationResolvers[fieldName] = resolver;
            _endpoint.AddMutationResolver(fieldName, resolver);
            return _self;
        }

        /// <summary>
        /// Adds a subscription resolver
        /// </summary>
        /// <param name="fieldName">The GraphQL field name</param>
        /// <param name="resolver">The resolver function</param>
        /// <returns>The builder instance for fluent chaining</returns>
        public TBuilder AddSubscriptionResolver(string fieldName, Func<object, object, Task<object>> resolver)
        {
            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentException("Field name cannot be null or empty", nameof(fieldName));
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            _subscriptionResolvers[fieldName] = resolver;
            _endpoint.AddSubscriptionResolver(fieldName, resolver);
            return _self;
        }

        /// <summary>
        /// Sets the default timeout for GraphQL operations
        /// </summary>
        /// <param name="timeout">The timeout in milliseconds</param>
        /// <returns>The builder instance for fluent chaining</returns>
        public override TBuilder Timeout(long timeout)
        {
            _endpoint.DefaultTimeout = timeout;
            return _self;
        }

    }
}
