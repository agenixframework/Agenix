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

namespace Agenix.Validation.Xml.Schema;

/// <summary>
///     Interface for schema mapping strategies used in schema repository.
/// </summary>
public interface IXsdSchemaMappingStrategy
{
    /// <summary>
    ///     Gets the schema for a given namespace or root element name.
    /// </summary>
    /// <param name="schemas">List of available schemas.</param>
    /// <param name="document">Document instance to validate.</param>
    /// <returns>The matching XSD schema or null if no match found.</returns>
    IXsdSchema? GetSchema(List<IXsdSchema> schemas, XmlDocument document);
}
