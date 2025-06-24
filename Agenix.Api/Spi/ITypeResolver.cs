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

namespace Agenix.Api.Spi;

/// <summary>
///     Resolves types by searching for assembly resource mapping files in order to resolve class references at runtime.
/// </summary>
public interface ITypeResolver
{
    /**
     * Property name that holds the type information to resolve
     */
    const string DEFAULT_TYPE_PROPERTY = "type";

    /**
     * Property name to mark that multiple types will be present all types are loaded
     */
    const string TYPE_PROPERTY_WILDCARD = "*";

    /// <summary>
    ///     Resolve resource path property file with given name and load given property.
    /// </summary>
    /// <param name="resourcePath"></param>
    /// <param name="property"></param>
    /// <returns></returns>
    string ResolveProperty(string resourcePath, string property);

    /// <summary>
    ///     Load given property from given resource path property file and create new instance of given type. The type
    ///     information
    ///     is read by the given property in the resource file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resourcePath"></param>
    /// <param name="property"></param>
    /// <param name="initargs"></param>
    /// <returns></returns>
    T Resolve<T>(string resourcePath, string property, params object[] initargs);

    /// <summary>
    ///     Load all resources in given resource path and create new instance of given type. The type information is read by
    ///     the given property in the resource file.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resourcePath"></param>
    /// <param name="property"></param>
    /// <param name="keyProperty"></param>
    /// <returns></returns>
    IDictionary<string, T> ResolveAll<T>(string resourcePath, string property, string keyProperty);
}
