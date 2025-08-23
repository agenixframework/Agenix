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

using System.Threading;
using System.Threading.Tasks;
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
public class CustomTestContainerBuilder<T>(T container)
    : AbstractAsyncTestContainerBuilder<T, dynamic>, IAsyncTestAction
    where T : IAsyncTestActionContainer
{
    private readonly T _container = container;

    /// <summary>
    ///     Executes the asynchronous test action using the specified <see cref="TestContext" />.
    /// </summary>
    /// <param name="context">
    ///     The instance of <see cref="TestContext" /> containing information relevant to the execution of
    ///     the test action.
    /// </param>
    /// <param name="cancellationToken"></param>
    /// <returns>
    ///     A task that represents the asynchronous execution operation for the test action.
    /// </returns>
    public async Task ExecuteAsync(TestContext context, CancellationToken cancellationToken = default)
    {
        await base.Build().ExecuteAsync(context, cancellationToken);
    }

    /// <summary>
    ///     Builds and returns an instance of the container of type <typeparamref name="T" />.
    /// </summary>
    /// <returns>
    ///     An instance of <typeparamref name="T" />, representing the constructed container.
    /// </returns>
    protected override T DoBuild()
    {
        return _container;
    }

    /// <summary>
    ///     Builds and returns the container instance. If the container already contains actions, it directly returns the
    ///     container.
    ///     Otherwise, it delegates the build process to the base class implementation.
    /// </summary>
    /// <returns>
    ///     The instance of the container of type <typeparamref name="T" /> after the build process.
    /// </returns>
    public override T Build()
    {
        return _container.GetActions().Count > 0 ? _container : base.Build();
    }

    /// <summary>
    ///     Creates a new instance of <see cref="CustomTestContainerBuilder{C}" /> using the specified container.
    /// </summary>
    /// <param name="container">An instance of a container type that implements <see cref="ITestActionContainer" />.</param>
    /// <typeparam name="TC">The type of the container used, which implements <see cref="ITestActionContainer" />.</typeparam>
    /// <returns>
    ///     A new instance of <see cref="AbstractTestContainerBuilder{C, dynamic}" /> initialized with the specified
    ///     container.
    /// </returns>
    public static CustomTestContainerBuilder<TC> Container<TC>(TC container)
        where TC : IAsyncTestActionContainer
    {
        return new CustomTestContainerBuilder<TC>(container);
    }
}
