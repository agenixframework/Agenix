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

namespace Agenix.Api.Functions;

/// <summary>
///     Helper class parsing a parameter string and converting the tokens to a parameter list.
/// </summary>
public static class FunctionParameterHelper
{
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
            var processed = parameterList[i];

            if (IsStartOfQuotedParameter(processed))
            {
                var reconstructedParameter = ReconstructQuotedParameter(parameterString, parameterList, ref i);
                postProcessedList.Add(CutOffSingleQuotes(reconstructedParameter));
            }
            else
            {
                postProcessedList.Add(processed);
            }
        }

        return postProcessedList;
    }

    private static bool IsStartOfQuotedParameter(string parameter)
    {
        return parameter.StartsWith("'") && !parameter.EndsWith("'");
    }

    private static string ReconstructQuotedParameter(string originalString, IList<string> parameterList,
        ref int currentIndex)
    {
        var processed = parameterList[currentIndex];
        var nextIndex = currentIndex + 1;

        while (nextIndex < parameterList.Count)
        {
            var nextParameter = parameterList[nextIndex];
            processed = AppendParameterWithOriginalSeparator(originalString, processed, nextParameter);

            currentIndex++;
            if (nextParameter.EndsWith("'"))
            {
                break;
            }

            nextIndex++;
        }

        return processed;
    }

    private static string AppendParameterWithOriginalSeparator(string originalString, string processed,
        string nextParameter)
    {
        var separators = new[] { ", ", ",", " , " };

        foreach (var separator in separators)
        {
            var candidateString = processed + separator + nextParameter;
            if (originalString.Contains(candidateString))
            {
                return candidateString;
            }
        }

        return processed + "," + nextParameter;
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
