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
using System.Linq;
using System.Text;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function returning a random numeric value. Argument specifies the number of digits and padding bool flag
/// </summary>
public class RandomNumberFunction : IFunction
{
    /// <summary>
    ///     Basic seed generating random number
    /// </summary>
    private static readonly Random _generator = new(Environment.TickCount);

    public string Execute(List<string> parameterList, TestContext testContext)
    {
        var paddingOn = true;

        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Function parameters must not be empty");
        }

        if (parameterList.Count > 2)
        {
            throw new InvalidFunctionUsageException("Too many parameters for function");
        }

        var numberLength = int.Parse(parameterList[0]);
        if (numberLength < 0)
        {
            throw new InvalidFunctionUsageException(
                "Invalid parameter definition. Number of letters must not be positive non-zero integer value");
        }

        if (parameterList.Count > 1)
        {
            paddingOn = bool.Parse(parameterList[1]);
        }

        return GetRandomNumber(numberLength, paddingOn);
    }


    public string GetRandomNumber(int numberLength, bool paddingOn)
    {
        if (numberLength < 1)
        {
            throw new InvalidFunctionUsageException(
                "numberLength must be greater than 0 - supplied " + numberLength);
        }

        var buffer = new StringBuilder();
        for (var i = 0; i < numberLength; i++)
        {
            buffer.Append(_generator.Next(10));
        }

        return CheckLeadingZeros(buffer.ToString(), paddingOn);
    }

    /// <summary>
    ///     Remove leading Zero numbers.
    /// </summary>
    /// <param name="generated"></param>
    /// <param name="paddingOn"></param>
    /// <returns></returns>
    public static string CheckLeadingZeros(string generated, bool paddingOn)
    {
        return paddingOn ? ReplaceLeadingZero(generated) : RemoveLeadingZeros(generated);
    }

    /// <summary>
    ///     Removes leading zero numbers if present.
    /// </summary>
    /// <param name="generated"></param>
    /// <returns></returns>
    private static string RemoveLeadingZeros(string generated)
    {
        var builder = new StringBuilder();
        var leading = true;
        foreach (var t in generated.Where(t => t != '0' || !leading))
        {
            leading = false;
            builder.Append(t);
        }

        if (builder.Length == 0)
        // very unlikely to happen, ensures that empty string is not returned
        {
            builder.Append('0');
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Replaces first leading zero number if present.
    /// </summary>
    /// <param name="generated"></param>
    /// <returns></returns>
    private static string ReplaceLeadingZero(string generated)
    {
        if (generated[0] != '0')
        {
            return generated;
        }

        // find number > 0 as a replacement to avoid leading zero numbers
        var replacement = 0;
        while (replacement == 0)
        {
            replacement = _generator.Next(10);
        }

        return replacement + generated[1..];
    }
}
