using System.Collections.Generic;
using System.Text.RegularExpressions;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function searches for occurrences of a given character sequence and replaces all findings with given replacement
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
            //resultString = resultString.Replace(regex, replacement);
            resultString = Regex.Replace(resultString, regex, replacement);

        return resultString;
    }
}