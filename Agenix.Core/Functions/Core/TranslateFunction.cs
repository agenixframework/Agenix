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

using System.Collections.Generic;
using System.Text.RegularExpressions;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function searches for occurrences of a given character sequence and replaces all findings with a given replacement
///     string.
/// </summary>
public class TranslateFunction : IFunction
{
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count < 3)
        {
            throw new InvalidFunctionUsageException("Function parameters not set correctly");
        }

        var resultString = parameterList[0];

        string regex = null;
        string replacement = null;

        if (parameterList.Count > 1)
        {
            regex = parameterList[1];
        }

        if (parameterList.Count > 2)
        {
            replacement = parameterList[2];
        }

        if (regex != null && replacement != null)
        {
            resultString = Regex.Replace(resultString, regex, replacement);
        }

        return resultString;
    }
}
