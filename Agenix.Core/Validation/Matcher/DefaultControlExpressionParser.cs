using System;
using System.Collections.Generic;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Validation.Matcher;

/// <summary>
///     Default implementation of control expression parser.
/// </summary>
public class DefaultControlExpressionParser : IControlExpressionParser
{
    public const char DefaultDelimiter = '\'';

    public List<string> ExtractControlValues(string controlExpression, char delimiter)
    {
        var useDelimiter = delimiter.Equals('\0') ? DefaultDelimiter : delimiter;
        var extractedParameters = new List<string>();

        if (string.IsNullOrEmpty(controlExpression)) return extractedParameters;

        ExtractParameters(controlExpression, useDelimiter, extractedParameters, 0);
        if (extractedParameters.Count == 0)
            // if the controlExpression has text but no parameters were extracted, then assume that
            // the controlExpression itself is the only parameter
            extractedParameters.Add(controlExpression);

        return extractedParameters;
    }

    private void ExtractParameters(string controlExp, char delimiter, ICollection<string> extractedParameters,
        int searchFrom)
    {
        while (true)
        {
            var startParameter = controlExp.IndexOf(delimiter, searchFrom);

            if (startParameter == -1) return; // No starting delimiter found, exit

            var endParameter = controlExp.IndexOf(delimiter, startParameter + 1);

            // Ensure end parameter check is within bounds and find the right delimiter
            while (endParameter != -1 && endParameter < controlExp.Length - 1 && controlExp[endParameter + 1] != ',' &&
                   controlExp[endParameter + 1] != ')') endParameter = controlExp.IndexOf(delimiter, endParameter + 1);

            if (endParameter == -1)
                throw new CoreSystemException(
                    $"No matching delimiter ({delimiter}) found after position '{startParameter}' in control expression: {controlExp}");

            var extractedParameter = controlExp.Substring(startParameter + 1, endParameter - startParameter - 1);
            extractedParameters.Add(extractedParameter);

            // Debugging outputs
            Console.WriteLine($"Extracted Parameter: {extractedParameter}");

            // Move searchFrom to endParameter + 1 to continue searching the remainder of the string
            searchFrom = endParameter + 1;

            // Check if there are more delimiters and commas to continue
            if (controlExp.IndexOf(delimiter, searchFrom) == -1 || controlExp.IndexOf(',', searchFrom) == -1) break;
        }
    }
}