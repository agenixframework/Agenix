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

namespace Agenix.Screenplay.Questions.Waits;

/// <summary>
///     Interface defining methods for specifying timeout durations in fluent API scenarios.
/// </summary>
public interface IWithTimeout
{
    /// <summary>
    ///     Specifies the maximum timeout duration using a long value.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>An interface for specifying the time units.</returns>
    IWithTimeUnits ForNoLongerThan(long timeout);

    /// <summary>
    ///     Specifies the maximum timeout duration using an int value.
    /// </summary>
    /// <param name="timeout">The timeout duration.</param>
    /// <returns>An interface for specifying the time units.</returns>
    IWithTimeUnits ForNoLongerThan(int timeout);
}
