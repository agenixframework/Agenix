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

using Agenix.Api.Condition;
using Agenix.Api.Context;

namespace Agenix.Core.Condition;

/// <summary>
///     Represents an abstract base class for defining specific conditions.
///     This class provides an implementation of the ICondition interface,
///     managing the name of the condition and requiring derived classes to
///     implement the core condition functionalities.
/// </summary>
public abstract class AbstractCondition : ICondition
{
    /// The condition name
    private readonly string _name;

    /// Represents an abstract condition that serves as a base class for specific condition implementations.
    /// Implements the ICondition interface and provides functionality for condition name management.
    /// /
    public AbstractCondition()
    {
        _name = GetType().Name;
    }

    /// Represents an abstract condition that implements the ICondition interface.
    /// Provides basic functionality for retrieving the name of the condition.
    /// /
    public AbstractCondition(string name)
    {
        _name = name;
    }

    public string GetName()
    {
        return _name;
    }

    public abstract bool IsSatisfied(TestContext context);
    public abstract string GetSuccessMessage(TestContext context);
    public abstract string GetErrorMessage(TestContext context);
}
