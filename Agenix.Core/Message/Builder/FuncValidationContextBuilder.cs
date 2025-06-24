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
using Agenix.Api;
using Agenix.Api.Validation.Context;

namespace Agenix.Core.Message.Builder;

/// <summary>
///     A builder class for creating instances of <see cref="IValidationContext" /> using a provided factory function.
/// </summary>
/// <typeparam name="TBuilder">
///     The type of the builder implementing the <see cref="IValidationContext.IBuilder{T, TB}" />
///     interface.
/// </typeparam>
public class FuncValidationContextBuilder<TBuilder>(Func<IValidationContext> validationContextFactory)
    : IValidationContext.IBuilder<IValidationContext, TBuilder>
    where TBuilder : IBuilder
{
    /// <summary>
    ///     Builds and returns an instance of <see cref="IValidationContext" />.
    /// </summary>
    /// <returns>An instance of <see cref="IValidationContext" /> created by the factory.</returns>
    public IValidationContext Build()
    {
        return validationContextFactory();
    }
}
