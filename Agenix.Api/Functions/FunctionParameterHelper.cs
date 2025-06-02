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

namespace Agenix.Api.Functions;

/// <summary>
///     Helper class parsing a parameter string and converting the tokens to a parameter list.
/// </summary>
public sealed class FunctionParameterHelper
{
    /// <summary>
    ///     Prevent class instantiation.
    /// </summary>
    private FunctionParameterHelper()
    {
    }

    /// <summary>
    ///     Convert a parameter string to a list of parameters.
    /// </summary>
    /// <param name='parameterString'>Comma separated parameter string.</param>
    /// <returns>The list of parameters</returns>
    public static List<string> GetParameterList(string parameterString)
    {
        var stringsSplit = parameterString.Split(",");

        IList<string> parameterList = stringsSplit
            .Select(stringSplit => CutOffSingleQuotes(stringSplit.Trim()))
            .ToList();

        var postProcessedList = new List<string>();

        for (var i = 0; i < parameterList.Count; i++)
        {
            var next = i + 1;

            var processed = parameterList[i];

            if (processed.StartsWith("'") && !processed.EndsWith("'"))
            {
                while (next < parameterList.Count)
                {
                    if (parameterString.Contains(processed + ", " + parameterList[next]))
                    {
                        processed += ", " + parameterList[next];
                    }
                    else if (parameterString.Contains(processed + ", " + parameterList[next]))
                    {
                        processed += "," + parameterList[next];
                    }
                    else if (parameterString.Contains(processed + " , " + parameterList[next]))
                    {
                        processed += " , " + parameterList[next];
                    }
                    else
                    {
                        processed += parameterList[next];
                    }

                    i++;
                    if (parameterList[next].EndsWith("'"))
                    {
                        break;
                    }

                    next++;
                }
            }

            postProcessedList.Add(CutOffSingleQuotes(processed));
        }

        return postProcessedList;
    }

    private static string CutOffSingleQuotes(string param)
    {
        if (param.Equals("'"))
        {
            return "";
        }

        if (param.Length > 1 && param[0] == '\'' && param[^1] == '\'')
        {
            return param.Substring(1, param.Length - 2);
        }

        return param;
    }
}
