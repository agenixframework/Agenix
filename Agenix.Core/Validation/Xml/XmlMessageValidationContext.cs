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

using System.Collections.Generic;
using Agenix.Api.Validation.Context;
using Agenix.Api.Validation.Xml;

namespace Agenix.Core.Validation.Xml;

/// <summary>
///     Represents an XML validation context that encapsulates validation-related
///     details required for the validation of XML messages.
/// </summary>
public class XmlMessageValidationContext : DefaultMessageValidationContext
{
    /// Holds a dictionary of control namespaces used specifically for XML message validation.
    /// /
    private readonly Dictionary<string, string> _controlNamespaces;

    /// Represents an optional delegate serving as a parent validation context
    /// for the current XML message validation. This context is intended to be
    /// updated with the status and relevant details from the existing context.
    private readonly IMessageValidationContext _delegateContext;

    /// <summary>
    ///     Contains namespace definitions used for resolving namespaces during the validation of XML messages.
    /// </summary>
    private readonly Dictionary<string, string> _namespaces;

    /// Represents an XML validation context that encapsulates validation-related
    /// details required for the validation of XML messages.
    /// /
    public XmlMessageValidationContext() : this(new Builder())
    {
    }

    /// Represents an XML validation context that encapsulates validation-related
    /// details required for the validation of XML messages.
    /// /
    public XmlMessageValidationContext(XmlValidationContextBuilder<XmlMessageValidationContext, Builder> builder)
        : base(new DefaultMessageValidationContext.Builder()
            .SchemaValidation(builder._schemaValidation)
            .Schema(builder._schema)
            .SchemaRepository(builder._schemaRepository)
            .Ignore(builder.IgnoreExpressions))
    {
        _delegateContext = builder._delegate;
        _namespaces = new Dictionary<string, string>(builder._namespaces);
        _controlNamespaces = new Dictionary<string, string>(builder.ControlNamespaces);
    }


    /// Represents a mapping of XML namespace prefixes to their corresponding namespace URIs
    /// used during XML message validation.
    public Dictionary<string, string> Namespaces => _namespaces;

    /// Dictionary containing control namespaces used specifically for validation purposes
    public Dictionary<string, string> ControlNamespaces => _controlNamespaces;

    /// Updates the validation status, including propagating the updated status
    /// to a delegate context if one is present.
    /// <param name="status">The validation status to update.</param>
    /// /
    public override void UpdateStatus(ValidationStatus status)
    {
        base.UpdateStatus(status);

        _delegateContext?.UpdateStatus(status);
    }

    /// <summary>
    ///     Provides an implementation for creating and customizing XML validation contexts
    ///     through a fluent API. This builder simplifies the process of constructing
    ///     and adapting message validation contexts for XML schemas and XPath checks.
    /// </summary>
    public new sealed class Builder : XmlValidationContextBuilder<XmlMessageValidationContext, Builder>
    {
        /// Static entry method for fluent builder API.
        /// @return A new instance of the Builder for fluent API usage.
        /// /
        public static Builder Xml()
        {
            return new Builder();
        }

        /// <summary>
        ///     Adapts the provided message validation context builder into an XML-specific message validation context builder.
        /// </summary>
        /// <param name="messageValidationContext">
        ///     The builder of the source message validation context to be adapted.
        /// </param>
        /// <returns>
        ///     A new instance of the builder for the XML message validation context, incorporating properties from the given
        ///     context.
        /// </returns>
        public static Builder Adapt(
            IMessageValidationContext.Builder<IMessageValidationContext, DefaultMessageValidationContext.Builder>
                messageValidationContext)
        {
            return Adapt(messageValidationContext.Build());
        }

        /// Adapts an existing message validation context to an XML message validation context, preserving all relevant properties.
        /// <param name="messageValidationContext">
        ///     The source message validation context to be adapted into an XML message validation context.
        /// </param>
        /// <returns>
        ///     A new instance of the XML message validation context that incorporates all relevant values from the provided
        ///     message validation context.
        /// </returns>
        public static Builder Adapt(IMessageValidationContext messageValidationContext)
        {
            var builder = new Builder();

            builder.Ignore(messageValidationContext.IgnoreExpressions);
            builder.SchemaValidation(messageValidationContext.IsSchemaValidationEnabled);
            builder.Schema(messageValidationContext.Schema);
            builder.SchemaRepository(messageValidationContext.SchemaRepository);
            builder.Delegate(messageValidationContext);

            return builder;
        }

        /// Represents a method to create or configure an XPath message validation context
        /// through a fluent interface or builder pattern. Enables chaining and customization
        /// of validation rules associated with XPath expressions.
        public XpathMessageValidationContext.Builder Expressions()
        {
            return new XpathMessageValidationContext.Builder();
        }

        /// Represents a mathematical or logical expression used in constructing or processing
        /// computation logic, queries, or conditions.
        public XpathMessageValidationContext.Builder Expression(string path, object expectedValue)
        {
            return new XpathMessageValidationContext.Builder().Expression(path, expectedValue);
        }

        /// Converts the current XML validation context builder to an XPath message validation
        /// context builder, transferring relevant configuration and settings.
        /// <returns>The new XPath message validation context builder containing the transferred settings.</returns>
        public XpathMessageValidationContext.Builder XPath()
        {
            return new XpathMessageValidationContext.Builder()
                .NamespaceContext(_namespaces)
                .Namespaces(ControlNamespaces)
                .SchemaValidation(_schemaValidation)
                .SchemaRepository(_schemaRepository)
                .Schema(_schema)
                .Ignore(IgnoreExpressions);
        }

        public override XmlMessageValidationContext Build()
        {
            return new XmlMessageValidationContext(this);
        }
    }

    /// <summary>
    ///     Provides a base fluent builder for constructing XML validation contexts.
    ///     This builder simplifies the process of configuring namespace mappings and delegates
    ///     essential validation behaviors to a customizable context.
    /// </summary>
    public abstract class XmlValidationContextBuilder<T, S> : IMessageValidationContext.Builder<T, S>,
        IXmlNamespaceAware
        where T : XmlMessageValidationContext
        where S : XmlValidationContextBuilder<T, S>, new()

    {
        protected readonly S _self;

        protected XmlValidationContextBuilder()
        {
            _self = (S)this;
        }

        protected internal IMessageValidationContext _delegate { get; set; }

        protected internal Dictionary<string, string> _namespaces { get; set; } = new();
        protected internal Dictionary<string, string> ControlNamespaces { get; } = new();

        public void SetNamespaces(IDictionary<string, string> namespaces)
        {
            _namespaces = new Dictionary<string, string>(namespaces);
        }

        /// Represents a namespace within the context.
        /// Allows configuring prefixes and namespace URIs for XML validation.
        /// @param prefix The prefix associated with the namespace.
        /// @param namespaceUri The URI of the namespace.
        /// @return The updated XmlValidationContextBuilder instance.
        /// /
        public S Namespace(string prefix, string namespaceUri)
        {
            ControlNamespaces[prefix] = namespaceUri;
            return _self;
        }

        /// <summary>
        ///     Configures the namespace definitions for XML message validation.
        /// </summary>
        /// <param name="newNamespaces">
        ///     A dictionary containing the namespace prefixes and their respective URIs to be added or
        ///     updated.
        /// </param>
        /// <returns>The current instance of <c>XmlValidationContextBuilder</c> with the updated namespaces.</returns>
        public S Namespaces(Dictionary<string, string> newNamespaces)
        {
            foreach (var kvp in newNamespaces)
            {
                ControlNamespaces[kvp.Key] = kvp.Value;
            }

            return _self;
        }

        /// Represents a namespace within the context of XML message validation.
        /// /
        public S NamespaceContext(string prefix, string namespaceUri)
        {
            _namespaces[prefix] = namespaceUri;
            return _self;
        }

        /// Represents a namespace context for managing XML namespace mappings.
        /// Enables configuration of prefixes and corresponding namespace URIs
        /// used in XML message validation.
        /// /
        public S NamespaceContext(Dictionary<string, string> newNamespaces)
        {
            foreach (var kvp in newNamespaces)
            {
                _namespaces[kvp.Key] = kvp.Value;
            }

            return _self;
        }

        /// <summary>
        ///     Represents a delegate for managing and connecting contexts or handlers.
        /// </summary>
        /// <param name="newDelegate">The new delegate context to set for building or invoking operations.</param>
        /// <returns>The updated context builder after setting the delegate.</returns>
        protected S Delegate(IMessageValidationContext newDelegate)
        {
            _delegate = newDelegate;
            return _self;
        }
    }
}
