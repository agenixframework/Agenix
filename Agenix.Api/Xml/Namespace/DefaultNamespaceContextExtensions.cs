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

namespace Agenix.Api.Xml.Namespace;

/// <summary>
///     Extension methods for DefaultNamespaceContext integration
/// </summary>
public static class DefaultNamespaceContextExtensions
{
    /// <summary>
    ///     Converts DefaultNamespaceContext to XmlNamespaceManager for XPath operations
    /// </summary>
    /// <param name="context">The namespace context to convert</param>
    /// <returns>XmlNamespaceManager with all namespace mappings</returns>
    public static XmlNamespaceManager ToXmlNamespaceManager(this DefaultNamespaceContext context)
    {
        var nameTable = new NameTable();
        var manager = new XmlNamespaceManager(nameTable);

        foreach (var ns in context.GetNamespaces())
        {
            manager.AddNamespace(ns.Key, ns.Value);
        }

        return manager;
    }

    /// <summary>
    ///     Creates DefaultNamespaceContext from XmlNamespaceManager
    /// </summary>
    /// <param name="manager">The XmlNamespaceManager to convert</param>
    /// <returns>DefaultNamespaceContext with all namespace mappings</returns>
    public static DefaultNamespaceContext ToDefaultNamespaceContext(this XmlNamespaceManager manager)
    {
        var context = new DefaultNamespaceContext();

        foreach (string prefix in manager)
        {
            var uri = manager.LookupNamespace(prefix);
            if (!string.IsNullOrEmpty(uri))
            {
                context.AddNamespace(prefix, uri);
            }
        }

        return context;
    }
}
