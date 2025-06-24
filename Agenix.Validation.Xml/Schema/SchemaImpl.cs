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
///     Represents an implementation of the <see cref="IWsdlSchema" /> interface, providing functionality
///     to work with XML schemas as both <see cref="XmlSchema" /> and <see cref="XmlElement" /> objects.
/// </summary>
public class SchemaImpl : IWsdlSchema
{
    public SchemaImpl(XmlSchema xmlSchema)
    {
        Schema = xmlSchema;
        // Convert XmlSchema to XmlElement if needed
        Element = ConvertSchemaToElement(xmlSchema);
    }

    public SchemaImpl(XmlElement element)
    {
        Element = element;
        // Convert XmlElement to XmlSchema if needed
        Schema = ConvertElementToSchema(element);
    }

    public XmlSchema Schema { get; }

    public XmlElement Element { get; }

    public string TargetNamespace => Schema?.TargetNamespace ?? Element?.GetAttribute("targetNamespace");

    /// <summary>
    ///     Converts an <see cref="XmlSchema" /> to an <see cref="XmlElement" />.
    /// </summary>
    /// <param name="schema">The <see cref="XmlSchema" /> to be converted to an <see cref="XmlElement" />.</param>
    /// <returns>An <see cref="XmlElement" /> instance representing the provided <see cref="XmlSchema" />.</returns>
    private XmlElement ConvertSchemaToElement(XmlSchema schema)
    {
        var doc = new XmlDocument();
        using var writer = doc.CreateNavigator().AppendChild();
        schema.Write(writer);
        return doc.DocumentElement;
    }

    /// <summary>
    ///     Converts an <see cref="XmlElement" /> to an <see cref="XmlSchema" />.
    /// </summary>
    /// <param name="element">The <see cref="XmlElement" /> to be converted to an <see cref="XmlSchema" />.</param>
    /// <returns>An <see cref="XmlSchema" /> instance representing the provided <see cref="XmlElement" />.</returns>
    private XmlSchema ConvertElementToSchema(XmlElement element)
    {
        using var reader = new XmlNodeReader(element);
        return XmlSchema.Read(reader, null);
    }
}
