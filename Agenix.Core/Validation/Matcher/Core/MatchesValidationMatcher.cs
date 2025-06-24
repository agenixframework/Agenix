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
using System.ComponentModel;
using System.Text.RegularExpressions;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;

namespace Agenix.Core.Validation.Matcher.Core;

/// Represents a validation matcher that uses a regular expression pattern
/// to validate string values against a control regex pattern.
/// </summary>
/// <remarks>
///     This class implements the <see cref="IValidationMatcher" /> interface and
///     uses the <c>Regex.IsMatch</c> method to determine if a given input matches
///     a provided regular expression pattern. If the pattern is invalid or the
///     input does not conform to the pattern, validation exceptions are thrown.
/// </remarks>
/// <exception cref="ValidationException">
///     Thrown when the input does not match the control regex pattern or if the pattern syntax is invalid.
/// </exception>
public class MatchesValidationMatcher : IValidationMatcher
{
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        var control = controlParameters[0];
        bool success;

        try
        {
            success = Regex.IsMatch(value, control);
        }
        catch (Exception e)
        {
            throw new ValidationException(TypeDescriptor.GetClassName(typeof(MatchesValidationMatcher))
                                          + " failed for field '" + fieldName
                                          + "'. Found invalid pattern syntax", e);
        }

        if (!success)
        {
            throw new ValidationException(TypeDescriptor.GetClassName(typeof(MatchesValidationMatcher))
                                          + " failed for field '" + fieldName
                                          + "'. Received value is '" + value
                                          + "', control value is '" + control + "'");
        }
    }
}
