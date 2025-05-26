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