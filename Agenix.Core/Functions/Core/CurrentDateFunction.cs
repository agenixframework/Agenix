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
        if (parameterList == null || parameterList.Count == 0) return GetDefaultCurrentDate();

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