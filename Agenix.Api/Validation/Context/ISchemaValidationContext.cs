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

/// <summary>
///     Interface for schema validation context.
/// </summary>
public interface ISchemaValidationContext
{
    /// <summary>
    ///     Gets a value indicating whether schema validation is enabled.
    /// </summary>
    bool IsSchemaValidationEnabled { get; }

    /// <summary>
    ///     Gets the schema repository.
    /// </summary>
    string SchemaRepository { get; }

    /// <summary>
    ///     Gets the schema.
    /// </summary>
    string Schema { get; }


    /// <summary>
    ///     Fluent builder interface.
    /// </summary>
    /// <typeparam name="TB">The type of the builder.</typeparam>
    public interface IBuilder<TB> : IBuilder
    {
        /// <summary>
        ///     Sets schema validation enabled or disabled for this message.
        /// </summary>
        /// <param name="enabled">True to enable schema validation; otherwise, false.</param>
        /// <returns>The builder.</returns>
        TB SchemaValidation(bool enabled);

        /// <summary>
        ///     Sets an explicit schema instance name to use for schema validation.
        /// </summary>
        /// <param name="schemaName">The name of the schema.</param>
        /// <returns>The builder.</returns>
        TB Schema(string schemaName);

        /// <summary>
        ///     Sets an explicit XSD schema repository instance to use for validation.
        /// </summary>
        /// <param name="schemaRepository">The schema repository.</param>
        /// <returns>The builder.</returns>
        TB SchemaRepository(string schemaRepository);
    }
}
