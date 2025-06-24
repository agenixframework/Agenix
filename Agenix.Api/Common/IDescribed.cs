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

namespace Agenix.Api.Common;

/// <summary>
///     Provides a contract for objects that have a description.
///     The description is intended to give additional context or details about the implementing object.
/// </summary>
public interface IDescribed
{
    /// Retrieves the description associated with this instance.
    /// @return The description as a string.
    /// /
    public string GetDescription()
    {
        return "";
    }

    /// Sets the description for this instance.
    /// <param name="description">The description to be set.</param>
    /// <return>The current instance with the updated description.</return>
    ITestAction SetDescription(string description);
}
