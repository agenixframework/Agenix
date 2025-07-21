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
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function changes the given date value by adding/subtracting day/month/year/hour/minute
///     offset values. Class uses a special date format to parse date string to DateTime instance.
/// </summary>
public class ChangeDateFunction : DateOffSetParser
{
    /// <summary>
    ///     Logger
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger<ChangeDateFunction>();

    /// <summary>
    ///     Executes the change date function.
    /// </summary>
    /// <param name="parameterList">
    ///     List of parameters:
    ///     [0] - Date string to parse
    ///     [1] - Date offset (optional)
    ///     [2] - Date format pattern (optional)
    /// </param>
    /// <param name="testContext">Test context</param>
    /// <returns>The modified date as a formatted string</returns>
    /// <exception cref="InvalidFunctionUsageException">Thrown when parameters are null or empty</exception>
    /// <exception cref="AgenixSystemException">Thrown when date parsing or formatting fails</exception>
    public override string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Function parameters must not be empty");
        }

        DateTime dateTime;
        string result;

        var dateFormatPattern = parameterList.Count > 2 ? parameterList[2] : DefaultDateFormat;

        try
        {
            dateTime = DateTime.ParseExact(parameterList[0], dateFormatPattern, CultureInfo.InvariantCulture);
        }
        catch (FormatException e)
        {
            throw new AgenixSystemException(
                $"Failed to parse date '{parameterList[0]}' with format '{dateFormatPattern}'", e);
        }

        if (parameterList.Count > 1)
        {
            ApplyDateOffset(ref dateTime, parameterList[1]);
        }

        try
        {
            result = dateTime.ToString(dateFormatPattern, CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error while formatting date parameter value");
            throw new AgenixSystemException("Error while formatting date parameter value", e);
        }

        return result;
    }
}
