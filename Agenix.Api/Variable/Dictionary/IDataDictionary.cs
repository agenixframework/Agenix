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

using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Core;

namespace Agenix.Api.Variable.Dictionary;

public interface IDataDictionary : IMessageProcessor

{
    // Common non-generic methods
}

/// <summary>
///     Data dictionary interface describes a mechanism to modify message content (payload) with global dictionary
///     elements.
///     Dictionary translates element values to those defined in the dictionary.
///     <para>
///         Dictionary takes part in message construction for inbound and outbound messages in agenix.
///     </para>
/// </summary>
/// <typeparam name="T">The type of the key element</typeparam>
/// <since>1.4</since>
public interface IDataDictionary<T> : IMessageProcessor, IMessageDirectionAware, IScoped, InitializingPhase,
    IDataDictionary
{
    /// <summary>
    ///     Gets the data dictionary name.
    /// </summary>
    /// <returns>The dictionary name</returns>
    string Name { get; }

    /// <summary>
    ///     Gets the path mapping strategy.
    /// </summary>
    PathMappingStrategy PathMappingStrategy { get; }

    /// <summary>
    ///     Translate value with given path in message content.
    /// </summary>
    /// <typeparam name="R">The type of the value to translate</typeparam>
    /// <param name="key">The key element in message content</param>
    /// <param name="value">Current value</param>
    /// <param name="context">The current test context</param>
    /// <returns>Translated value</returns>
    R Translate<R>(T key, R value, TestContext context);
}

/// <summary>
///     Possible mapping strategies for identifying matching dictionary items
///     with path comparison.
/// </summary>
public enum PathMappingStrategy
{
    EXACT,
    ENDS_WITH,
    STARTS_WITH
}
