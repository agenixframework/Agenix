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

namespace Agenix.Api.Spi;

/// <summary>
///     Abstract base class for building test actions with reference resolver capabilities.
///     This class is intended to be inherited by other test action builders that require
///     access to a reference resolver to resolve dependencies or references during their construction or execution.
/// </summary>
/// <typeparam name="T">The type of the test action that this builder creates.</typeparam>
/// <remarks>
///     This class implements both <see cref="ITestActionBuilder{T}.IDelegatingTestActionBuilder{T}" /> and
///     <see cref="IReferenceResolverAware" />.
///     It facilitates delegation to another implementation of <see cref="ITestActionBuilder{T}" /> and seamlessly
///     integrates with a reference resolver
///     through the <see cref="SetReferenceResolver" /> method.
/// </remarks>
/// <example>
///     This class should be extended by concrete implementations to provide the logic for constructing specific types of
///     test actions.
/// </example>
public abstract class
    AbstractReferenceResolverAwareTestActionBuilder<T> : IAsyncTestActionBuilder<T>.IDelegatingTestActionBuilder<T>,
    IReferenceResolverAware where T : IAsyncTestAction
{
    /// <summary>
    ///     Represents the delegate instance responsible for managing the construction
    ///     or delegation of asynchronous test actions within the current test action builder context.
    /// </summary>
    /// <remarks>
    ///     This variable serves as the primary mechanism for assigning and propagating
    ///     action-building responsibilities to another action builder or sub-builder. It allows
    ///     for chaining and composition of action-building steps while maintaining flexibility
    ///     in the overall construction process.
    /// </remarks>
    /// <value>
    ///     An instance of <see cref="IAsyncTestActionBuilder{T}" /> which facilitates delegation
    ///     of action-building logic. When set, it ensures that the current builder has a delegate
    ///     capable of handling the construction of the intended test action.
    /// </value>
    protected IAsyncTestActionBuilder<T> _delegate;

    /// <summary>
    ///     Represents the resolver for managing references within the current context.
    ///     It is responsible for resolving and providing access to specific references
    ///     as required by the test action builder or associated components.
    /// </summary>
    /// <remarks>
    ///     This property is typically used to integrate a reference resolver into a
    ///     test action building process or to propagate reference resolution capabilities
    ///     across composed action builders.
    /// </remarks>
    /// <value>
    ///     An instance of <see cref="IReferenceResolver" /> used for resolving references
    ///     within the action building or execution context. When not set, reference
    ///     resolution may not be available.
    /// </value>
    protected IReferenceResolver? ReferenceResolver;

    /// <summary>
    ///     Gets the delegate instance of the <see cref="IAsyncTestActionBuilder{T}" /> being utilized
    ///     by the current test action builder. The delegate represents the inner test action builder
    ///     that is being wrapped or forwarded for further handling.
    /// </summary>
    /// <value>
    ///     The delegate represents the instance of the inner <see cref="IAsyncTestActionBuilder{T}" />
    ///     that the current builder forwards actions to or utilizes.
    /// </value>
    public IAsyncTestActionBuilder<T> Delegate => _delegate;

    /// <summary>
    ///     Builds and returns an instance of the test action type.
    /// </summary>
    /// <returns>An instance of the constructed test action.</returns>
    public abstract T Build();

    /// <summary>
    ///     Sets the reference resolver to be used for resolving references during test action execution.
    /// </summary>
    /// <param name="referenceResolver">The new reference resolver to set.</param>
    public void SetReferenceResolver(IReferenceResolver referenceResolver)
    {
        if (ReferenceResolver == null)
        {
            return;
        }

        ReferenceResolver = referenceResolver;

        if (Delegate is IReferenceResolverAware referenceResolverAware)
        {
            referenceResolverAware.SetReferenceResolver(referenceResolver);
        }
    }
}
