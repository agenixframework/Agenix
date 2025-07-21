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
using System.Text;
using System.Web;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Decodes URL encoded string to a character sequence.
/// </summary>
public class UrlDecodeFunction : IFunction
{
    /// <summary>
    ///     Executes the URL decode function.
    /// </summary>
    /// <param name="parameterList">List of parameters: [0] - string to decode, [1] - charset (optional, defaults to UTF-8)</param>
    /// <param name="testContext">Test context</param>
    /// <returns>URL decoded string</returns>
    /// <exception cref="InvalidFunctionUsageException">Thrown when parameters are null or empty</exception>
    /// <exception cref="AgenixSystemException">Thrown when character encoding is not supported</exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Invalid function parameter usage! Missing parameters!");
        }

        var charset = "UTF-8";
        if (parameterList.Count > 1)
        {
            charset = parameterList[1];
        }

        try
        {
            var encoding = Encoding.GetEncoding(charset);
            return HttpUtility.UrlDecode(parameterList[0], encoding);
        }
        catch (ArgumentException e)
        {
            throw new AgenixSystemException("Unsupported character encoding", e);
        }
    }
}
