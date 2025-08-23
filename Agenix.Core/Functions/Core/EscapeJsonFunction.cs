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
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     This function takes a JSON string as input and escapes all double quotes within it.
/// </summary>
/// <remarks>
///     <para>The input must be a single non-empty string containing a valid JSON, wrapped in double quotes.</para>
///     <para>
///         If the input is invalid (null, empty, or contains more than one string), an <see cref="ArgumentException" />
///         will be thrown.
///     </para>
///     <para>Example input: <c>"{\"mySuperJson\": \"valium\"}"</c></para>
///     <para>Example output: <c>"{\\\"mySuperJson\\\": \\\"valium\\\"}"</c></para>
/// </remarks>
public class EscapeJsonFunction : IFunction
{
    /// <summary>
    ///     Executes the JSON escape function.
    /// </summary>
    /// <param name="parameterList">List containing a single JSON string to escape</param>
    /// <param name="testContext">Test context</param>
    /// <returns>The JSON string with escaped double quotes</returns>
    /// <exception cref="ArgumentException">Thrown when input is invalid</exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList is not { Count: 1 }
            || string.IsNullOrEmpty(parameterList[0]))
        {
            throw new ArgumentException(
                "Parameter List input is invalid (null, empty, or contains more than one string). The input must be a single non-empty string containing a valid JSON, wrapped in double quotes to be transformed into a valid string."
            );
        }

        return EscapeJson(parameterList[0]);
    }

    /// <summary>
    ///     Escapes double quotes in a JSON string.
    /// </summary>
    /// <param name="jsonString">The JSON string to escape</param>
    /// <returns>The escaped JSON string</returns>
    private static string EscapeJson(string jsonString)
    {
        return jsonString.Replace("\"", "\\\"");
    }
}
