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
using System.Globalization;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Returns the largest (closest to positive infinity) double value according to numeric argument
///     value.
/// </summary>
public class FloorFunction : IFunction
{
    /// <summary>
    ///     Executes the floor function on the first parameter.
    /// </summary>
    /// <param name="parameterList">List containing at least one numeric string value</param>
    /// <param name="testContext">Test context</param>
    /// <returns>The floor of the numeric value as a string</returns>
    /// <exception cref="InvalidFunctionUsageException">Thrown when parameters are null, empty, or contain non-numeric values</exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Function parameters must not be empty");
        }

        return Math.Floor(ParseDouble(parameterList[0])).ToString(CultureInfo.InvariantCulture);
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
