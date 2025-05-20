using System.Collections.Generic;
using System.Text.RegularExpressions;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function searches for occurrences of a given character sequence and replaces all findings with a given replacement
///     string.
/// </summary>
public class TranslateFunction : IFunction
{
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count < 3)
            throw new InvalidFunctionUsageException("Function parameters not set correctly");

        var resultString = parameterList[0];

        string regex = null;
        string replacement = null;

        if (parameterList.Count > 1) regex = parameterList[1];

        if (parameterList.Count > 2) replacement = parameterList[2];

        if (regex != null && replacement != null)
            resultString = Regex.Replace(resultString, regex, replacement);

        return resultString;
    }
}