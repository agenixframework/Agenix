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

namespace Agenix.Api.Validation.Context;

/// Represents a default implementation for the message validation context.
/// This context provides facilities to manage schema validation settings
/// and a collection of expressions to be ignored during validation.
/// Includes a dedicated Builder class for constructing instances with
/// specific configurations, allowing for fluent-style setup of schema
/// validation and ignore expressions.
/// Implements IMessageValidationContext interface, inheriting its schema
/// validation and overall validation context capabilities.
public class DefaultMessageValidationContext(
    IMessageValidationContext.Builder<IMessageValidationContext, DefaultMessageValidationContext.Builder> builder)
    : DefaultValidationContext, IMessageValidationContext
{
    /// <summary>
    ///     A collection of XPath or JSON expressions used to specify message elements to be excluded from validation.
    /// </summary>
    private readonly HashSet<string> _ignoreExpressions = builder.IgnoreExpressions;

    /// <summary>
    ///     Explicit schema instance to use for this validation
    /// </summary>
    private readonly string _schema = builder._schema;

    /// <summary>
    ///     Explicit schema repository to use for this validation
    /// </summary>
    private readonly string _schemaRepository = builder._schemaRepository;

    /// <summary>
    ///     Should a message be validated with its schema definition?
    /// </summary>
    private readonly bool _schemaValidationEnabled = builder._schemaValidation;

    public DefaultMessageValidationContext() : this(new Builder())
    {
    }

    /// Gets the set of XPath expressions used to identify message elements
    /// that should be ignored during validation.
    public HashSet<string> IgnoreExpressions => _ignoreExpressions;

    /// Determines whether schema validation is enabled for the context.
    public bool IsSchemaValidationEnabled => _schemaValidationEnabled;

    /// Represents the location or source of the schema used for validation
    /// within the message validation context. This property provides the
    /// necessary reference to identify or retrieve the schema required
    /// for validating message structures.
    public string SchemaRepository => _schemaRepository;

    /// Represents the schema associated with the validation context, typically used
    /// for validation against a predefined structure or set of rules.
    public string Schema => _schema;

    /// Fluent builder for constructing instances of DefaultMessageValidationContext,
    /// allowing the configuration of schema validation properties and ignored expressions.
    /// /
    public sealed class Builder : IMessageValidationContext.Builder<IMessageValidationContext, Builder>
    {
        public override IMessageValidationContext Build()
        {
            return new DefaultMessageValidationContext(this);
        }
    }
}
