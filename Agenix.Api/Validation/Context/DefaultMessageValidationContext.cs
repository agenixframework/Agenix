#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

namespace Agenix.Api.Validation.Context;

/// Represents a default implementation for the message validation context.
/// This context provides facilities to manage schema validation settings
/// and a collection of expressions to be ignored during validation.
/// Includes a dedicated Builder class for constructing instances with
/// specific configurations, allowing for fluent-style setup of schema
/// validation and ignore expressions.
/// Implements IMessageValidationContext interface, inheriting its schema
/// validation and overall validation context capabilities.
public class DefaultMessageValidationContext(
    IMessageValidationContext.Builder<IMessageValidationContext, DefaultMessageValidationContext.Builder> builder)
    : DefaultValidationContext, IMessageValidationContext
{
    /// <summary>
    ///     A collection of XPath or JSON expressions used to specify message elements to be excluded from validation.
    /// </summary>
    private readonly HashSet<string> _ignoreExpressions = builder.IgnoreExpressions;

    /// <summary>
    ///     Explicit schema instance to use for this validation
    /// </summary>
    private readonly string _schema = builder._schema;

    /// <summary>
    ///     Explicit schema repository to use for this validation
    /// </summary>
    private readonly string _schemaRepository = builder._schemaRepository;

    /// <summary>
    ///     Should a message be validated with its schema definition?
    /// </summary>
    private readonly bool _schemaValidationEnabled = builder._schemaValidation;

    public DefaultMessageValidationContext() : this(new Builder())
    {
    }

    /// Gets the set of XPath expressions used to identify message elements
    /// that should be ignored during validation.
    public HashSet<string> IgnoreExpressions => _ignoreExpressions;

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
}
