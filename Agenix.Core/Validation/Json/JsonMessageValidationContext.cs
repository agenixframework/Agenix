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

using Agenix.Api.Validation.Context;

namespace Agenix.Core.Validation.Json;

/// <summary>
///     Validation context holding JSON specific validation information.
/// </summary>
public class JsonMessageValidationContext : DefaultMessageValidationContext, IValidationContext
{
    // Default constructor
    public JsonMessageValidationContext() : this(new Builder())
    {
    }

    /// Represents the validation context specific to JSON messages,
    /// extending the behavior and properties from the default message validation context.
    /// /
    public JsonMessageValidationContext(Builder builder) : base(new DefaultMessageValidationContext.Builder()
        .Ignore(builder.IgnoreExpressions)
        .Schema(builder._schema)
        .SchemaRepository(builder._schemaRepository)
        .SchemaValidation(builder._schemaValidation))
    {
    }

    /// Indicates whether a validation context requires a validator for processing.
    /// This property overrides the default behavior defined in the IValidationContext interface,
    /// explicitly returning true to signify that the context requires validation.
    bool IValidationContext.RequiresValidator => true;

    public new sealed class Builder : IMessageValidationContext.Builder<JsonMessageValidationContext, Builder>
    {
        /// <summary>
        ///     Creates a new instance of JsonMessageValidationContext using the builder's current state.
        /// </summary>
        /// <returns>A JsonMessageValidationContext instance.</returns>
        public override JsonMessageValidationContext Build()
        {
            return new JsonMessageValidationContext(this);
        }

        /// <summary>
        ///     Creates a new instance of JsonMessageValidationContext.Builder.
        /// </summary>
        /// <returns>A builder for creating a JsonMessageValidationContext.</returns>
        public static Builder Json()
        {
            return new Builder();
        }

        /// <summary>
        ///     Adds a JSON path expression to the validation context builder.
        /// </summary>
        /// <returns>A new instance of JsonPathMessageValidationContext.Builder initialized for JSON path message validation.</returns>
        public JsonPathMessageValidationContext.Builder Expressions()
        {
            return new JsonPathMessageValidationContext.Builder();
        }

        /// <summary>
        ///     Adds a JSON path expression to the validation context.
        /// </summary>
        /// <param name="path">The JSON path expression.</param>
        /// <param name="expectedValue">The expected value at the given JSON path.</param>
        /// <returns>A builder for chaining method calls.</returns>
        public JsonPathMessageValidationContext.Builder Expression(string path, object expectedValue)
        {
            return new JsonPathMessageValidationContext.Builder().Expression(path, expectedValue);
        }
    }
}
