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
using Agenix.Api.Container;
using Agenix.Api.Context;

namespace Agenix.Core.Container;

/// <summary>
///     Represents a builder pattern class for creating instances of test action containers.
///     This class extends the <see cref="AbstractTestContainerBuilder{T, dynamic}" /> and implements
///     the <see cref="ITestAction" /> interface.
/// </summary>
/// <typeparam name="T">The type of test action container that implements <see cref="ITestActionContainer" />.</typeparam>
/// <remarks>
///     This class builds a test action container of type <typeparamref name="T" /> by providing concrete
///     implementations for building and executing test actions within the container. It checks for existing actions
///     and decides the container's build path accordingly.
/// </remarks>
public class CustomTestContainerBuilder<T>(T container) : AbstractTestContainerBuilder<T, dynamic>, ITestAction
    where T : ITestActionContainer
{
    private readonly T _container = container;

    public void Execute(TestContext context)
    {
        base.Build().Execute(context);
    }

    protected override T DoBuild()
    {
        return _container;
    }

    public override T Build()
    {
        if (_container.GetActions().Count > 0)
        {
            return _container;
        }

        return base.Build();
    }

    /// <summary>
    ///     Creates a new instance of <see cref="CustomTestContainerBuilder{C}" /> using the specified container.
    /// </summary>
    /// <param name="container">An instance of a container type that implements <see cref="ITestActionContainer" />.</param>
    /// <typeparam name="C">The type of the container used, which implements <see cref="ITestActionContainer" />.</typeparam>
    /// <returns>
    ///     A new instance of <see cref="AbstractTestContainerBuilder{C, dynamic}" /> initialized with the specified
    ///     container.
    /// </returns>
    public static AbstractTestContainerBuilder<C, dynamic> Container<C>(C container)
        where C : ITestActionContainer
    {
        return new CustomTestContainerBuilder<C>(container);
    }
}
