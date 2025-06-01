#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
