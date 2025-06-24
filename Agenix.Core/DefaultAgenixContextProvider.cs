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

using Agenix.Api;

namespace Agenix.Core;

/// Default implementation of AgenixContext provider. It facilitates the generation of
/// AgenixContext instances according to a specified creation strategy. The provider
/// can generate a new context instance upon each request or return a consistent,
/// singleton instance, as defined by the AgenixInstanceStrategy.
/// /
public class DefaultAgenixContextProvider(AgenixInstanceStrategy strategy) : IAgenixContextProvider
{
    public static string Spring = "spring";

    private static AgenixContext _context;

    public DefaultAgenixContextProvider() : this(AgenixInstanceStrategy.SINGLETON)
    {
    }

    /// <summary>
    ///     Creates an instance of the <see cref="AgenixContext" /> based on the current strategy.
    ///     If the strategy is set to NEW, or the context has not been initialized, a new instance
    ///     of <see cref="AgenixContext" /> is created. Otherwise, the existing context is returned.
    /// </summary>
    /// <returns>
    ///     An instance of the <see cref="AgenixContext" />. Depending on the strategy,
    ///     it may either be a new instance or a reused existing instance.
    /// </returns>
    public AgenixContext Create()
    {
        if (strategy.Equals(AgenixInstanceStrategy.NEW) || _context == null)
        {
            _context = AgenixContext.Create();
        }

        return _context;
    }
}
