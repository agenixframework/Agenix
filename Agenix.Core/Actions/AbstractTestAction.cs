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

using Agenix.Api;
using Agenix.Api.Common;
using Agenix.Api.Context;

namespace Agenix.Core.Actions;

/// <summary>
///     Abstract base class for test actions. Class provides a default name and description.
/// </summary>
public abstract class AbstractTestAction : ITestAction, INamed, IDescribed
{
    /// <summary>
    ///     Describing the test action
    /// </summary>
    protected string description;

    /// <summary>
    /// Abstract base class for test actions.
    /// Provides default implementation for name and description management.
    /// </summary>
    public AbstractTestAction()
    {
        Name = GetType().Name;
    }

    public AbstractTestAction(string name, string description)
    {
        Name = name;
        this.description = description;
    }

    public AbstractTestAction(string name,
        AbstractTestActionBuilder<ITestAction, dynamic> builder)
    {
        Name = builder.GetName() ?? name;
        description = builder.GetDescription();
    }

    public string GetDescription()
    {
        return description;
    }

    public ITestAction SetDescription(string description)
    {
        this.description = description;
        return this;
    }

    public void SetName(string name)
    {
        Name = name;
    }

    /// <summary>
    ///     TestAction name injected as spring bean name
    /// </summary>
    public string Name { get; private set; }

    public virtual bool IsDisabled(TestContext context)
    {
        return false;
    }

    /// <summary>
    ///     Do basic logging and delegate execution to subclass.
    /// </summary>
    /// <param name="context"></param>
    public virtual void Execute(TestContext context)
    {
        if (!IsDisabled(context))
        {
            DoExecute(context);
        }
    }

    /// <summary>
    ///     Subclasses may add custom execution logic here.
    /// </summary>
    /// <param name="context"></param>
    public abstract void DoExecute(TestContext context);
}
