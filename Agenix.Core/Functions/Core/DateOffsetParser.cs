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
using System.Text;
using Agenix.Api.Context;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Represents a base parser class for handling date offsets by parsing strings and applying
///     modifications to date values. It provides utility functions for calculating and applying
///     offsets such as years, months, days, hours, minutes, and seconds, based on specified input.
/// </summary>
public abstract class DateOffSetParser : IFunction
{
    /// <summary>
    ///     Specifies the default date format as a constant string. This format is used
    ///     for date-to-string conversions when no custom format is provided by the user.
    ///     The default value is "dd.MM.yyyy".
    /// </summary>
    protected const string DefaultDateFormat = "dd.MM.yyyy";

    /// <summary>
    ///     Abstract method to be implemented by derived classes
    /// </summary>
    public abstract string Execute(List<string> parameterList, TestContext testContext);

    /// <summary>
    ///     Adds/removes date value offset by parsing offset string for
    ///     year/month/day/hour/minute/second offsets.
    /// </summary>
    /// <param name="dateTime">DateTime to modify</param>
    /// <param name="offsetString">Offset string to parse</param>
    protected static void ApplyDateOffset(ref DateTime dateTime, string offsetString)
    {
        dateTime = dateTime.AddYears(GetDateValueOffset(offsetString, 'y'));
        dateTime = dateTime.AddMonths(GetDateValueOffset(offsetString, 'M'));
        dateTime = dateTime.AddDays(GetDateValueOffset(offsetString, 'd'));
        dateTime = dateTime.AddHours(GetDateValueOffset(offsetString, 'h'));
        dateTime = dateTime.AddMinutes(GetDateValueOffset(offsetString, 'm'));
        dateTime = dateTime.AddSeconds(GetDateValueOffset(offsetString, 's'));
    }

    /// <summary>
    ///     Parse offset string and add or subtract date offset value.
    /// </summary>
    /// <param name="offsetString">Offset string to parse</param>
    /// <param name="c">Character to look for</param>
    /// <returns>Offset value</returns>
    protected static int GetDateValueOffset(string offsetString, char c)
    {
        var charList = new List<char>();

        var index = offsetString.IndexOf(c);
        if (index == -1)
        {
            return 0;
        }

        for (var i = index - 1; i >= 0; i--)
        {
            if (char.IsDigit(offsetString[i]))
            {
                charList.Insert(0, offsetString[i]);
            }
            else
            {
                var offsetValue = new StringBuilder();
                offsetValue.Append('0');
                foreach (var character in charList)
                {
                    offsetValue.Append(character);
                }

                return offsetString[i] == '-' ? int.Parse("-" + offsetValue) : int.Parse(offsetValue.ToString());
            }
        }

        return 0;
    }
}
