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
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
///     Represents a validation matcher that ensures a given value is empty.
/// </summary>
/// <remarks>
///     The EmptyValidationMatcher class implements the IValidationMatcher interface and provides a method to validate
///     whether a specific field's value is empty.
/// </remarks>
public class EmptyValidationMatcher : IValidationMatcher
{
    /// <summary>
    ///     Validates whether the provided value for the specified field is empty.
    /// </summary>
    /// <param name="fieldName">The name of the field being validated.</param>
    /// <param name="value">The value of the field to be validated.</param>
    /// <param name="controlParameters">A list of control parameters for validation.</param>
    /// <param name="context">The test context in which validation is occurring.</param>
    /// <exception cref="ValidationException">Thrown when the value is not empty.</exception>
    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        if (value == null || !string.IsNullOrEmpty(value))
        {
            throw new ValidationException(GetType().Name
                                          + " failed for field '" + fieldName
                                          + "'. Received value '" + value
                                          + "' should be empty");
        }
    }
}
