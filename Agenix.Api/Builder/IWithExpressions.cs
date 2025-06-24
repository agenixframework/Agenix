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

namespace Agenix.Api.Builder;

/// <summary>
///     Provides a fluent interface for defining and managing expressions to be evaluated.
///     Implementations of this interface allow the configuration of expressions and their corresponding
///     target variable names for storing evaluation results in a context.
/// </summary>
/// <typeparam name="TB">
///     The type that implements this interface, enabling method chaining in a fluent API style.
/// </typeparam>
public interface IWithExpressions<out TB>
{
    /// <summary>
    ///     Sets the expressions to evaluate. Keys are expressions that should be evaluated, and values are target
    ///     variable names that are stored in the test context with the evaluated result as a variable value.
    /// </summary>
    /// <param name="expressions">A dictionary of expressions and their corresponding target variable names</param>
    /// <returns>The type that implements this interface</returns>
    TB Expressions(IDictionary<string, object> expressions);

    /// <summary>
    ///     Adds an expression that gets evaluated. The evaluation result is stored in the test context as a variable
    ///     with the given variable name.
    /// </summary>
    /// <param name="expression">The expression to be evaluated</param>
    /// <param name="value">The target variable name</param>
    /// <returns>The type that implements this interface</returns>
    TB Expression(string expression, object value);
}
