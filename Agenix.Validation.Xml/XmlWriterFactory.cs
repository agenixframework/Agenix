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

using System.Text;
using System.Xml;

namespace Agenix.Validation.Xml;

/// <summary>
///     Provides functionality to create instances of <see cref="XmlWriter" /> objects configured with optional settings.
/// </summary>
/// <remarks>
///     The <see cref="XmlWriterFactory" /> class allows creating <see cref="XmlWriter" /> objects for various output
///     targets,
///     such as <see cref="StringBuilder" />, <see cref="Stream" />, and <see cref="TextWriter" />. An optional
///     <see cref="XmlConfigurer" /> can be supplied to customize the configuration of the created <see cref="XmlWriter" />
///     objects.
/// </remarks>
public class XmlWriterFactory(XmlConfigurer? configurer = null)
{
    public XmlWriter CreateWriter(Stream output)
    {
        return configurer?.CreateXmlWriter(output) ?? XmlWriter.Create(output);
    }

    public XmlWriter CreateWriter(TextWriter output)
    {
        return configurer?.CreateXmlWriter(output) ?? XmlWriter.Create(output);
    }

    public XmlWriter CreateWriter(StringBuilder output)
    {
        return configurer?.CreateXmlWriter(output) ?? XmlWriter.Create(output);
    }
}
