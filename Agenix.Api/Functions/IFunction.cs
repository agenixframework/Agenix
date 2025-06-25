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

using System.Collections.Concurrent;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Functions;

/// <summary>
///     General function interface.
/// </summary>
public interface IFunction
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IFunction));

    /// <summary>
    ///     Lazy-initialized collection of function implementations with thread-safe loading.
    /// </summary>
    private static readonly Lazy<IDictionary<string, IFunction>> Functions =
        new(LoadFunctions);

    /// <summary>
    ///     Represents the resource path for function extensions.
    /// </summary>
    static string ResourcePath => "Extension/agenix/function";

    /// <summary>
    ///     Loads and initializes all function implementations from the resource path.
    /// </summary>
    /// <returns>A dictionary containing all loaded function implementations.</returns>
    private static IDictionary<string, IFunction> LoadFunctions()
    {
        var functions = new ConcurrentDictionary<string, IFunction>();
        var resolvedFunctions = new ResourcePathTypeResolver().ResolveAll<dynamic>(ResourcePath);

        foreach (var kvp in resolvedFunctions)
        {
            functions[kvp.Key] = kvp.Value;
        }

        if (Log.IsEnabled(LogLevel.Debug))
        {
            foreach (var kvp in functions)
            {
                Log.LogDebug("Found function '{KvpKey}' as {Type}", kvp.Key, kvp.Value.GetType());
            }
        }

        return functions;
    }

    /// <summary>
    ///     Retrieves a collection of registered function implementations, resolving them if not already loaded.
    /// </summary>
    /// <returns>A dictionary where the keys are function names and the values are the corresponding function implementations.</returns>
    static IDictionary<string, IFunction> Lookup()
    {
        return Functions.Value;
    }

    /// <summary>
    ///     Method called on execution.
    /// </summary>
    /// <param name="parameterList">The list of function arguments.</param>
    /// <param name="testContext">The test context</param>
    /// <returns>The function result as string.</returns>
    string Execute(List<string> parameterList, TestContext testContext);
}
