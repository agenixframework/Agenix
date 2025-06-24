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

namespace Agenix.Api.Validation.Matcher;

/// <summary>
///     Control expression parser for extracting the individual control values from a control expression.
/// </summary>
public interface IControlExpressionParser
{
    /// <summary>
    ///     Some validation matchers can optionally contain one or more control values nested within a control expression
    ///     Matchers implementing this interface should take care of extracting the individual control values from the
    ///     expression. <br />
    ///     For example, expects between 2 and 3 control values(dateFrom, dateTo and optionally datePattern) to be
    ///     provided.It's
    ///     ControlExpressionParser would be expected to parse the control expression, returning each individual
    ///     control///value for each nested parameter found within the control expression.
    /// </summary>
    /// <param name="controlExpression">the control expression to be parsed</param>
    /// <param name="delimiter">the delimiter to use. When {@literal NULL} the {@link #DEFAULT_DELIMITER} is assumed.</param>
    /// <returns></returns>
    List<string> ExtractControlValues(string controlExpression, char delimiter);
}
