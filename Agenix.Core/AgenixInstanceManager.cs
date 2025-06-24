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
using Agenix.Api;

namespace Agenix.Core;

/// <summary>
///     Instance creation manager creates new Agenix instances or always a singleton based on instance creation strategy.
/// </summary>
public class AgenixInstanceManager
{
    /**
     * Singleton
     */
    private static Agenix _agenix;

    /**
     * List of instance resolvers capable of taking part an Agenix instance creation process
     */
    private static readonly List<IAgenixInstanceProcessor> InstanceProcessors = [];

    /**
     * Strategy decides which instances are created
     */
    protected static AgenixInstanceStrategy Strategy = AgenixInstanceStrategy.NEW;

    /// Add an instance processor.
    /// <param name="processor">The processor to be added to the instance creation process.</param>
    /// /
    public static void AddInstanceProcessor(IAgenixInstanceProcessor processor)
    {
        InstanceProcessors.Add(processor);
    }

    /**
     * Initializing method loads Agenix context and reads bean definitions
     * such as test listeners and test context factory.
     * @return
     */
    public static Agenix NewInstance()
    {
        return NewInstance(IAgenixContextProvider.Lookup());
    }

    /// Creates a new Agenix instance using the specified context provider.
    /// @param contextProvider The context provider to be used for creating the Agenix instance.
    /// @return The newly created Agenix instance.
    /// /
    public static Agenix NewInstance(IAgenixContextProvider contextProvider)
    {
        if (Strategy.Equals(AgenixInstanceStrategy.NEW) || _agenix == null)
        {
            _agenix = new Agenix(contextProvider.Create());
            InstanceProcessors.ForEach(processor => processor.Process(_agenix));
        }

        return _agenix;
    }

    /// Sets the instance creation strategy.
    /// @param mode The strategy mode to set for instance creation.
    /// /
    public static void Mode(AgenixInstanceStrategy mode)
    {
        Strategy = mode;
    }

    /// Gets the actual instance that has been created with this manager.
    /// @return The current Agenix instance.
    /// /
    public static Agenix Get()
    {
        return _agenix;
    }

    /// Provides access to the current Agenix instance or creates a new one if it doesn't exist.
    /// @return The current or newly created Agenix instance.
    /// /
    public static Agenix GetOrDefault()
    {
        return _agenix ??= NewInstance();
    }

    /// Check if there has already been an instance instantiated using this manager.
    /// @return true if an instance has been created; otherwise, false.
    /// /
    public static bool HasInstance()
    {
        return _agenix != null;
    }

    /// Removes the current Agenix instance, if any.
    /// /
    public static void Reset()
    {
        _agenix = null;
    }
}
