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
    abstract class Builder<T, S> : IBuilder<T, Builder<T, S>>, IBuilder<Builder<T, S>>
        where T : IMessageValidationContext
        where S : Builder<T, S>

    {
        public readonly HashSet<string> IgnoreExpressions = [];
        protected readonly S Self;
        public string _schema;
        public string _schemaRepository;
        public bool _schemaValidation = true;

        protected Builder()
        {
            Self = (S)this;
        }

        /// Enables or disables schema validation for the message.
        /// <param name="enabled">
        ///     A boolean value indicating whether schema validation should be enabled (true) or disabled
        ///     (false).
        /// </param>
        /// <return>Returns the current builder instance to allow method chaining.</return>
        public Builder<T, S> SchemaValidation(bool enabled)
        {
            _schemaValidation = enabled;
            return Self;
        }

        /// Sets an explicit schema instance name to use for schema validation.
        /// <param name="schemaName">The name of the schema to be used for validation.</param>
        /// <return>Returns the current builder instance to allow method chaining.</return>
        public Builder<T, S> Schema(string schemaName)
        {
            _schema = schemaName;
            return Self;
        }

        /// Sets the schema repository to be used for schema validation.
        /// <param name="schemaRepository">The name or path of the schema repository to use.</param>
        /// <return>Returns the current builder instance to allow method chaining.</return>
        public Builder<T, S> SchemaRepository(string schemaRepository)
        {
            _schemaRepository = schemaRepository;
            return Self;
        }

        public abstract T Build();

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
    }
}
