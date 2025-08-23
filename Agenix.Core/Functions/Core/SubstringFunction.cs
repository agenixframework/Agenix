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
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function implements simple substring functionality.
///     <para>
///         Function requires at least a target string and a beginIndex as function parameters. An
///         optional endIndex may be given as function parameter, too. The parameter usage looks
///         like this: substring(targetString, beginIndex, [endIndex]).
///     </para>
/// </summary>
public class SubstringFunction : IFunction
{
    /// <summary>
    ///     Executes the SubstringFunction, which returns a substring from the target string starting at beginIndex.
    /// </summary>
    /// <param name="parameterList">
    ///     A list of strings where the first entry is the target string, the second is beginIndex, and
    ///     the optional third is endIndex.
    /// </param>
    /// <param name="testContext">The current test context used during function execution.</param>
    /// <returns>Returns the substring based on the provided indices.</returns>
    /// <exception cref="InvalidFunctionUsageException">
    ///     Thrown when the parameter list is null, has fewer than 2 elements, or
    ///     beginIndex is invalid.
    /// </exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count < 2)
        {
            throw new InvalidFunctionUsageException(
                "Insufficient function parameters - parameter usage: (targetString, beginIndex, [endIndex])");
        }

        var targetString = parameterList[0];
        var beginIndex = parameterList[1];
        string endIndex = null;

        if (string.IsNullOrWhiteSpace(beginIndex))
        {
            throw new InvalidFunctionUsageException("Invalid beginIndex - please check function parameters");
        }

        if (parameterList.Count > 2)
        {
            endIndex = parameterList[2];
        }

        targetString = !string.IsNullOrWhiteSpace(endIndex)
            ? targetString.Substring(int.Parse(beginIndex), int.Parse(endIndex) - int.Parse(beginIndex))
            : targetString[int.Parse(beginIndex)..];

        return targetString;
    }
}
