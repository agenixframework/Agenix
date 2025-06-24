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

namespace Agenix.Api.Spi;

/**
 * Bind objects to registry for later reference. Objects declared in registry can be injected in various ways (e.g. annotations).
 */
public interface IReferenceRegistry
{
    void Bind(string name, object value);
}

/// ReferenceRegistry is responsible for binding objects to a registry for later reference.
/// Declared objects can be injected in multiple ways, such as through annotations.
/// /
public class ReferenceRegistry : IReferenceRegistry
{
    public void Bind(string name, object value)
    {
        //implement the bind logic here
    }

    /// Get proper bean name for future bind operation on registry.
    /// @param bindAnnotation The annotation containing binding information.
    /// @param defaultName The default name to use if the annotation does not provide a name.
    /// @return The name to use for binding in the registry.
    /// /
    public static string GetName(BindToRegistryAttribute bindAnnotation, string defaultName)
    {
        return !string.IsNullOrEmpty(bindAnnotation.Name) ? bindAnnotation.Name : defaultName;
    }
}
