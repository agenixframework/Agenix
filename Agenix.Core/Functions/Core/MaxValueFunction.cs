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

using System.Collections.Generic;
using System.Globalization;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function returns the maximum numeric value in a set of numeric arguments.
/// </summary>
public class MaxFunction : IFunction
{
    /// <summary>
    ///     Executes the max function to find the maximum value among the parameters.
    /// </summary>
    /// <param name="parameterList">List containing numeric string values</param>
    /// <param name="testContext">Test context</param>
    /// <returns>The maximum numeric value as a string</returns>
    /// <exception cref="InvalidFunctionUsageException">Thrown when parameters are null, empty, or contain non-numeric values</exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Function parameters must not be empty");
        }

        var result = 0.0;

        for (var i = 0; i < parameterList.Count; i++)
        {
            var token = parameterList[i];
            if (i == 0 || ParseDouble(token) > result)
            {
                result = ParseDouble(token);
            }
        }

        return result.ToString(CultureInfo.InvariantCulture);
    }

    private static double ParseDouble(string value)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
        {
            return result;
        }

        throw new InvalidFunctionUsageException($"Unable to parse '{value}' as a numeric value");
    }
}
