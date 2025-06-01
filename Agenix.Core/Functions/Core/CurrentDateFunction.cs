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
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using static System.Console;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function returning the actual date as formatted string value. User specifies format string as argument.
///     TODO: Function has also to support additional date offset in order to manipulate result date value. E.g.
///     core:CurrentDate('yyyy-MM-dd', '+1y') -> current date + one year
/// </summary>
public class CurrentDateFunction : IFunction
{
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            return GetDefaultCurrentDate();
        }

        try
        {
            return parameterList[0].Equals("") ? GetDefaultCurrentDate() : DateTime.Now.ToString(parameterList[0]);
        }
        catch (Exception e)
        {
            WriteLine("Error while formatting data value {0}", e);
            throw new AgenixSystemException(e.Message);
        }
    }

    private static string GetDefaultCurrentDate()
    {
        return DateTime.Now.ToString("dd.MM.yyyy");
    }
}
