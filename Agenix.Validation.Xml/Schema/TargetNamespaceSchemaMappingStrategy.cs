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

using System.Xml.Schema;
using Agenix.Api.Util;

namespace Agenix.Validation.Xml.Schema;

/// <summary>
///     Mapping strategy checks on target namespaces in schemas to find matching schema
///     instance.
/// </summary>
public class TargetNamespaceSchemaMappingStrategy : AbstractSchemaMappingStrategy
{
    /// <summary>
    ///     Retrieves an XML schema from a collection of schemas that matches the specified namespace and element name.
    /// </summary>
    /// <param name="schemas">The collection of XML schemas to search through.</param>
    /// <param name="namespaceName">The target namespace to match against the schemas' target namespaces.</param>
    /// <param name="elementName">The name of the root element to match (not used in the logic of this implementation).</param>
    /// <returns>
    ///     The first matching <see cref="XmlSchema" /> whose target namespace matches the specified
    ///     <paramref name="namespaceName" />, or null if no match is found.
    /// </returns>
    public override IXsdSchema? GetSchema(List<IXsdSchema> schemas, string namespaceName, string elementName)
    {
        return schemas.FirstOrDefault(schema =>
            StringUtils.HasText(schema?.TargetNamespace ?? string.Empty) &&
            string.Equals(schema?.TargetNamespace, namespaceName, StringComparison.Ordinal));
    }
}
