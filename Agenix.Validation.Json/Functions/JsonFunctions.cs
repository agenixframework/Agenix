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

using Agenix.Api.Context;
using Agenix.Validation.Json.Functions.Core;

namespace Agenix.Validation.Json.Functions;

/// <summary>
///     The <c>JsonFunctions</c> class provides utility methods for processing JSON data.
/// </summary>
public sealed class JsonFunctions
{
    /// <summary>
    ///     The <c>JsonFunctions</c> class provides utility methods for handling and processing JSON data.
    /// </summary>
    private JsonFunctions()
    {
    }

    /// <summary>
    ///     Executes a JSON path expression on the provided JSON content within the given test context.
    /// </summary>
    /// <param name="content">The JSON content to evaluate the expression against.</param>
    /// <param name="expression">The JSON path expression to execute.</param>
    /// <param name="context">The test context in which the JSON path function is executed.</param>
    /// <returns>The result of executing the JSON path expression as a string.</returns>
    public static string JsonPath(string content, string expression, TestContext context)
    {
        return new JsonPathFunction().Execute(new List<string> { content, expression }, context);
    }
}
