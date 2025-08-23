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
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Decodes base64 binary data to a character sequence.
/// </summary>
public class DecodeBase64Function : IFunction
{
    /// <summary>
    ///     Decodes a base64-encoded string from the input parameters to a UTF-8 string.
    /// </summary>
    /// <param name="parameterList">
    ///     A list of parameters where the first parameter is expected to be a base64-encoded string to decode.
    /// </param>
    /// <param name="testContext">
    ///     The context of the test providing necessary execution environment information.
    /// </param>
    /// <returns>
    ///     The decoded UTF-8 string derived from the base64-encoded string.
    /// </returns>
    /// <exception cref="InvalidFunctionUsageException">
    ///     Thrown when the parameter list is null or empty.
    /// </exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Invalid function parameter usage! Missing parameters!");
        }

        var base64EncodedBytes = Convert.FromBase64String(parameterList[0]);
        return Encoding.UTF8.GetString(base64EncodedBytes);
    }
}
