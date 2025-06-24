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
using Agenix.Api.Report;

namespace Agenix.Core.Report;

/// Provides a default set of test reporters to be used during test executions.
/// This class automatically adds a predefined list of test reporters upon instantiation, facilitating
/// the test reporting mechanism with minimal configuration.
/// It derives from the base class TestReporters, utilizing its methods to manage the addition
/// and orchestration of test reporters.
public class DefaultTestReporters : TestReporters
{
    public static readonly List<ITestReporter> DefaultReporters = [new LoggingReporter()];

    public DefaultTestReporters()
    {
        DefaultReporters.ForEach(AddTestReporter);
    }
}
