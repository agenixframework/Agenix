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
///     Function implements substring before functionality.
/// </summary>
public class SubstringBeforeFunction : IFunction
{
    /// <summary>
    ///     Executes the SubstringBeforeFunction, which returns the substring before the first occurrence of a search string.
    /// </summary>
    /// <param name="parameterList">
    ///     A list of strings where the first entry is the source string and the second entry is the
    ///     search string.
    /// </param>
    /// <param name="testContext">The current test context used during function execution.</param>
    /// <returns>Returns the substring before the first occurrence of the search string.</returns>
    /// <exception cref="InvalidFunctionUsageException">Thrown when the parameter list is null or has fewer than 2 elements.</exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count < 2)
        {
            throw new InvalidFunctionUsageException("Function parameters not set correctly");
        }

        var resultString = parameterList[0];

        if (parameterList.Count > 1)
        {
            var searchString = parameterList[1];
            resultString = resultString[..resultString.IndexOf(searchString, StringComparison.Ordinal)];
        }

        return resultString;
    }
}
