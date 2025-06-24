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

namespace Agenix.Api.Annotations;

/// <summary>
///     Agenix test case annotation used in C# DSL test cases to execute several tests within one single test builder
///     class. Each method annotated with this annotation will result in a separate test execution.
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class AgenixTestAttribute : Attribute
{
    /// <summary>
    ///     Test name optional - by default, the method name is used as a test name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
