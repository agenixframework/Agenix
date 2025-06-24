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
    AbstractReferenceResolverAwareTestActionBuilder<T> : ITestActionBuilder<T>.IDelegatingTestActionBuilder<T>,
    IReferenceResolverAware where T : ITestAction
{
    protected ITestActionBuilder<T> _delegate;
    protected IReferenceResolver referenceResolver;

    public void SetReferenceResolver(IReferenceResolver newReferenceResolver)
    {
        if (referenceResolver != null)
        {
            referenceResolver = newReferenceResolver;

            if (Delegate is IReferenceResolverAware referenceResolverAware)
            {
                referenceResolverAware.SetReferenceResolver(referenceResolver);
            }
        }
    }

    public ITestActionBuilder<T> Delegate => _delegate;

    public abstract T Build();
}
