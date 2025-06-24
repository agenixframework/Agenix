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
                    else if (parameterString.Contains(processed + "," +
                                                      parameterList[next])) // Fixed: removed duplicate
                    {
                        processed += "," + parameterList[next];
                    }
                    else if (parameterString.Contains(processed + " , " + parameterList[next]))
                    {
                        processed += " , " + parameterList[next];
                    }
                    else
                    {
                        processed += "," + parameterList[next]; // Fixed: added comma in fallback
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
