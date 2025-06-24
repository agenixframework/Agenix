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

using System.Text;

namespace Agenix.Api.Exceptions;

/// Represents a custom exception that is thrown when multiple actions in a parallel container fail.
/// This exception aggregates a list of nested exceptions that caused the parallel execution to fail,
/// providing a comprehensive message detailing each individual exception.
/// /
public class ParallelContainerException(List<AgenixSystemException> nestedExceptions) : AgenixSystemException
{
    public override string Message
    {
        get
        {
            var builder = new StringBuilder();

            builder.Append("Several actions failed in parallel container");
            foreach (var exception in nestedExceptions)
            {
                builder.Append("\n\t+ ").Append(exception.GetType().Name).Append(": ").Append(exception.Message);
            }

            return builder.ToString();
        }
    }
}
