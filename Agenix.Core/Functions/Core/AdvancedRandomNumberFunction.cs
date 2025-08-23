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
///     A function for generating random double values with specified decimal places and range. This
///     function includes options to specify the number of decimal places, minimum and maximum values,
///     and whether to include or exclude the minimum and maximum values.
///     <para>
///         Parameters:
///         <list type="number">
///             <item>
///                 Decimal places: The number of decimal places in the generated random number (optional, default: 2).
///                 Note that definition of 0 results in an integer.
///             </item>
///             <item>
///                 Min value: The minimum value for the generated random number (optional, default:
///                 <see cref="DefaultMinValue" />).
///             </item>
///             <item>
///                 Max value: The maximum value for the generated random number (optional, default:
///                 <see cref="DefaultMaxValue" />).
///             </item>
///             <item>Exclude min: Whether to exclude the minimum value (optional, default: false).</item>
///             <item>Exclude max: Whether to exclude the maximum value (optional, default: false).</item>
///             <item>Multiple of: The generated number will be a multiple of this value (optional).</item>
///             <item>Format pattern: The format pattern for the returned string (optional, default: null)</item>
///         </list>
///     </para>
/// </summary>
public class AdvancedRandomNumberFunction : IFunction
{
    private const decimal DefaultMaxValue = 1000000m;
    private const decimal DefaultMinValue = -DefaultMaxValue;
    private static readonly Random Random = new();

    /// <summary>
    ///     Executes the core functionality of generating a formatted random number
    ///     based on the specified parameters.
    /// </summary>
    /// <param name="parameterList">
    ///     A list of string parameters to configure the random number generation, such as decimal
    ///     places, range, exclusions, and formatting.
    /// </param>
    /// <param name="testContext">The testing context instance providing additional contextual information for execution.</param>
    /// <returns>A formatted random number as a string based on the specified parameters.</returns>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null)
        {
            throw new InvalidFunctionUsageException("Function parameters must not be null.");
        }

        var decimalPlaces = GetParameter(parameterList, 0, int.Parse, 2);
        if (decimalPlaces < 0)
        {
            throw new InvalidFunctionUsageException(
                "Decimal places must be a non-negative integer value.");
        }

        var minValue = GetParameter(parameterList, 1, s => decimal.Parse(s, CultureInfo.InvariantCulture),
            DefaultMinValue);
        var maxValue = GetParameter(parameterList, 2, s => decimal.Parse(s, CultureInfo.InvariantCulture),
            DefaultMaxValue);
        if (minValue > maxValue)
        {
            throw new InvalidFunctionUsageException("Min value must be less than max value.");
        }

        var excludeMin = GetParameter(parameterList, 3, bool.Parse, false);
        var excludeMax = GetParameter(parameterList, 4, bool.Parse, false);
        var multiple =
            GetParameter<decimal?>(parameterList, 5, s => decimal.Parse(s, CultureInfo.InvariantCulture), null);
        var formatPattern = GetParameter(parameterList, 6, s => s, null);

        var randomValue = GetRandomNumber(decimalPlaces, minValue, maxValue, excludeMin, excludeMax, multiple);

        return FormatRandomNumber(randomValue, decimalPlaces, formatPattern);
    }

    private static T GetParameter<T>(IList<string> parameters, int index, Func<string, T> parser, T defaultValue)
    {
        if (index < parameters.Count)
        {
            var param = parameters[index];
            return string.Equals(param, "null", StringComparison.OrdinalIgnoreCase)
                ? defaultValue
                : ParseParameter(index + 1, param, parser);
        }

        return defaultValue;
    }

    private static T ParseParameter<T>(int index, string text, Func<string, T> parseFunction)
    {
        try
        {
            var value = parseFunction(text);

            // Only check for null - default values for value types are valid
            if (typeof(T).IsClass && Equals(value, default(T)))
            {
                throw new InvalidFunctionUsageException(
                    $"Text '{text}' could not be parsed to '{typeof(T).Name}'. Resulting value is null");
            }

            return value;
        }
        catch (Exception e)
        {
            throw new InvalidFunctionUsageException(
                $"Invalid parameter at index {index}. {text} must be parsable to {typeof(T).Name}.", e);
        }
    }

    /// <summary>
    ///     Static number generator method.
    /// </summary>
    private decimal? GetRandomNumber(int decimalPlaces,
        decimal minValue,
        decimal maxValue,
        bool excludeMin,
        bool excludeMax,
        decimal? multiple)
    {
        minValue = excludeMin ? IncrementToExclude(minValue) : minValue;
        maxValue = excludeMax ? DecrementToExclude(maxValue) : maxValue;

        var range = maxValue - minValue;

        if (multiple.HasValue)
        {
            return CreateMultipleOf(minValue, maxValue, multiple.Value);
        }

        var randomValue = CreateRandomValue(minValue, range, Random.NextDouble());
        randomValue = Math.Round(randomValue, decimalPlaces, MidpointRounding.AwayFromZero);

        return randomValue;
    }

    /// <summary>
    ///     Formats a random number into a string representation based on the specified decimal places and format pattern.
    /// </summary>
    /// <param name="randomValue">The random number to format. Can be null if no value is generated.</param>
    /// <param name="decimalPlaces">The number of decimal places to round the value to.</param>
    /// <param name="formatPattern">
    ///     An optional string defining a custom format for the number. If null or empty, default
    ///     rounding is applied.
    /// </param>
    /// <returns>A formatted string representation of the random number, or "Infinity" if no random value exists.</returns>
    public static string FormatRandomNumber(decimal? randomValue, int decimalPlaces, string formatPattern)
    {
        if (!randomValue.HasValue)
        {
            // May only happen if multiple is out of range of min/max
            return double.PositiveInfinity.ToString(CultureInfo.InvariantCulture);
        }

        if (!string.IsNullOrEmpty(formatPattern))
        {
            var culture = CultureInfo.CreateSpecificCulture("en-US"); // Ensure '.' as decimal separator
            return randomValue.Value.ToString(formatPattern, culture);
        }

        // Default behavior: round to decimalPlaces if no format is provided
        return decimalPlaces == 0
            ? randomValue.Value.ToString("F0", CultureInfo.InvariantCulture)
            : Math.Round(randomValue.Value, decimalPlaces, MidpointRounding.AwayFromZero)
                .ToString(CultureInfo.InvariantCulture);
    }

    // Pass in random for testing
    protected virtual decimal CreateRandomValue(decimal minValue, decimal range, double random)
    {
        var offset = range * (decimal)random;
        var value = minValue + offset;
        return value > decimal.MaxValue ? decimal.MaxValue : value;
    }

    private static decimal LargestMultipleOf(decimal highest, decimal multipleOf)
    {
        var factor = Math.Floor(highest / multipleOf);
        return multipleOf * factor;
    }

    private static decimal LowestMultipleOf(decimal lowest, decimal multipleOf)
    {
        var factor = Math.Ceiling(lowest / multipleOf);
        return multipleOf * factor;
    }

    private static decimal IncrementToExclude(decimal val)
    {
        var increment = DetermineIncrement(val);
        return Math.Round(val + increment, FindLeastSignificantDecimalPlace(val), MidpointRounding.AwayFromZero);
    }

    private static decimal DecrementToExclude(decimal val)
    {
        var increment = DetermineIncrement(val);
        return Math.Round(val - increment, FindLeastSignificantDecimalPlace(val), MidpointRounding.AwayFromZero);
    }

    private static decimal DetermineIncrement(decimal number)
    {
        return 1m / (decimal)Math.Pow(10, FindLeastSignificantDecimalPlace(number));
    }

    private static int FindLeastSignificantDecimalPlace(decimal number)
    {
        var parts = number.ToString(CultureInfo.InvariantCulture).Split('.');

        if (parts.Length == 1)
        {
            return 0;
        }

        // Remove trailing zeros
        var decimalPart = parts[1].TrimEnd('0');
        return decimalPart.Length;
    }

    private static decimal? CreateMultipleOf(decimal minimum, decimal maximum, decimal multipleOf)
    {
        var lowestMultiple = LowestMultipleOf(minimum, multipleOf);
        var largestMultiple = LargestMultipleOf(maximum, multipleOf);

        if (lowestMultiple > largestMultiple)
        {
            return null;
        }

        var range = (largestMultiple - lowestMultiple) / multipleOf;

        if (range > 11)
        {
            range = 10;
        }

        long factor = 0;
        if (range != 0)
        {
            factor = Random.Shared.NextInt64(1, (long)range + 1);
        }

        var randomMultiple = lowestMultiple + multipleOf * factor;
        randomMultiple = Math.Round(randomMultiple, FindLeastSignificantDecimalPlace(multipleOf),
            MidpointRounding.AwayFromZero);

        return randomMultiple;
    }
}
