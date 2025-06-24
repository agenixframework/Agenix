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

namespace Agenix.Api.Variable;

/// Represents a collection of global variables accessible in each test case.
/// Allows for managing and storing variables using a builder pattern.
/// /
public class GlobalVariables
{
    /// Stores a collection of key-value pairs representing global variables.
    /// /
    private readonly Dictionary<string, object> _variables;

    public GlobalVariables() : this(new Builder())
    {
    }

    public GlobalVariables(Builder builder)
    {
        _variables = builder.Variables;
    }

    /// Get the global variables.
    /// @return the global variables as a dictionary of key-value pairs.
    /// /
    public Dictionary<string, object> GetVariables()
    {
        return _variables;
    }

    /// Provides a builder for creating and managing global variables.
    /// Supports a fluent interface for defining variables and collections of variables.
    /// Used in conjunction with the GlobalVariables class to construct instances with specified configurations.
    public class Builder
    {
        public Dictionary<string, object> Variables { get; } = [];

        public Builder WithVariable(string name, object value)
        {
            Variables[name] = value;
            return this;
        }

        public Builder WithVariables(Dictionary<string, object> variables)
        {
            foreach (var item in variables)
            {
                Variables[item.Key] = item.Value;
            }

            return this;
        }

        public GlobalVariables Build()
        {
            return new GlobalVariables(this);
        }
    }
}
