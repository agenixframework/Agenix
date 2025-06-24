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

using System;
using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Api.Util;
using Microsoft.Extensions.Configuration;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function to get environment variable settings.
/// </summary>
public class EnvironmentPropertyFunction : IFunction
{
    /**
     * Configuration environment
     */
    private IConfiguration _environment;

    /// <summary>
    ///     Executes the function to retrieve an environment property value based on provided parameters.
    /// </summary>
    /// <param name="parameterList">
    ///     A list of parameters where the first element is the environment property name and the
    ///     second element, if provided, is the default value.
    /// </param>
    /// <param name="context">The test context in which the function is executed, providing necessary runtime information.</param>
    /// <returns>
    ///     The resolved property value as a string, or the default value if the property is not found. Throws an
    ///     exception if neither is available.
    /// </returns>
    public string Execute(List<string> parameterList, TestContext context)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Invalid function parameters - must set environment property name");
        }

        var propertyName = parameterList[0];

        var defaultValue = parameterList.Count > 1 ? Optional<string>.Of(parameterList[1]) : Optional<string>.Empty;

        var value = Optional<string>.OfNullable(_environment != null
            ? _environment[propertyName]
            : Environment.GetEnvironmentVariable(propertyName));

        return value.OrElseGet(() =>
            defaultValue.OrElseThrow(() =>
                new AgenixSystemException($"Failed to resolve property '{propertyName}' in environment")));
    }

    /// <summary>
    ///     Sets a new configuration environment.
    /// </summary>
    /// <param name="newEnvironment">The new configuration environment to be set.</param>
    public void SetEnvironment(IConfiguration newEnvironment)
    {
        _environment = newEnvironment;
    }

    /// <summary>
    ///     Retrieves the current configuration environment.
    /// </summary>
    /// <returns>The current configuration environment.</returns>
    public IConfiguration GetEnvironment()
    {
        return _environment;
    }
}
