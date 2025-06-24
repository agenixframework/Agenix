#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
