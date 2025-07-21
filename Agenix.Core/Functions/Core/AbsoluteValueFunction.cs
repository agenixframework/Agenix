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
///     Provides functionality to compute the absolute value of a numeric input string.
/// </summary>
public class AbsoluteFunction : IFunction
{
    /// <summary>
    ///     Executes the absolute value function
    /// </summary>
    /// <param name="parameterList">List of parameters</param>
    /// <param name="testContext">Test context</param>
    /// <returns>Absolute value as string</returns>
    /// <exception cref="InvalidFunctionUsageException">Thrown when parameters are invalid</exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Function parameters must not be empty");
        }

        var param = parameterList[0];

        if (param.Contains('.'))
        {
            var value = double.Parse(param);
            // If the number was negative, remove the minus sign to preserve original format
            return value < 0 ? param[1..] : param;
        }
        else
        {
            var value = int.Parse(param);
            // Use Math.Abs and convert back to string for integers
            return Math.Abs(value).ToString();
        }
    }
}
