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

using Agenix.Configuration.Core;
using Agenix.Configuration.Core.Models;

namespace Agenix.Configuration.Extensions.Builders;

/// <summary>
/// Builder interface for creating configuration managers with fluent API.
/// </summary>
/// <typeparam name="T">The type of the configuration object</typeparam>
public interface IConfigurationBuilder<T> where T : class, new()
{
    /// <summary>
    /// Sets the base name for configuration files.
    /// </summary>
    /// <param name="name">The configuration name (e.g., "appsettings")</param>
    /// <returns>The builder instance for method chaining</returns>
    IConfigurationBuilder<T> WithConfigurationName(string name);

    /// <summary>
    /// Sets the directory where configuration files are located.
    /// </summary>
    /// <param name="directory">The configuration directory path</param>
    /// <returns>The builder instance for method chaining</returns>
    IConfigurationBuilder<T> WithConfigurationDirectory(string directory);

    /// <summary>
    /// Sets the configuration file format (JSON or YAML).
    /// </summary>
    /// <param name="format">The configuration format to use</param>
    /// <returns>The builder instance for method chaining</returns>
    IConfigurationBuilder<T> WithFormat(ConfigurationFormat format);

    /// <summary>
    /// Enables or disables environment file support.
    /// </summary>
    /// <param name="enabled">True to enable environment file support</param>
    /// <returns>The builder instance for method chaining</returns>
    IConfigurationBuilder<T> WithEnvironmentFileSupport(bool enabled);

    /// <summary>
    /// Sets the directory where environment files are located.
    /// </summary>
    /// <param name="directory">The environment file directory path</param>
    /// <returns>The builder instance for method chaining</returns>
    IConfigurationBuilder<T> WithEnvironmentFileDirectory(string directory);

    /// <summary>
    /// Sets the name of the environment file.
    /// </summary>
    /// <param name="fileName">The environment file name (e.g., ".env")</param>
    /// <returns>The builder instance for method chaining</returns>
    IConfigurationBuilder<T> WithEnvironmentFileName(string fileName);

    /// <summary>
    /// Sets the default environment name.
    /// </summary>
    /// <param name="environment">The default environment name</param>
    /// <returns>The builder instance for method chaining</returns>
    IConfigurationBuilder<T> WithDefaultEnvironment(string environment);

    /// <summary>
    /// Enables or disables configuration caching.
    /// </summary>
    /// <param name="enabled">True to enable caching</param>
    /// <returns>The builder instance for method chaining</returns>
    IConfigurationBuilder<T> WithCaching(bool enabled);

    /// <summary>
    /// Builds and returns the configuration manager instance.
    /// </summary>
    /// <returns>A configured IConfigurationManager instance</returns>
    IConfigurationManager<T> Build();
}
