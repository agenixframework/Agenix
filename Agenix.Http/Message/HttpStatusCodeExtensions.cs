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

using System.Net;

namespace Agenix.Http.Message;

/// <summary>
///     Provides extension methods for working with the <see cref="HttpStatusCode" /> enumeration.
/// </summary>
public static class HttpStatusCodeExtensions
{
    /// <summary>
    ///     Converts an integer status code to its corresponding <see cref="HttpStatusCode" /> enumeration value, if defined.
    /// </summary>
    /// <param name="statusCode">The integer value of the status code to convert.</param>
    /// <returns>
    ///     The <see cref="HttpStatusCode" /> enumeration value corresponding to the specified integer status code, or null if
    ///     the status code is not defined in the <see cref="HttpStatusCode" /> enumeration.
    /// </returns>
    public static HttpStatusCode? ValueOf(int statusCode)
    {
        if (Enum.IsDefined(typeof(HttpStatusCode), statusCode))
        {
            return (HttpStatusCode)statusCode;
        }

        return null; // Or handle custom status codes differently
    }
}
