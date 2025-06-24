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

namespace Agenix.Screenplay;

/// <summary>
///     Provides utility methods to create and manage task instances that implement the IPerformable interface.
/// </summary>
public class Tasks
{
    /// <summary>
    ///     Creates an instance of a specified step class, ensuring that the instance implements the IPerformable interface.
    /// </summary>
    /// <typeparam name="T">The type that implements the IPerformable interface.</typeparam>
    /// <param name="stepClass">The type of the step class to instantiate.</param>
    /// <param name="parameters">An array of parameters required to initialize the step class.</param>
    /// <returns>An instance of the specified step class cast to the type T.</returns>
    public static T Instrumented<T>(params object[] parameters) where T : IPerformable
    {
        return DynamicConstructorBuilder.CreateInstance<T>(parameters);
    }
}
