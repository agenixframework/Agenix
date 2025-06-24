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

namespace Agenix.Api.Spi;

/// Used to bind an object to the Agenix context reference registry for dependency injection reasons.
/// Object is bound with given name. In case no explicit name is given the registry will auto compute the name from given
/// Class name, method name or field name.
/// /
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method)]
public class BindToRegistryAttribute : Attribute
{
    private string _name;

    /// Gets or sets the name used to bind the object to the Agenix context reference registry.
    /// If not explicitly set, the registry will auto compute the name from the class name, method name, or field name.
    public string Name
    {
        get => _name ?? string.Empty;
        set => _name = value;
    }
}
