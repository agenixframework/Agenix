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

using Agenix.Api.Context;
using Agenix.Api.Xml.Namespace;
using Agenix.Validation.Xml.Schema;

namespace Agenix.Validation.Xml;

/// <summary>
///     Helper class for XML validation operations.
///     Provides utility methods for resolving schema repositories and namespace context builders.
/// </summary>
public static class XmlValidationHelper
{
    private static readonly NamespaceContextBuilder DefaultNamespaceContextBuilder = new();

    /// <summary>
    ///     Consult reference resolver in given test context and resolve all available beans of type XsdSchemaRepository.
    /// </summary>
    /// <param name="context">The test context</param>
    /// <returns>List of XsdSchemaRepository instances</returns>
    public static List<XsdSchemaRepository> GetSchemaRepositories(TestContext context)
    {
        if (context.ReferenceResolver != null &&
            context.ReferenceResolver.IsResolvable(typeof(XsdSchemaRepository)))
        {
            return context.ReferenceResolver.ResolveAll<XsdSchemaRepository>().Values.ToList();
        }

        return [];
    }

    /// <summary>
    ///     Consult reference resolver in given test context and resolve bean of type NamespaceContextBuilder.
    /// </summary>
    /// <param name="context">The current test context</param>
    /// <returns>Resolved namespace context builder instance</returns>
    public static NamespaceContextBuilder GetNamespaceContextBuilder(TestContext context)
    {
        if (context.ReferenceResolver != null &&
            context.ReferenceResolver.IsResolvable(typeof(NamespaceContextBuilder)))
        {
            return context.ReferenceResolver.Resolve<NamespaceContextBuilder>();
        }

        return DefaultNamespaceContextBuilder;
    }
}
