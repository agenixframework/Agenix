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

namespace Agenix.Validation.Xml;

/// <summary>
///     Factory for creating configured XML readers
/// </summary>
public class XmlReaderFactory(XmlConfigurer? configurer = null)
{
    public XmlReader CreateReader(Stream input)
    {
        return configurer?.CreateXmlReader(input) ?? XmlReader.Create(input);
    }

    public XmlReader CreateReader(string xmlContent)
    {
        return configurer?.CreateXmlReader(xmlContent) ?? XmlReader.Create(new StringReader(xmlContent));
    }

    public XmlReader CreateReader(TextReader input)
    {
        return configurer?.CreateXmlReader(input) ?? XmlReader.Create(input);
    }
}
