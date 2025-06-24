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

using System;
using Agenix.Api.Context;
using Agenix.Core;

namespace Agenix.ReqnrollPlugin.EventArguments;

/// <summary>
///     Provides data for the event triggered when a run starts.
/// </summary>
public class RunFinishedEventArgs(AgenixContext agenixContext) : EventArgs
{
    /// <summary>
    ///     Represents the arguments for the event raised when a run starts.
    ///     Provides contextual information about the run initiation process.
    /// </summary>
    public RunFinishedEventArgs(AgenixContext agenixContext, TestContext testContext) : this(agenixContext)
    {
        TestContext = testContext;
    }

    /// <summary>
    ///     Represents the core context implementation in Agenix, encapsulating primary components and services
    ///     used throughout the system.
    /// </summary>
    /// <remarks>
    ///     This type is integral to configuring, managing, and coordinating various aspects of the Agenix framework,
    ///     including test listeners, action listeners, reporting, validation, messaging, configuration, and more.
    ///     It implements multiple interfaces to facilitate extensibility by handling specific framework behaviors.
    /// </remarks>
    public AgenixContext AgenixContext { get; } = agenixContext;

    public TestContext TestContext { get; set; }

    public bool Canceled { get; set; }
}
