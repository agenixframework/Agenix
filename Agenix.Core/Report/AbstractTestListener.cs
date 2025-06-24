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
using Agenix.Api;
using Agenix.Api.Report;

namespace Agenix.Core.Report;

/// <summary>
///     Basic implementation of <see cref="ITestListener" /> interface so that subclasses must not implement
///     all methods but only overwrite some listener methods.
/// </summary>
public abstract class AbstractTestListener : ITestListener
{
    public virtual void OnTestFailure(ITestCase test, Exception cause) { }

    public virtual void OnTestSkipped(ITestCase test) { }

    public virtual void OnTestStart(ITestCase test) { }

    public void OnTestFinish(ITestCase test)
    {
    }

    public virtual void OnTestSuccess(ITestCase test) { }

    public virtual void OnTestExecutionEnd(ITestCase test) { }
}
