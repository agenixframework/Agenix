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

namespace Agenix.Api.Report;

/// Listener for events regarding a test suite lifecycle.
/// /
public interface IAsyncTestSuiteListener
{
    /// Invoked on test suite start.
    /// /
    Task OnStart();

    /// Invoked after successful test suite start.
    Task OnStartSuccess();

    /// Invoked after failed test suite start.
    /// <param name="cause">The exception cause of the failure.</param>
    Task OnStartFailure(Exception cause);

    /// Invoked on test suite finish.
    Task OnFinish();

    /// Invoked after successful test suite finish.
    Task OnFinishSuccess();

    /// Invoked after failed test suite finish.
    /// <param name="cause">The exception cause of the failure.</param>
    Task OnFinishFailure(Exception cause);
}
