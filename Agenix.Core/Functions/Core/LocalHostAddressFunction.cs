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
using System.Collections.Generic;
using System.Net;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function that returns the local host address/hostname.
/// </summary>
public class LocalHostAddressFunction : IFunction
{
    /// <summary>
    ///     Executes the local host address function.
    /// </summary>
    /// <param name="parameterList">Parameter list (must be empty)</param>
    /// <param name="testContext">Test context</param>
    /// <returns>The local hostname as a string</returns>
    /// <exception cref="InvalidFunctionUsageException">Thrown when parameters are provided</exception>
    /// <exception cref="AgenixSystemException">Thrown when unable to locate the local host address</exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList.Count > 0)
        {
            throw new InvalidFunctionUsageException("Unexpected parameter for function.");
        }

        try
        {
            return Dns.GetHostName();
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Unable to locate local host address", e);
        }
    }
}
