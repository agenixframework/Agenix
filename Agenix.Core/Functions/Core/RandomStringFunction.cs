#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
