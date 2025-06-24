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

using Agenix.Api;

namespace Agenix.Core;

/// Abstract base class for building test actions.
/// @typeparam T The type of test action being built.
/// @typeparam TS The type of the builder implementation.
/// /
public abstract class AbstractTestActionBuilder<T, TS> : ITestActionBuilder<T>
    where T : ITestAction
    where TS : class
{
    private string description;

    private string name;
    protected TS self;

    protected AbstractTestActionBuilder()
    {
        self = (TS)(object)this;
    }

    /// Builds the test action.
    /// @return The build instance of the test action.
    /// /
    public abstract T Build();

    /// Sets the test action name.
    /// <param name="name">The test action name.</param>
    /// <return>The builder instance with the updated name.</return>
    public virtual TS Name(string name)
    {
        this.name = name;
        return self;
    }

    /// Sets the description for the test action.
    /// @param description the description of the test action.
    /// @return The builder instance with the updated description.
    public virtual TS Description(string description)
    {
        this.description = description;
        return self;
    }

    /// Obtains the name of the test action.
    /// @return The name of the test action.
    public string GetName()
    {
        return name;
    }

    /// Obtains the description.
    /// @return The description of the test action.
    public string GetDescription()
    {
        return description;
    }
}
