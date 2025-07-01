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

using Agenix.Configuration.Core.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace Agenix.Configuration.Extensions;

/// <summary>
/// Extension methods for adding YAML configuration sources.
/// </summary>
public static class YamlConfigurationExtensions
{
    /// <summary>
    /// Adds a YAML configuration source to the configuration builder.
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="path">Path to the YAML file</param>
    /// <returns>The configuration builder</returns>
    public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, string path)
    {
        return AddYamlFile(builder, provider: null, path: path, optional: false, reloadOnChange: false);
    }

    /// <summary>
    /// Adds a YAML configuration source to the configuration builder.
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="path">Path to the YAML file</param>
    /// <param name="optional">Whether the file is optional</param>
    /// <returns>The configuration builder</returns>
    public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, string path, bool optional)
    {
        return AddYamlFile(builder, provider: null, path: path, optional: optional, reloadOnChange: false);
    }

    /// <summary>
    /// Adds a YAML configuration source to the configuration builder.
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="path">Path to the YAML file</param>
    /// <param name="optional">Whether the file is optional</param>
    /// <param name="reloadOnChange">Whether to reload when the file changes</param>
    /// <returns>The configuration builder</returns>
    public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
    {
        return AddYamlFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
    }

    /// <summary>
    /// Adds a YAML configuration source to the configuration builder.
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="provider">The file provider</param>
    /// <param name="path">Path to the YAML file</param>
    /// <param name="optional">Whether the file is optional</param>
    /// <param name="reloadOnChange">Whether to reload when the file changes</param>
    /// <returns>The configuration builder</returns>
    public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, IFileProvider? provider, string path, bool optional, bool reloadOnChange)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));
        if (string.IsNullOrEmpty(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        return builder.AddYamlFile(source =>
        {
            source.FileProvider = provider;
            source.Path = path;
            source.Optional = optional;
            source.ReloadOnChange = reloadOnChange;
        });
    }

    /// <summary>
    /// Adds a YAML configuration source to the configuration builder.
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="configureSource">Action to configure the YAML source</param>
    /// <returns>The configuration builder</returns>
    public static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, Action<YamlConfigurationSource> configureSource)
        => builder.Add(configureSource);
}
