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

/// Represents a message validation context, combining validation functionality
/// with schema validation capabilities and supporting the management of
/// ignored message elements.
public interface IMessageValidationContext : IValidationContext, ISchemaValidationContext
{
    /// Retrieves the ignored message elements.
    /// @return a set of ignored expressions
    /// /
    HashSet<string> IgnoreExpressions { get; }

    /// Provides a builder for constructing instances of message validation contexts.
    /// This abstract class is designed to be extended by concrete implementations
    /// and provides methods for configuring schema validation, schema repositories,
    /// and ignored message elements.
    public abstract class Builder<T, S> : IBuilder<T, Builder<T, S>>,
        IBuilder<IValidationContext, IBuilder>,
        IBuilder<Builder<T, S>>,
        IBuilder
        where T : IMessageValidationContext
        where S : Builder<T, S>
    {
        public readonly HashSet<string> IgnoreExpressions = [];
        protected readonly S Self;
        public string _schema;
        public string _schemaRepository;
        public bool _schemaValidation = true;

        protected Builder()
        {
            Self = (S)this;
        }

        /// Enables or disables schema validation for the message.
        /// <param name="enabled">
        ///     A boolean value indicating whether schema validation should be enabled (true) or disabled
        ///     (false).
        /// </param>
        /// <return>Returns the current builder instance to allow method chaining.</return>
        public Builder<T, S> SchemaValidation(bool enabled)
        {
            _schemaValidation = enabled;
            return Self;
        }

        /// Sets an explicit schema instance name to use for schema validation.
        /// <param name="schemaName">The name of the schema to be used for validation.</param>
        /// <return>Returns the current builder instance to allow method chaining.</return>
        public Builder<T, S> Schema(string schemaName)
        {
            _schema = schemaName;
            return Self;
        }

        /// Sets the schema repository to be used for schema validation.
        /// <param name="schemaRepository">The name or path of the schema repository to use.</param>
        /// <return>Returns the current builder instance to allow method chaining.</return>
        public Builder<T, S> SchemaRepository(string schemaRepository)
        {
            _schemaRepository = schemaRepository;
            return Self;
        }

        IValidationContext IBuilder<IValidationContext, IBuilder>.Build()
        {
            return Build();
        }


        public abstract T Build();

        /// Adds an ignored path expression for a specific message element.
        /// <param name="path">The path expression of the element to be ignored.</param>
        /// <return>Returns the current builder instance to allow method chaining.</return>
        public S Ignore(string path)
        {
            IgnoreExpressions.Add(path);
            return Self;
        }

        /// Adds a set of ignored path expressions for message elements.
        /// <param name="paths">A set of path expressions representing the elements to be ignored in the validation context.</param>
        /// <return>Returns the current builder instance to allow method chaining.</return>
        public S Ignore(ISet<string> paths)
        {
            IgnoreExpressions.UnionWith(paths);
            return Self;
        }
    }
}
