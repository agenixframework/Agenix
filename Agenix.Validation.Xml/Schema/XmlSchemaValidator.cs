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
///     The <c>XmlSchemaValidator</c> class is used to validate XML documents against a set of XML Schema Definitions
///     (XSDs).
///     It ensures that the XML conforms to the rules and structure defined by the associated schema set.
/// </summary>
public class XmlSchemaValidator(XmlSchemaSet schemaSet)
{
    private readonly XmlSchemaSet _schemaSet = schemaSet ?? throw new ArgumentNullException(nameof(schemaSet));

    /// <summary>
    ///     Validates an XML document against the defined XML Schema Definitions (XSDs) in the schema set.
    /// </summary>
    /// <param name="reader">
    ///     The <c>XmlReader</c> instance for the XML document that needs to be validated.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the provided <c>XmlReader</c> instance is null.
    /// </exception>
    /// <exception cref="XmlException">
    ///     Thrown if the XML document does not conform to the schemas or a validation error occurs.
    /// </exception>
    public void Validate(XmlReader reader)
    {
        var settings = new XmlReaderSettings
        {
            Schemas = _schemaSet,
            ValidationType = ValidationType.Schema,
            ValidationFlags = XmlSchemaValidationFlags.ProcessInlineSchema |
                              XmlSchemaValidationFlags.ProcessSchemaLocation |
                              XmlSchemaValidationFlags.ReportValidationWarnings
        };

        settings.ValidationEventHandler += (sender, e) =>
        {
            if (e.Severity == XmlSeverityType.Error)
            {
                throw new XmlException($"Validation error: {e.Message}");
            }
        };

        using var validatingReader = XmlReader.Create(reader, settings);
        while (validatingReader.Read()) { }
    }
}
