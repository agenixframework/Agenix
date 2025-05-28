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

using Agenix.Api.Validation.Context;

namespace Agenix.Core.Validation.Json;

/// <summary>
///     Validation context holding JSON specific validation information.
/// </summary>
public class JsonMessageValidationContext : DefaultMessageValidationContext, IValidationContext
{
    // Default constructor
    public JsonMessageValidationContext() : this(new Builder())
    {
    }

    /// Represents the validation context specific to JSON messages,
    /// extending the behavior and properties from the default message validation context.
    /// /
    public JsonMessageValidationContext(Builder builder) : base(new DefaultMessageValidationContext.Builder()
        .Ignore(builder.IgnoreExpressions)
        .Schema(builder._schema)
        .SchemaRepository(builder._schemaRepository)
        .SchemaValidation(builder._schemaValidation))
    {
    }

    /// Indicates whether a validation context requires a validator for processing.
    /// This property overrides the default behavior defined in the IValidationContext interface,
    /// explicitly returning true to signify that the context requires validation.
    bool IValidationContext.RequiresValidator => true;

    public new sealed class Builder : IMessageValidationContext.Builder<JsonMessageValidationContext, Builder>
    {
        /// <summary>
        ///     Creates a new instance of JsonMessageValidationContext using the builder's current state.
        /// </summary>
        /// <returns>A JsonMessageValidationContext instance.</returns>
        public override JsonMessageValidationContext Build()
        {
            return new JsonMessageValidationContext(this);
        }

        /// <summary>
        ///     Creates a new instance of JsonMessageValidationContext.Builder.
        /// </summary>
        /// <returns>A builder for creating a JsonMessageValidationContext.</returns>
        public static Builder Json()
        {
            return new Builder();
        }

        /// <summary>
        ///     Adds a JSON path expression to the validation context builder.
        /// </summary>
        /// <returns>A new instance of JsonPathMessageValidationContext.Builder initialized for JSON path message validation.</returns>
        public JsonPathMessageValidationContext.Builder Expressions()
        {
            return new JsonPathMessageValidationContext.Builder();
        }

        /// <summary>
        ///     Adds a JSON path expression to the validation context.
        /// </summary>
        /// <param name="path">The JSON path expression.</param>
        /// <param name="expectedValue">The expected value at the given JSON path.</param>
        /// <returns>A builder for chaining method calls.</returns>
        public JsonPathMessageValidationContext.Builder Expression(string path, object expectedValue)
        {
            return new JsonPathMessageValidationContext.Builder().Expression(path, expectedValue);
        }
    }
}
