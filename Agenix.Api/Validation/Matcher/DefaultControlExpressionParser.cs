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

        if (string.IsNullOrEmpty(controlExpression))
        {
            return extractedParameters;
        }

        ExtractParameters(controlExpression, useDelimiter, extractedParameters, 0);
        if (extractedParameters.Count == 0)
            // if the controlExpression has text but no parameters were extracted, then assume that
            // the controlExpression itself is the only parameter
        {
            extractedParameters.Add(controlExpression);
        }

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
            if (startParameter == -1)
            {
                return; // No starting delimiter found, exit
            }

            var endParameter = FindMatchingDelimiter(controlExp, delimiter, startParameter);
            if (endParameter == -1)
            {
                throw new AgenixSystemException(
                    $"No matching delimiter ({delimiter}) found after position '{startParameter}' in control expression: {controlExp}");
            }

            var extractedParameter = controlExp.Substring(startParameter + 1, endParameter - startParameter - 1);
            extractedParameters.Add(extractedParameter);

            // Move searchFrom to after the closing delimiter
            searchFrom = endParameter + 1;

            // Skip any whitespace or commas to find the next parameter
            while (searchFrom < controlExp.Length &&
                   (controlExp[searchFrom] == ',' || char.IsWhiteSpace(controlExp[searchFrom])))
            {
                searchFrom++;
            }

            // If we've reached the end or there's no delimiter ahead, we're done
            if (searchFrom >= controlExp.Length || controlExp.IndexOf(delimiter, searchFrom) == -1)
            {
                break;
            }
        }
    }

    /// <summary>
    ///     Finds the matching closing delimiter by looking for the next delimiter that appears
    ///     right before a comma or at the end of the string, indicating it's a closing delimiter
    ///     rather than a delimiter within the content.
    /// </summary>
    /// <param name="controlExp">The control expression string.</param>
    /// <param name="delimiter">The delimiter character to match.</param>
    /// <param name="startDelimiterPos">The position of the opening delimiter.</param>
    /// <returns>The index of the matching delimiter, or -1 if not found.</returns>
    private int FindMatchingDelimiter(string controlExp, char delimiter, int startDelimiterPos)
    {
        var searchFrom = startDelimiterPos + 1;

        while (searchFrom < controlExp.Length)
        {
            var nextDelimiter = controlExp.IndexOf(delimiter, searchFrom);
            if (nextDelimiter == -1)
            {
                return -1; // No more delimiters found
            }

            // Check if this delimiter is followed by a comma, whitespace, or is at the end
            // This indicates it's a closing delimiter
            var nextPos = nextDelimiter + 1;
            if (nextPos >= controlExp.Length ||
                controlExp[nextPos] == ',' ||
                char.IsWhiteSpace(controlExp[nextPos]))
            {
                return nextDelimiter; // Found the closing delimiter
            }

            // This delimiter is within the content, continue searching
            searchFrom = nextDelimiter + 1;
        }

        return -1; // No matching delimiter found
    }
}
