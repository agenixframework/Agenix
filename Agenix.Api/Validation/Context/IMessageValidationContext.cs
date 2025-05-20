namespace Agenix.Api.Validation.Context
{
    /// Represents a message validation context, combining validation functionality
    /// with schema validation capabilities and supporting the management of
    /// ignored message elements.
    public interface IMessageValidationContext : IValidationContext, ISchemaValidationContext
    {
        /// Retrieves the ignored message elements.
        /// @return a set of ignored expressions
        /// /
        ISet<string> IgnoreExpressions { get; }

        /// Provides a builder for constructing instances of message validation contexts.
        /// This abstract class is designed to be extended by concrete implementations
        /// and provides methods for configuring schema validation, schema repositories,
        /// and ignored message elements.
        abstract class Builder<T, S> : IValidationContext.IBuilder<T, Builder<T, S>>, ISchemaValidationContext.IBuilder<Builder<T, S>>
            where T : IMessageValidationContext 
            where S : Builder<T, S>

        {
            protected readonly S Self;

            public readonly HashSet<string> IgnoreExpressions = [];
            public bool _schemaValidation = true;
            public string _schemaRepository;
            public string _schema;

            protected Builder()
            {
                Self = (S)this;
            }

            /// Enables or disables schema validation for the message.
            /// <param name="enabled">A boolean value indicating whether schema validation should be enabled (true) or disabled (false).</param>
            /// <return>Returns the current builder instance to allow method chaining.</return>
            public Builder<T, S> SchemaValidation(bool enabled)
            {
                _schemaValidation = enabled;
                return Self;
            }

            /// Sets an explicit schema instance name to use for schema validation.
            /// <param name="schemaName">The name of the schema to be used for validation.</param>
            /// <return>Returns the current builder instance to allow method chaining.</return>
            public Builder<T, S>  Schema(string schemaName)
            {
                _schema = schemaName;
                return Self;
            }

            /// Sets the schema repository to be used for schema validation.
            /// <param name="schemaRepository">The name or path of the schema repository to use.</param>
            /// <return>Returns the current builder instance to allow method chaining.</return>
            public Builder<T, S>  SchemaRepository(string schemaRepository)
            {
                _schemaRepository = schemaRepository;
                return Self;
            }

            /// Adds an ignored path expression for a specific message element.
            /// <param name="path">The path expression of the element to be ignored.</param>
            /// <return>Returns the current builder instance to allow method chaining.</return>
            public S Ignore(string path)
            {
                IgnoreExpressions.Add(path);
                return Self;
            }

            /// Adds a set of ignored path expressions for message elements.
            /// <param name="paths">A set of path expressions representing the elements to be ignored in the validation context.</param>
            /// <return>Returns the current builder instance to allow method chaining.</return>
            public S Ignore(ISet<string> paths)
            {
                IgnoreExpressions.UnionWith(paths);
                return Self;
            }
            
            public abstract T Build();
        }
    }
}