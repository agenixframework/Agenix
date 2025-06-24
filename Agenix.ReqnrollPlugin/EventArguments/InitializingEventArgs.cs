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

namespace Agenix.ReqnrollPlugin.EventArguments;

/// <summary>
///     Represents event data for the initializing event in the Agenix Reqnroll plugin.
/// </summary>
public class InitializingEventArgs(Core.Agenix agenix)
{
    /// <summary>
    ///     Represents the core class of the Agenix system, acting as the central entry point for managing and interacting with
    ///     testing-related functionalities.
    /// </summary>
    /// <remarks>
    ///     The `Agenix` class is responsible for coordinating test execution, suite initialization, reporting mechanisms,
    ///     and listener integrations within the Agenix framework. It serves as a foundational component, encapsulating
    ///     configuration management, context provisioning, and test action execution.
    /// </remarks>
    /// <remarks>
    ///     This class supports adding and managing test-related listeners, reporters, and suites, ensuring a flexible and
    ///     extensible workflow for test execution processes. It also provides utility methods for versioning, resource
    ///     management, and lifecycle operations.
    /// </remarks>
    public Core.Agenix Agenix { get; set; } = agenix;
}
