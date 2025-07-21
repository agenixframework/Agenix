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

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function to choose one random value from a list of strings. The enumeration values to choose from
///     can either be specified as parameters or in the <see cref="Values" /> property of
///     an instance of this class. These two possibilities can only be used exclusively - either empty values
///     property and non-empty parameters or empty parameters and non-empty values property.
/// </summary>
/// <remarks>
///     <para>Example custom function definition and the corresponding usage in a test:</para>
///     <para>You can define custom functions with predefined values or use the function with parameters directly.</para>
///     <para>Usage with parameters: <c>citrus:randomEnumValue('200', '401', '500')</c></para>
///     <para>This will randomly select one of the provided values.</para>
///     <para>
///         You should choose which one of the two flavors to use based on the number of times you use this function - if
///         you need it in
///         only one special case, you may go with specifying the list as arguments, otherwise you should define a custom
///         function and reuse it.
///     </para>
/// </remarks>
public class RandomEnumValueFunction : IFunction
{
    private readonly Random _random = new();

    /// <summary>
    ///     Gets the predefined values for this function.
    /// </summary>
    public List<string> Values { get; set; }

    /// <summary>
    ///     Executes the random enum value function.
    /// </summary>
    /// <param name="parameterList">List of string values to choose from (if Values property is not set)</param>
    /// <param name="testContext">Test context</param>
    /// <returns>A randomly selected value from the available options</returns>
    /// <exception cref="InvalidFunctionUsageException">
    ///     Thrown when both parameters and Values property are provided, or when
    ///     no values are available
    /// </exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (Values == null)
        {
            return RandomValue(parameterList);
        }

        if (parameterList.Count > 0)
        {
            throw new InvalidFunctionUsageException("The enumeration values have already been set");
        }

        return RandomValue(Values);
    }

    /// <summary>
    ///     Pseudo-randomly choose one of the supplied values and return it.
    /// </summary>
    /// <param name="values">List of values to choose from</param>
    /// <returns>A randomly selected value</returns>
    /// <exception cref="InvalidFunctionUsageException">Thrown when values are null or empty</exception>
    protected string RandomValue(List<string> values)
    {
        if (values == null || values.Count == 0)
        {
            throw new InvalidFunctionUsageException("No values to choose from");
        }

        var idx = _random.Next(values.Count);
        return values[idx];
    }
}
