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
using Agenix.Api.Common;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;

namespace Agenix.Core.Endpoint;

public abstract class AbstractEndpointBuilder<T> : IEndpointBuilder<T> where T : IEndpoint
{
    public virtual T Build()
    {
        return GetEndpoint();
    }

    public virtual bool Supports(Type endpointType)
    {
        return GetEndpoint().GetType() == endpointType;
    }

    /// <summary>
    ///     Sets the endpoint name.
    /// </summary>
    /// <param name="endpointName">the endpoint name</param>
    /// <returns></returns>
    public AbstractEndpointBuilder<T> Name(string endpointName)
    {
        GetEndpoint().SetName(endpointName);
        return this;
    }

    /// <summary>
    ///     Initializes the endpoint.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="AgenixSystemException"></exception>
    public AbstractEndpointBuilder<T> Initialize()
    {
        if (GetEndpoint() is InitializingPhase phase)
        {
            try
            {
                phase.Initialize();
            }
            catch (Exception e)
            {
                throw new AgenixSystemException("Failed to initialize server", e);
            }
        }

        return this;
    }

    /// <summary>
    ///     Gets the target endpoint instance.
    /// </summary>
    /// <returns></returns>
    protected abstract T GetEndpoint();
}
