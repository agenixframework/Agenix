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
using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using static System.Console;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function returning the actual date as formatted string value. User specifies format string as argument.
///     TODO: Function has also to support additional date offset in order to manipulate result date value. E.g.
///     agenix:CurrentDate('yyyy-MM-dd', '+1y') -> current date + one year
/// </summary>
public class CurrentDateFunction : IFunction
{
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            return GetDefaultCurrentDate();
        }

        try
        {
            return parameterList[0].Equals("") ? GetDefaultCurrentDate() : DateTime.Now.ToString(parameterList[0]);
        }
        catch (Exception e)
        {
            WriteLine("Error while formatting data value {0}", e);
            throw new AgenixSystemException(e.Message);
        }
    }

    private static string GetDefaultCurrentDate()
    {
        return DateTime.Now.ToString("dd.MM.yyyy");
    }
}
