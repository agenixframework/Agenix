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
using Agenix.Api.Exceptions;
using Agenix.Api.Util;
using Agenix.Api.Variable;
using Agenix.Core.Validation.Json;

namespace Agenix.Validation.Json.Json;

/// <summary>
///     Represents an implementation of a segment variable extractor that validates and evaluates JSON path expressions
///     to extract values from JSON strings. Inherits the properties and behavior of the abstract base class
///     <see cref="SegmentVariableExtractorRegistry.AbstractSegmentVariableExtractor" />.
/// </summary>
public class JsonPathSegmentVariableExtractor : SegmentVariableExtractorRegistry.AbstractSegmentVariableExtractor
{
    /// <summary>
    ///     Determines whether the value can be extracted from the given object based on a JSONPath expression.
    /// </summary>
    /// <param name="testContext">
    ///     Represents the context of the current test execution, providing access to necessary services
    ///     and state.
    /// </param>
    /// <param name="obj">The input object to check. If null, the method returns true.</param>
    /// <param name="matcher">Contains the JSONPath expression which is matched against the object to validate extractability.</param>
    /// <returns>
    ///     True if the object is null, a valid JSON string, and the specified JSONPath expression is valid; otherwise,
    ///     false.
    /// </returns>
    public override bool CanExtract(TestContext testContext, object? obj, VariableExpressionSegmentMatcher matcher)
    {
        return obj == null || (obj is string jsonPath && IsJsonPredicate.Instance.Test(jsonPath) &&
                               JsonPathMessageValidationContext.IsJsonPathExpression(matcher.SegmentExpression));
    }

    /// <summary>
    ///     Extracts a value from a given object based on a JSONPath expression defined in the matcher.
    /// </summary>
    /// <param name="testContext">
    ///     Represents the context of the current test execution, providing access to various services
    ///     and state.
    /// </param>
    /// <param name="obj">The input object from which the value is to be extracted. If null, the result will also be null.</param>
    /// <param name="matcher">
    ///     Contains the JSONPath expression used to identify and extract the desired portion from the
    ///     object.
    /// </param>
    /// <returns>
    ///     The extracted value as an object, based on the JSONPath expression provided in the matcher. If the object is
    ///     null, returns null.
    /// </returns>
    protected override object DoExtractValue(TestContext testContext, object? obj,
        VariableExpressionSegmentMatcher matcher)
    {
        return obj == null ? null : ExtractJsonPath(obj.ToString(), matcher.SegmentExpression);
    }

    /// <summary>
    ///     Extracts a specific portion of a JSON payload based on a provided JSONPath expression.
    /// </summary>
    /// <param name="json">A string containing the JSON payload from which the extraction is performed.</param>
    /// <param name="segmentExpression">The JSONPath expression specifying the portion of the JSON to extract.</param>
    /// <returns>An object containing the result of the JSONPath evaluation. If the extraction fails, an exception is thrown.</returns>
    /// <exception cref="AgenixSystemException">
    ///     Thrown when the JSONPath extraction fails, such as in cases of malformed JSON or invalid JSONPath expressions.
    /// </exception>
    private static object ExtractJsonPath(string json, string segmentExpression)
    {
        try
        {
            return JsonPathUtils.EvaluateAsString(json, segmentExpression);
        }
        catch (AgenixSystemException e)
        {
            throw new AgenixSystemException($"Unable to extract jsonPath from segmentExpression {segmentExpression}",
                e);
        }
    }
}
