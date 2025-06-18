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
using System.Linq;
using Reqnroll;

namespace Agenix.ReqnrollPlugin.Extensions;

/// <summary>
///     Provides extension methods for the StepInfo class.
/// </summary>
public static class StepInfoExtensions
{
    public static string GetFormattedParameters(this StepInfo stepInfo)
    {
        var fullText = "";

        if (stepInfo.StepInstance.MultilineTextArgument != null)
        {
            fullText = "```" + Environment.NewLine + stepInfo.StepInstance.MultilineTextArgument + Environment.NewLine +
                       "```";
        }
        // format table
        else if (stepInfo.StepInstance.TableArgument != null)
        {
            fullText = "| **" + string.Join("** | **", stepInfo.StepInstance.TableArgument.Header) + "** |";
            fullText += Environment.NewLine + "| " +
                        string.Join(" | ", stepInfo.StepInstance.TableArgument.Header.Select(c => "---")) + " |";

            fullText = stepInfo.StepInstance.TableArgument.Rows.Aggregate(fullText,
                (current, row) => current + Environment.NewLine + "| " + string.Join(" | ", row.Values) + " |");
        }

        return fullText;
    }

    /// <summary>
    ///     Retrieves a textual caption for the given step information by combining the step definition keyword and the step
    ///     text.
    /// </summary>
    /// <param name="stepInfo">
    ///     The step information containing details such as the step keyword and the descriptive text.
    /// </param>
    /// <returns>
    ///     A string representing the caption for the step, which includes the step definition keyword followed by the
    ///     descriptive text associated with the step.
    /// </returns>
    public static string GetCaption(this StepInfo stepInfo)
    {
        var caption = stepInfo.StepInstance.StepDefinitionKeyword + " " + stepInfo.StepInstance.Text;

        return caption;
    }
}
