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
///     Library holding a set of functions. Each library defines a function prefix as namespace, so there will be no naming
///     conflicts when using multiple libraries at a time.
/// </summary>
public class FunctionLibrary
{
    /// <summary>
    ///     The Default function prefix
    /// </summary>
    private const string DefaultPrefix = "agenix:";

    /// <summary>
    ///     The dictionary (map) of functions in this library
    /// </summary>
    private IDictionary<string, IFunction> _members =
        new Dictionary<string, IFunction>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Name of a function library
    /// </summary>
    private string _name = DefaultPrefix;

    /// <summary>
    ///     Function library prefix
    /// </summary>
    private string _prefix = DefaultPrefix;

    /// <summary>
    ///     The dictionary (map) of functions in this library
    /// </summary>
    public IDictionary<string, IFunction> Members
    {
        get => _members;
        set => _members = value;
    }

    /// <summary>
    ///     Name of function library
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     Function library prefix
    /// </summary>
    public string Prefix
    {
        get => _prefix;
        set => _prefix = value;
    }

    /// <summary>
    ///     Try to find function in library by name.
    /// </summary>
    /// <param name="functionName">The  function name.</param>
    /// <returns>The function instance.</returns>
    public IFunction GetFunction(string functionName)
    {
        if (!_members.ContainsKey(functionName))
        {
            throw new NoSuchFunctionException("Can not find function '" + functionName + "' in library " + _name +
                                              " (" + _prefix + ")");
        }

        return _members[functionName];
    }

    /// <summary>
    ///     Does this function library know a function with the given name.
    /// </summary>
    /// <param name="functionName">The name to search for.</param>
    /// <returns>The flag to mark existence.</returns>
    public bool KnowsFunction(string functionName)
    {
        var functionPrefix = functionName.Substring(0, functionName.IndexOf(':') + 1);

        if (!functionPrefix.Equals(_prefix))
        {
            return false;
        }

        return _members.ContainsKey(
            functionName.Substring(functionName.IndexOf(':') + 1, functionName.IndexOf('(')));
    }
}
