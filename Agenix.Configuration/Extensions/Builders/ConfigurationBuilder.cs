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
/// Builder implementation for creating configuration managers with fluent API.
/// </summary>
/// <typeparam name="T">The type of the configuration object</typeparam>
public class ConfigurationBuilder<T> : IConfigurationBuilder<T> where T : class, new()
{
    private readonly ConfigurationOptions _options = new();

    /// <inheritdoc />
    public IConfigurationBuilder<T> WithConfigurationName(string name)
    {
        _options.ConfigurationName = name ?? throw new ArgumentNullException(nameof(name));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationBuilder<T> WithConfigurationDirectory(string directory)
    {
        _options.ConfigurationDirectory = directory ?? throw new ArgumentNullException(nameof(directory));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationBuilder<T> WithFormat(ConfigurationFormat format)
    {
        _options.Format = format;
        return this;
    }

    /// <inheritdoc />
    public IConfigurationBuilder<T> WithEnvironmentFileSupport(bool enabled)
    {
        _options.EnvironmentFileSupport = enabled;
        return this;
    }

    /// <inheritdoc />
    public IConfigurationBuilder<T> WithEnvironmentFileDirectory(string directory)
    {
        _options.EnvironmentFileDirectory = directory ?? throw new ArgumentNullException(nameof(directory));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationBuilder<T> WithEnvironmentFileName(string fileName)
    {
        _options.EnvironmentFileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationBuilder<T> WithDefaultEnvironment(string environment)
    {
        _options.DefaultEnvironment = environment ?? throw new ArgumentNullException(nameof(environment));
        return this;
    }

    /// <inheritdoc />
    public IConfigurationBuilder<T> WithCaching(bool enabled)
    {
        _options.CachingEnabled = enabled;
        return this;
    }

    /// <inheritdoc />
    public IConfigurationManager<T> Build()
    {
        return new ConfigurationManager<T>(_options);
    }
}
