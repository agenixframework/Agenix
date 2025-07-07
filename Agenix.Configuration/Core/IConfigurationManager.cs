
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

namespace Agenix.Configuration.Core;

/// <summary>
/// Interface for configuration management with environment-specific configuration loading.
/// </summary>
/// <typeparam name="T">The type of the configuration object</typeparam>
public interface IConfigurationManager<T> where T : class, new()
{
    /// <summary>
    /// Gets a value indicating whether configuration caching is enabled.
    /// </summary>
    bool IsConfigurationCached { get; }

    /// <summary>
    /// Gets the name of the current environment being used.
    /// </summary>
    string? CurrentEnvironment { get; }

    /// <summary>
    /// Gets the configuration for the current environment.
    /// </summary>
    /// <returns>The configuration object for the current environment</returns>
    T GetConfiguration();

    /// <summary>
    /// Gets the configuration for a specific environment.
    /// </summary>
    /// <param name="environment">The environment name (e.g., "Development", "Production")</param>
    /// <returns>The configuration object for the specified environment</returns>
    T GetConfiguration(string environment);

    /// <summary>
    /// Asynchronously gets the configuration for the current environment.
    /// </summary>
    /// <returns>A task containing the configuration object for the current environment</returns>
    Task<T> GetConfigurationAsync();

    /// <summary>
    /// Asynchronously gets the configuration for a specific environment.
    /// </summary>
    /// <param name="environment">The environment name (e.g., "Development", "Production")</param>
    /// <returns>A task containing the configuration object for the specified environment</returns>
    Task<T> GetConfigurationAsync(string environment);

    /// <summary>
    /// Reloads the configuration for the current environment, clearing any cached values.
    /// </summary>
    void ReloadConfiguration();

    /// <summary>
    /// Reloads the configuration for a specific environment, clearing any cached values.
    /// </summary>
    /// <param name="environment">The environment name to reload</param>
    void ReloadConfiguration(string environment);
}
