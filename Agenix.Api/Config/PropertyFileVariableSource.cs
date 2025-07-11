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

using Agenix.Api.IO;
using Agenix.Api.Util;

namespace Agenix.Api.Config;

/// <summary>
///     Implementation of <see cref="IVariableSource" /> that
///     resolves variable name against Java-style property file.
/// </summary>
/// <seealso cref="Api.Util.Properties" />
[Serializable]
public class PropertyFileVariableSource : IVariableSource
{
    private readonly object _objectMonitor = new();
    private bool _ignoreMissingResources;
    private IResource[] _locations;
    protected Properties Properties;

    /// <summary>
    ///     Gets or sets the locations of the property files
    ///     to read properties from.
    /// </summary>
    /// <value>
    ///     The locations of the property files
    ///     to read properties from.
    /// </value>
    public IResource[] Locations
    {
        get => _locations;
        set => _locations = value;
    }

    /// <summary>
    ///     Convenience property. Gets or sets a single location
    ///     to read properties from.
    /// </summary>
    /// <value>
    ///     A location to read properties from.
    /// </value>
    public IResource Location
    {
        set => _locations = [value];
    }

    /// <summary>
    ///     Sets a value indicating whether to ignore resource locations that do not exist.  This will call
    ///     the <see cref="IResource" /> Exists property.
    /// </summary>
    /// <value>
    ///     <c>true</c> if one should ignore missing resources; otherwise, <c>false</c>.
    /// </value>
    public bool IgnoreMissingResources
    {
        set => _ignoreMissingResources = value;
    }

    /// <summary>
    ///     Before requesting a variable resolution, a client should
    ///     ask whether the source can resolve a particular variable name.
    /// </summary>
    /// <param name="name">the name of the variable to resolve</param>
    /// <returns><c>true</c> if the variable can be resolved, <c>false</c> otherwise</returns>
    public bool CanResolveVariable(string name)
    {
        lock (_objectMonitor)
        {
            if (Properties != null)
            {
                return Properties.Contains(name);
            }

            Properties = new Properties();
            InitProperties();
            return Properties.Contains(name);
        }
    }

    /// <summary>
    ///     Resolves variable value for the specified variable name.
    /// </summary>
    /// <param name="name">
    ///     The name of the variable to resolve.
    /// </param>
    /// <returns>
    ///     The variable value if able to resolve, <c>null</c> otherwise.
    /// </returns>
    public string ResolveVariable(string name)
    {
        lock (_objectMonitor)
        {
            if (Properties != null)
            {
                return Properties.GetProperty(name);
            }

            Properties = new Properties();
            InitProperties();
            return Properties.GetProperty(name);
        }
    }

    /// <summary>
    ///     Initializes properties based on the specified
    ///     property file locations.
    /// </summary>
    protected virtual void InitProperties()
    {
        foreach (var location in _locations)
        {
            var exists = location.Exists;
            if (!exists && _ignoreMissingResources)
            {
                continue;
            }

            using var input = location.InputStream;
            lock (_objectMonitor)
            {
                Properties.Load(input);
            }
        }
    }
}
