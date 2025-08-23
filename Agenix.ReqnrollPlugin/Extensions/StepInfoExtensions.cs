#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2025 Agenix
//
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
    /// <summary>
    ///     Generates a formatted representation of the parameters associated with the step, either as a multiline text block
    ///     or a markdown-formatted table, depending on the type of arguments present in the step information.
    /// </summary>
    /// <param name="stepInfo">
    ///     The step information containing details about the step instance, including any multiline text or table arguments.
    /// </param>
    /// <returns>
    ///     A string containing the formatted representation of the step's parameters, which may include a markdown-style
    ///     representation of either a multiline string or a table.
    /// </returns>
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
                        string.Join(" | ", stepInfo.StepInstance.TableArgument.Header.Select(_ => "---")) + " |";

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
