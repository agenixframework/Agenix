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
using Agenix.Api.Log;
using Agenix.Api.Validation.Matcher;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Validation.Matcher.Core;

/// <summary>
///     The <c>CreateVariableValidationMatcher</c> class is responsible for validating and setting variable values
///     within a given <see cref="TestContext" /> during the validation process.
/// </summary>
/// <remarks>
///     This class implements the <see cref="IValidationMatcher" /> interface and provides functionality
///     for setting variables based on the provided field name and value. Optionally, a list of control
///     parameters can be used to customize the variable name.
/// </remarks>
/// <example>
///     This class is designed to integrate with the validation framework and is not used directly,
///     but rather utilized during validation workflows to handle specific variable mappings.
/// </example>
/// <seealso cref="IValidationMatcher" />
public class CreateVariableValidationMatcher : IValidationMatcher
{
    /// <summary>Logger</summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(CreateVariableValidationMatcher));

    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        var name = fieldName;

        if (controlParameters != null && controlParameters.Count > 0)
        {
            name = controlParameters[0];
        }

        Log.LogDebug("Setting variable: {Name} to value: {Value}", name, value);

        context.SetVariable(name, value);
    }
}
