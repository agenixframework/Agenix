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

namespace Agenix.Core;

/// <summary>
///     Represents a delegate for processing an Agenix instance during its creation process.
/// </summary>
/// <param name="instance">The Agenix instance to be processed.</param>
public delegate void AgenixInstanceProcessorDelegate(Agenix instance);

/// <summary>
///     Agenix instance processor takes part in instance creation process.
/// </summary>
public interface IAgenixInstanceProcessor
{
    /// <summary>
    ///     Process Agenix instance after this has been instantiated.
    /// </summary>
    /// <param name="instance">The Agenix instance to process.</param>
    void Process(Agenix instance);
}
