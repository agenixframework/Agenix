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

using Agenix.Api.Exceptions;

namespace Agenix.Api.Validation.Matcher;

/// <summary>
///     Default implementation of control expression parser.
/// </summary>
public class DefaultControlExpressionParser : IControlExpressionParser
{
    public const char DefaultDelimiter = '\'';

    /// <summary>
    ///     Extracts individual control values from a control expression string based on the specified delimiter.
    ///     If no parameters are explicitly found in the control expression, the entire control expression
    ///     is treated as a single control value.
    /// </summary>
    /// <param name="controlExpression">The control expression string to parse for control values.</param>
    /// <param name="delimiter">The character used to delimit control values within the control expression.</param>
    /// <returns>A list of strings representing the extracted control values.</returns>
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

    /// <summary>
    ///     Extracts parameters from a control expression string using the specified delimiter
    ///     and adds them to the provided collection.
    /// </summary>
    /// <param name="controlExp">The control expression string to parse for parameters.</param>
    /// <param name="delimiter">The character used to delimit parameters within the control expression.</param>
    /// <param name="extractedParameters">A collection to which extracted parameters are added.</param>
    /// <param name="searchFrom">The starting index from which to search for parameters in the control expression.</param>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when a starting delimiter is found but a matching ending delimiter is not
    ///     present in the control expression.
    /// </exception>
    private void ExtractParameters(string controlExp, char delimiter, ICollection<string> extractedParameters,
        int searchFrom)
    {
        while (true)
        {
            var startParameter = controlExp.IndexOf(delimiter, searchFrom);

            if (startParameter == -1) return; // No starting delimiter found, exit

            var endParameter = controlExp.IndexOf(delimiter, startParameter + 1);

            // Ensure the end parameter check is within bounds and find the right delimiter
            while (endParameter != -1 && endParameter < controlExp.Length - 1 && controlExp[endParameter + 1] != ',' &&
                   controlExp[endParameter + 1] != ')') endParameter = controlExp.IndexOf(delimiter, endParameter + 1);

            if (endParameter == -1)
                throw new AgenixSystemException(
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
