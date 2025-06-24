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

namespace Agenix.Screenplay.Formatting;

/// <summary>
///     A utility class for removing redundant terms from an expression.
/// </summary>
/// <remarks>
///     This class provides a static method to streamline and clean up expressions
///     by stripping predefined redundant prefixes. It is particularly tailored to
///     identify the common terms that may add unnecessary verbosity to text.
/// </remarks>
public class StripRedundantTerms
{
    private static readonly IReadOnlyList<string> RedundantHamcrestPrefixes =
        ["is ", "be ", "should be"];

    public static string From(string expression)
    {
        return RedundantHamcrestPrefixes.Aggregate(expression, RemovePrefix);
    }

    private static string RemovePrefix(string expression, string prefix)
    {
        return expression.StartsWith(prefix)
            ? expression.Substring(prefix.Length)
            : expression;
    }
}
