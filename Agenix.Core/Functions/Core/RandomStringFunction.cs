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
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

public class RandomStringFunction : IFunction
{
    public const string Uppercase = "UPPERCASE";

    public const string Lowercase = "LOWERCASE";

    public const string Mixed = "MIXED";

    private readonly char[] _alphabetLower =
    {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u',
        'v', 'w', 'x', 'y', 'z'
    };

    private readonly char[] _alphabetMixed =
    {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
        'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p',
        'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
    };

    private readonly char[] _alphabetUpper =
    {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U',
        'V', 'W', 'X', 'Y', 'Z'
    };

    private readonly Random _generator = new(Environment.TickCount);

    private readonly char[] _numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

    public string Execute(List<string> parameterList, TestContext testContext)
    {
        var notationMethod = Mixed;
        var includeNumbers = false;

        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Function parameters must not be empty");
        }

        if (parameterList.Count > 3)
        {
            throw new InvalidFunctionUsageException("Too many parameters for function");
        }

        var numberOfLetters = Convert.ToInt32(parameterList[0]);

        if (numberOfLetters < 0)
        {
            throw new InvalidFunctionUsageException(
                "Invalid parameter definition. Number of letters must not be positive non-zero integer value");
        }

        if (parameterList.Count > 1)
        {
            notationMethod = parameterList[1];
        }

        if (parameterList.Count > 2)
        {
            includeNumbers = Convert.ToBoolean(parameterList[2]);
        }

        return notationMethod switch
        {
            Uppercase => GetRandomString(numberOfLetters, _alphabetUpper, includeNumbers),

            Lowercase => GetRandomString(numberOfLetters, _alphabetLower, includeNumbers),

            _ => GetRandomString(numberOfLetters, _alphabetMixed, includeNumbers)
        };
    }

    /// <summary>
    ///     Random number generator aware string generating method
    /// </summary>
    /// <param name="numberOfLetters"></param>
    /// <param name="alphabet"></param>
    /// <param name="includeNumbers"></param>
    /// <returns></returns>
    public string GetRandomString(int numberOfLetters, char[] alphabet, bool includeNumbers)
    {
        var builder = new StringBuilder();

        var upperRange = alphabet.Length - 1;

        // make sure first character is not a number
        builder.Append(alphabet[_generator.Next(upperRange)]);

        if (includeNumbers)
        {
            upperRange += _numbers.Length;
        }

        for (var i = 1; i < numberOfLetters; i++)
        {
            var letterIndex = _generator.Next(upperRange);

            builder.Append(letterIndex > alphabet.Length - 1
                ? _numbers[letterIndex - alphabet.Length]
                : alphabet[letterIndex]);
        }

        return builder.ToString();
    }
}
