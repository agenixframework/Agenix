namespace Agenix.Api.Validation.Context;

/// Represents a default implementation for the message validation context.
/// This context provides facilities to manage schema validation settings
/// and a collection of expressions to be ignored during validation.
/// Includes a dedicated Builder class for constructing instances with
/// specific configurations, allowing for fluent-style setup of schema
/// validation and ignore expressions.
/// Implements IMessageValidationContext interface, inheriting its schema
/// validation and overall validation context capabilities.
public class DefaultMessageValidationContext(IMessageValidationContext.Builder<IMessageValidationContext, DefaultMessageValidationContext.Builder> builder)
    : DefaultValidationContext, IMessageValidationContext
{
    /// <summary>
    /// A collection of XPath or JSON expressions used to specify message elements to be excluded from validation.
    /// </summary>
    private readonly HashSet<string> _ignoreExpressions = builder.IgnoreExpressions;

    /// <summary>
    /// Should a message be validated with its schema definition?
    /// </summary>
    private readonly bool _schemaValidationEnabled = builder._schemaValidation;
    
    /// <summary>
    /// Explicit schema instance to use for this validation
    /// </summary>
    private readonly string _schema = builder._schema;
    
    /// <summary>
    /// Explicit schema repository to use for this validation
    /// </summary>
    private readonly string _schemaRepository = builder._schemaRepository;

    public DefaultMessageValidationContext() : this(new Builder())
    {
    }

    /// Fluent builder for constructing instances of DefaultMessageValidationContext,
    /// allowing the configuration of schema validation properties and ignored expressions.
    /// /
    public sealed class Builder : IMessageValidationContext.Builder<IMessageValidationContext, Builder> 
    {
        public override IMessageValidationContext Build()
        {
            return new DefaultMessageValidationContext(this);
        }
    }

    /// Gets the set of XPath expressions used to identify message elements
    /// that should be ignored during validation.
    public ISet<string> IgnoreExpressions => _ignoreExpressions;

    /// Determines whether schema validation is enabled for the context.
    public bool IsSchemaValidationEnabled => _schemaValidationEnabled;

    /// Represents the location or source of the schema used for validation
    /// within the message validation context. This property provides the
    /// necessary reference to identify or retrieve the schema required
    /// for validating message structures.
    public string SchemaRepository => _schemaRepository;

    /// Represents the schema associated with the validation context, typically used
    /// for validation against a predefined structure or set of rules.
    public string Schema => _schema;
}