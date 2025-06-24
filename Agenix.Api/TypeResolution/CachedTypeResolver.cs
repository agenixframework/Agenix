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

#region Imports

using System.Collections;
using System.Collections.Specialized;
using Agenix.Api.Util;

#endregion

namespace Agenix.Api.TypeResolution;

/// <summary>
///     Resolves (instantiates) a <see cref="System.Type" /> by it's (possibly
///     assembly qualified) name, and caches the <see cref="System.Type" />
///     instance against the type name.
/// </summary>
public class CachedTypeResolver : ITypeResolver
{
    /// <summary>
    ///     The cache, mapping type names (<see cref="System.String" /> instances) against
    ///     <see cref="System.Type" /> instances.
    /// </summary>
    private readonly IDictionary typeCache = new HybridDictionary();

    private readonly ITypeResolver typeResolver;

    /// <summary>
    ///     Creates a new instance of the <see cref="CachedTypeResolver" /> class.
    /// </summary>
    /// <param name="typeResolver">
    ///     The <see cref="Type" /> that this instance will delegate
    ///     actual <see cref="System" /> resolution to if a <see cref="System" />
    ///     cannot be found in this instance's <see cref="System" /> cache.
    /// </param>
    /// <exception cref="System">
    ///     If the supplied <paramref name="typeResolver" /> is <see langword="null" />.
    /// </exception>
    public CachedTypeResolver(ITypeResolver typeResolver)
    {
        AssertUtils.ArgumentNotNull(typeResolver, "typeResolver");
        this.typeResolver = typeResolver;
    }

    /// <summary>
    ///     Resolves the supplied <paramref name="typeName" /> to a
    ///     <see cref="System.Type" />
    ///     instance.
    /// </summary>
    /// <param name="typeName">
    ///     The (possibly partially assembly qualified) name of a
    ///     <see cref="System.Type" />.
    /// </param>
    /// <returns>
    ///     A resolved <see cref="System.Type" /> instance.
    /// </returns>
    /// <exception cref="System.TypeLoadException">
    ///     If the supplied <paramref name="typeName" /> could not be resolved
    ///     to a <see cref="System.Type" />.
    /// </exception>
    public Type Resolve(string typeName)
    {
        if (StringUtils.IsNullOrEmpty(typeName))
        {
            throw BuildTypeLoadException(typeName);
        }

        Type type;
        try
        {
            lock (typeCache.SyncRoot)
            {
                type = typeCache[typeName] as Type;
                if (type == null)
                {
                    type = typeResolver.Resolve(typeName);
                    typeCache[typeName] = type;
                }
            }
        }
        catch (Exception ex)
        {
            if (ex is TypeLoadException)
            {
                throw;
            }

            throw BuildTypeLoadException(typeName, ex);
        }

        return type;
    }

    private static TypeLoadException BuildTypeLoadException(string typeName)
    {
        return new TypeLoadException("Could not load type from string value '" + typeName + "'.");
    }

    private static TypeLoadException BuildTypeLoadException(string typeName, Exception ex)
    {
        return new TypeLoadException("Could not load type from string value '" + typeName + "'.", ex);
    }
}
