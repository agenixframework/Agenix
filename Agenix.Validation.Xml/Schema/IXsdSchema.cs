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

using System.Xml;
using System.Xml.Schema;

namespace Agenix.Validation.Xml.Schema;

/// <summary>
///     Represents an interface that provides functionality for interacting with an XSD (XML Schema Definition) schema,
///     including schema retrieval, validation, and creation of validators.
/// </summary>
public interface IXsdSchema
{
    /// <summary>
    ///     Gets the target namespace of the schema
    /// </summary>
    string? TargetNamespace { get; }

    /// <summary>
    ///     Gets the underlying XmlSchema
    /// </summary>
    XmlSchema Schema { get; }

    /// <summary>
    ///     Gets the schema source document
    /// </summary>
    XmlDocument? Source { get; }

    /// <summary>
    ///     Creates a validator for this schema
    /// </summary>
    XmlSchemaValidator CreateValidator();

    /// <summary>
    ///     Validates an XML document against this schema
    /// </summary>
    List<string> Validate(XmlDocument document);
}
