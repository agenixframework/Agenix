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

namespace Agenix.Configuration.Core.Models;

/// <summary>
/// Supported configuration file formats.
/// </summary>
public enum ConfigurationFormat
{
    /// <summary>
    /// JSON configuration format
    /// </summary>
    JSON,

    /// <summary>
    /// YAML configuration format
    /// </summary>
    YAML
}

/// <summary>
/// Configuration options for the Agenix Configuration Manager.
/// </summary>
public class ConfigurationOptions
{
    /// <summary>
    /// Gets or sets the base name for configuration files (default: "appsettings").
    /// </summary>
    public string ConfigurationName { get; set; } = "appsettings";

    /// <summary>
    /// Gets or sets the directory where configuration files are located (default: "./").
    /// </summary>
    public string ConfigurationDirectory { get; set; } = "./";

    /// <summary>
    /// Gets or sets the configuration file format (default: Json).
    /// </summary>
    public ConfigurationFormat Format { get; set; } = ConfigurationFormat.JSON;

    /// <summary>
    /// Gets or sets whether environment file support is enabled (default: false).
    /// </summary>
    public bool EnvironmentFileSupport { get; set; } = false;

    /// <summary>
    /// Gets or sets the directory where environment files are located (default: "./").
    /// </summary>
    public string EnvironmentFileDirectory { get; set; } = "./";

    /// <summary>
    /// Gets or sets the name of the environment file (default: ".env").
    /// </summary>
    public string EnvironmentFileName { get; set; } = ".env";

    /// <summary>
    /// Gets or sets the default environment name (default: "dev").
    /// </summary>
    public string DefaultEnvironment { get; set; } = "dev";

    /// <summary>
    /// Gets or sets whether caching is enabled (default: false).
    /// </summary>
    public bool CachingEnabled { get; set; } = false;

    /// <summary>
    /// Gets the file extension based on the configured format.
    /// </summary>
    public string FileExtension => Format switch
    {
        ConfigurationFormat.JSON => ".json",
        ConfigurationFormat.YAML => ".yml",
        _ => ".json"
    };
}
