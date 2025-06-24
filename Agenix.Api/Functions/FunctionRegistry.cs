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

using Agenix.Api.Exceptions;

namespace Agenix.Api.Functions;

/// <summary>
///     Function registry holding all available function libraries.
/// </summary>
public class FunctionRegistry
{
    /// <summary>
    ///     The list of libraries providing custom functions
    /// </summary>
    private List<FunctionLibrary> _functionLibraries = [];

    /// <summary>
    ///     The list of libraries providing custom functions
    /// </summary>
    public List<FunctionLibrary> FunctionLibraries
    {
        get => _functionLibraries;
        set => _functionLibraries = value;
    }

    /// <summary>
    ///     Check if variable expression is a custom function. Expression has to start with one of the registered function
    ///     library prefix.
    /// </summary>
    /// <param name="variableExpression">to be checked</param>
    /// <returns>flag (true/false)</returns>
    public bool IsFunction(string variableExpression)
    {
        return !string.IsNullOrEmpty(variableExpression) &&
               _functionLibraries.Any(c => variableExpression.StartsWith(c.Prefix));
    }

    /// <summary>
    ///     Get library for function prefix.
    /// </summary>
    /// <param name="functionPrefix"> to be searched for</param>
    /// <returns>The FunctionLibrary instance</returns>
    public FunctionLibrary GetLibraryForPrefix(string functionPrefix)
    {
        var functionLibrary = _functionLibraries.FirstOrDefault(f => f.Prefix.Equals(functionPrefix));

        return functionLibrary ??
               throw new NoSuchFunctionLibraryException(
                   "Can not find function library for prefix " + functionPrefix);
    }

    /// <summary>
    ///     Adds the given function library to this registry.
    /// </summary>
    /// <param name="functionLibrary">The function library to add.</param>
    public void AddFunctionLibrary(FunctionLibrary functionLibrary)
    {
        var prefixAlreadyUsed = _functionLibraries.Any(lib => lib.Prefix == functionLibrary.Prefix);

        if (prefixAlreadyUsed)
        {
            throw new AgenixSystemException(
                $"Function library prefix '{functionLibrary.Prefix}' is already bound to another instance. Please choose another prefix.");
        }

        _functionLibraries.Add(functionLibrary);
    }
}
