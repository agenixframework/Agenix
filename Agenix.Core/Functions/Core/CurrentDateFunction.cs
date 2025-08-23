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
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function returning the actual date as a formatted string value. User specifies format string as an argument.
/// </summary>
public class CurrentDateFunction : DateOffSetParser
{
    /// <summary>
    ///     Logger
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger<CurrentDateFunction>();

    /// <summary>
    ///     Executes the current date function
    /// </summary>
    /// <param name="parameterList">List of parameters</param>
    /// <param name="testContext">Test context</param>
    /// <returns>Formatted date string</returns>
    /// <exception cref="AgenixSystemException">Thrown when date formatting fails</exception>
    public override string Execute(List<string> parameterList, TestContext testContext)
    {
        var dateTime = DateTime.Now;

        string result;

        var dateFormat = parameterList is { Count: > 0 } && parameterList.Any(p => !string.IsNullOrWhiteSpace(p))
            ? parameterList[0]
            : DefaultDateFormat;

        if (parameterList is { Count: > 1 })
        {
            ApplyDateOffset(ref dateTime, parameterList[1]);
        }

        try
        {
            result = dateTime.ToString(dateFormat);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error while formatting date value");
            throw new AgenixSystemException(e.Message);
        }

        return result;
    }
}
