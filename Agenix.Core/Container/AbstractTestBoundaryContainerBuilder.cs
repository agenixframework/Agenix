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

using System.Collections.Generic;
using System.Linq;

namespace Agenix.Core.Container;

/// <summary>
///     AbstractTestBoundaryContainerBuilder is an abstract class that facilitates building test boundary action containers
///     with specific conditions and properties. This builder pattern allows users to specify conditions on test names,
///     test groups, namespaces, system properties, and environment variables, ensuring that the before-test logic
///     runs only when these conditions match.
/// </summary>
/// <typeparam name="T">The type of the test boundary action container.</typeparam>
/// <typeparam name="S">The type of the builder class itself, to support fluent interfaces.</typeparam>
public abstract class AbstractTestBoundaryContainerBuilder<T, S> : AbstractTestContainerBuilder<T, S>
    where T : AbstractTestBoundaryActionContainer
    where S : AbstractTestBoundaryContainerBuilder<T, S>
{
    private readonly Dictionary<string, string> _env = new();
    private readonly Dictionary<string, string> _systemProperties = new();
    private readonly List<string> _testGroups = [];
    private string _namePattern;
    private string _namespaceNamePattern;

    /// Condition on test names. The before test logic will only run when this condition matches.
    /// @param namePattern a string pattern to match test names against
    /// @return the updated container builder instance
    /// /
    public S OnTests(string namePattern)
    {
        _namePattern = namePattern;
        return (S)this;
    }

    /// Condition on test group name. The before test logic will only run when this condition matches.
    /// @param testGroups vararg of test group names to match against
    /// @return the updated container builder instance
    /// /
    public S OnTestGroup(params string[] testGroups)
    {
        return OnTestGroups(testGroups.ToList());
    }

    /// Condition on test group names. The before test logic will only run when this condition matches.
    /// <param name="testGroups">A list of test group names to match against</param>
    /// <return>The updated container builder instance</return>
    public S OnTestGroups(List<string> testGroups)
    {
        _testGroups.AddRange(testGroups);
        return (S)this;
    }

    /// Condition on package names. The before test logic will only run when this condition matches.
    /// <param name="namespacePattern">A string pattern to match namespace names against</param>
    /// <return>The updated container builder instance</return>
    public S OnNamespace(string namespacePattern)
    {
        _namespaceNamePattern = namespacePattern;
        return (S)this;
    }

    /// Condition on system property with a specific value. The before test logic will only run when this condition matches.
    /// <param name="name">The name of the system property to match</param>
    /// <param name="value">The value of the system property to match</param>
    /// <return>The updated container builder instance</return>
    public S WhenSystemProperty(string name, string value)
    {
        _systemProperties[name] = value;
        return (S)this;
    }

    /// Condition on system properties. The before test logic will only run when this condition matches.
    /// <param name="systemProperties">
    ///     A dictionary containing system property names and their corresponding values to match
    ///     against
    /// </param>
    /// <return>The updated container builder instance</return>
    public S WhenSystemProperties(Dictionary<string, string> systemProperties)
    {
        foreach (var property in systemProperties)
        {
            _systemProperties[property.Key] = property.Value;
        }

        return (S)this;
    }


    /// Condition on environment variable with a specific value. The before test logic will only run when this condition matches.
    /// <param name="name">The name of the environment variable</param>
    /// <param name="value">The value of the environment variable</param>
    /// <return>The updated container builder instance</return>
    public S WhenEnv(string name, string value)
    {
        _env[name] = value;
        return (S)this;
    }

    /// Condition on environment variables. The before test logic will only run when this condition matches.
    /// @param envs A dictionary of environment variable names and values to match against
    /// @return The updated container builder instance
    public S WhenEnv(Dictionary<string, string> envs)
    {
        foreach (var variable in envs)
        {
            _env[variable.Key] = variable.Value;
        }

        return (S)this;
    }

    public override T Build()
    {
        var container = base.Build();

        container.SetNamePattern(_namePattern);
        container.SetPackageNamePattern(_namespaceNamePattern);
        container.SetTestGroups(_testGroups);
        container.SetSystemProperties(_systemProperties);
        container.SetEnv(_env);

        return container;
    }
}
