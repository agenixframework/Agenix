using System.Collections;
using System.Reflection;
using System.Text.Json;
using Agenix.Core.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agenix.GraphQL.Server;

/// <summary>
///     GraphQL Server implementation using Hot Chocolate framework
/// </summary>
public class GraphQLServer : AbstractServer
{
    private readonly WebApplicationBuilder _builder;
    private readonly Dictionary<string, object> _mutationResolvers = new();
    private readonly Dictionary<string, object> _queryResolvers = new();
    private readonly Dictionary<string, object> _subscriptionResolvers = new();
    private string[] _allowedOrigins = Array.Empty<string>();
    private WebApplication _app;
    private string _contextPath = "/";
    private bool _corsEnabled;
    private string _graphQLPath = "/graphql";

    // Configuration properties
    private string _host = "localhost";
    private bool _introspectionEnabled = true;
    private int _maxQueryDepth = 15;
    private int _port = 8080;
    private int _queryComplexityLimit = 1000;
    private string _schemaDefinition;

    /// <summary>
    ///     Constructor
    /// </summary>
    public GraphQLServer()
    {
        _builder = WebApplication.CreateBuilder();
        ConfigureServices();
    }

    /// <summary>
    ///     Constructor with logger
    /// </summary>
    public GraphQLServer(ILogger logger) : base(logger)
    {
        _builder = WebApplication.CreateBuilder();
        ConfigureServices();
    }

    /// <summary>
    ///     Configure services for Hot Chocolate GraphQL server
    /// </summary>
    private void ConfigureServices()
    {
        // Add Hot Chocolate GraphQL services
        var graphQLBuilder = _builder.Services
            .AddGraphQLServer()
            .AddQueryType(ConfigureQueryType)
            .AddInMemorySubscriptions();

        // Only add types if they have resolvers
        if (_mutationResolvers.Count > 0)
        {
            graphQLBuilder.AddMutationType(d => ConfigureMutationType(d));
        }

        if (_subscriptionResolvers.Count > 0)
        {
            graphQLBuilder.AddSubscriptionType(d => ConfigureSubscriptionType(d));
        }

        // Configure introspection - simply pass the boolean value
        graphQLBuilder.AllowIntrospection(_introspectionEnabled);

        // Add CORS if enabled
        if (_corsEnabled)
        {
            _builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    if (_allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(_allowedOrigins);
                    }
                    else
                    {
                        policy.AllowAnyOrigin();
                    }

                    policy.AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        // Configure Kestrel server
        _builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenAnyIP(_port);
        });
    }

    /// <summary>
    ///     Server startup implementation
    /// </summary>
    protected override void Startup()
    {
        try
        {
            // Build the application
            _app = _builder.Build();

            // Configure middleware pipeline
            if (_corsEnabled)
            {
                _app.UseCors();
            }

            // Map GraphQL endpoint
            _app.MapGraphQL(_graphQLPath);

            // Start the server
            _app.StartAsync();

            Logger.LogInformation("Hot Chocolate GraphQL Server started on {Host}:{Port}{Path}",
                _host, _port, _graphQLPath);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to start Hot Chocolate GraphQL Server");
            throw;
        }
    }

    /// <summary>
    ///     Server shutdown implementation
    /// </summary>
    protected override void Shutdown()
    {
        try
        {
            _app?.StopAsync().Wait(TimeSpan.FromSeconds(30));
            _app?.DisposeAsync();
            Logger.LogInformation("Hot Chocolate GraphQL Server stopped");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during Hot Chocolate GraphQL Server shutdown");
        }
    }

    #region Configuration Methods

    /// <summary>
    ///     Set the server host
    /// </summary>
    public GraphQLServer SetHost(string host)
    {
        _host = host ?? throw new ArgumentNullException(nameof(host));
        return this;
    }

    /// <summary>
    ///     Set the server port
    /// </summary>
    public GraphQLServer SetPort(int port)
    {
        if (port is <= 0 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "Port must be between 1 and 65535");
        }

        _port = port;
        return this;
    }

    /// <summary>
    ///     Set the GraphQL endpoint path
    /// </summary>
    public GraphQLServer SetGraphQLPath(string path)
    {
        _graphQLPath = path ?? throw new ArgumentNullException(nameof(path));
        return this;
    }

    /// <summary>
    ///     Set the context path
    /// </summary>
    public GraphQLServer SetContextPath(string contextPath)
    {
        _contextPath = contextPath ?? throw new ArgumentNullException(nameof(contextPath));
        return this;
    }

    /// <summary>
    ///     Enable or disable introspection
    /// </summary>
    public GraphQLServer SetIntrospectionEnabled(bool enabled)
    {
        _introspectionEnabled = enabled;
        return this;
    }

    /// <summary>
    ///     Set maximum query depth
    /// </summary>
    public GraphQLServer SetMaxQueryDepth(int maxDepth)
    {
        _maxQueryDepth = maxDepth;
        return this;
    }

    /// <summary>
    ///     Set query complexity limit
    /// </summary>
    public GraphQLServer SetQueryComplexityLimit(int limit)
    {
        _queryComplexityLimit = limit;
        return this;
    }

    /// <summary>
    ///     Enable CORS support
    /// </summary>
    public GraphQLServer SetCorsEnabled(bool enabled)
    {
        _corsEnabled = enabled;
        return this;
    }

    /// <summary>
    ///     Set allowed origins for CORS
    /// </summary>
    public GraphQLServer SetAllowedOrigins(string[] origins)
    {
        _allowedOrigins = origins ?? [];
        return this;
    }

    /// <summary>
    ///     Add a query resolver
    /// </summary>
    public GraphQLServer AddQueryResolver(string fieldName,
        Func<Dictionary<string, object>, object, Task<object>> resolver)
    {
        _queryResolvers[fieldName] = resolver;
        return this;
    }

    /// <summary>
    ///     Add a mutation resolver
    /// </summary>
    public GraphQLServer AddMutationResolver(string fieldName,
        Func<Dictionary<string, object>, object, Task<object>> resolver)
    {
        _mutationResolvers[fieldName] = resolver;
        return this;
    }

    /// <summary>
    ///     Add a subscription resolver
    /// </summary>
    public GraphQLServer AddSubscriptionResolver(string fieldName,
        Func<Dictionary<string, object>, object, Task<object>> resolver)
    {
        _subscriptionResolvers[fieldName] = resolver;
        return this;
    }

    #endregion

    #region Hot Chocolate Type Definitions

    /// <summary>
    ///     Configure Query type dynamically based on registered resolvers
    /// </summary>
    private void ConfigureQueryType(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Query");

        // Add dynamic query resolvers
        foreach (var resolver in _queryResolvers)
        {
            var fieldName = resolver.Key;
            var resolverFunc = resolver.Value;

            descriptor.Field(fieldName)
                .Type<AnyType>()
                .Resolve(async context =>
                {
                    try
                    {
                        // Create variables dictionary
                        var variables = new Dictionary<string, object>();

                        // Get variables from the GraphQL request if available
                        var requestVariables = context.Variables;
                        if (requestVariables != null)
                        {
                            foreach (var variable in requestVariables)
                            {
                                variables[variable.Name] = variable.Value;
                            }
                        }

                        if (resolverFunc is Func<Dictionary<string, object>, object, Task<object>> asyncResolver)
                        {
                            var result = await asyncResolver(variables, context);
                            return result; // Return the result directly without processing
                        }

                        return "Invalid resolver function";
                    }
                    catch (Exception ex)
                    {
                        return $"Error: {ex.Message}";
                    }
                });
        }
    }



    private object ProcessResolverResult(object result)
    {
        if (result == null)
        {
            return null;
        }

        // Handle different result types
        switch (result)
        {
            case string str:
                return str;

            case int num:
                return num;

            case bool boolean:
                return boolean;

            case IEnumerable enumerable when !(result is string):
                // Handle arrays/lists
                var list = new List<object>();
                foreach (var item in enumerable)
                {
                    list.Add(ProcessResolverResult(item));
                }

                return list;

            default:
                // Handle anonymous objects and complex types
                if (IsAnonymousType(result.GetType()))
                {
                    return ConvertAnonymousObjectToDictionary(result);
                }

                // For other objects, try to convert to dictionary
                return ConvertObjectToDictionary(result);
        }
    }

    private bool IsAnonymousType(Type type)
    {
        return type.Name.Contains("AnonymousType") ||
               (type.IsGenericType && type.Name.Contains("f__AnonymousType"));
    }

    private Dictionary<string, object> ConvertAnonymousObjectToDictionary(object obj)
    {
        var dictionary = new Dictionary<string, object>();
        var properties = obj.GetType().GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(obj);
            dictionary[property.Name] = ProcessResolverResult(value);
        }

        return dictionary;
    }

    private Dictionary<string, object> ConvertObjectToDictionary(object obj)
    {
        var dictionary = new Dictionary<string, object>();

        try
        {
            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.CanRead)
                {
                    var value = property.GetValue(obj);
                    dictionary[property.Name] = ProcessResolverResult(value);
                }
            }
        }
        catch
        {
            // If reflection fails, try JSON serialization as fallback
            var json = JsonSerializer.Serialize(obj);
            var jsonDoc = JsonDocument.Parse(json);
            return ConvertJsonElementToDictionary(jsonDoc.RootElement);
        }

        return dictionary;
    }

    private Dictionary<string, object> ConvertJsonElementToDictionary(JsonElement element)
    {
        var dictionary = new Dictionary<string, object>();

        foreach (var property in element.EnumerateObject())
        {
            dictionary[property.Name] = ConvertJsonElementToObject(property.Value);
        }

        return dictionary;
    }

    private object ConvertJsonElementToObject(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                return ConvertJsonElementToDictionary(element);

            case JsonValueKind.Array:
                var list = new List<object>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(ConvertJsonElementToObject(item));
                }

                return list;

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                if (element.TryGetInt32(out var intValue))
                {
                    return intValue;
                }

                if (element.TryGetInt64(out var longValue))
                {
                    return longValue;
                }

                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null;

            default:
                return element.ToString();
        }
    }


    /// <summary>
    ///     Configure Mutation type dynamically
    /// </summary>
    private void ConfigureMutationType(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Mutation");

        foreach (var resolver in _mutationResolvers)
        {
            var fieldName = resolver.Key;
            var resolverFunc = resolver.Value;

            descriptor.Field(fieldName)
                .Type<StringType>()
                .Resolve(async context =>
                {
                    var variables = context.ArgumentValue<Dictionary<string, object>>("variables") ??
                                    new Dictionary<string, object>();
                    if (resolverFunc is Func<Dictionary<string, object>, object, Task<object>> asyncResolver)
                    {
                        return await asyncResolver(variables, context);
                    }

                    return "Invalid resolver";
                });
        }
    }

    /// <summary>
    ///     Configure Subscription type dynamically
    /// </summary>
    private void ConfigureSubscriptionType(IObjectTypeDescriptor descriptor)
    {
        descriptor.Name("Subscription");

        foreach (var resolver in _subscriptionResolvers)
        {
            var fieldName = resolver.Key;
            var resolverFunc = resolver.Value;

            descriptor.Field(fieldName)
                .Type<StringType>()
                .Resolve(async context =>
                {
                    var variables = context.ArgumentValue<Dictionary<string, object>>("variables") ??
                                    new Dictionary<string, object>();
                    if (resolverFunc is Func<Dictionary<string, object>, object, Task<object>> asyncResolver)
                    {
                        return await asyncResolver(variables, context);
                    }

                    return "Invalid resolver";
                });
        }
    }

    #endregion
}
