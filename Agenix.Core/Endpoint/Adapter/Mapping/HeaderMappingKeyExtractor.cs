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

using Agenix.Api.Exceptions;
using Agenix.Api.Message;

namespace Agenix.Core.Endpoint.Adapter.Mapping;

/// <summary>
///     This class is responsible for extracting a mapping key from the headers of an incoming message.
/// </summary>
public class HeaderMappingKeyExtractor : AbstractMappingKeyExtractor
{
    /// Represents the header name used for mapping key extraction.
    /// /
    private string _headerName = "";

    /// <summary>
    ///     This class is responsible for extracting a mapping key from the headers of an incoming message.
    /// </summary>
    public HeaderMappingKeyExtractor()
    {
    }

    /// This class is responsible for extracting a mapping key from the headers of an incoming message.
    /// /
    public HeaderMappingKeyExtractor(string headerName)
    {
        _headerName = headerName;
    }

    /// <summary>
    ///     Extracts a mapping key from the headers of the given message.
    /// </summary>
    /// <param name="request">The incoming message from which the header mapping key is to be extracted.</param>
    /// <returns>The extracted mapping key as a string.</returns>
    /// <exception cref="AgenixSystemException">Thrown when the specified header is not found in the request message.</exception>
    protected override string GetMappingKey(IMessage request)
    {
        if (request.GetHeader(_headerName) != null)
        {
            return request.GetHeader(_headerName)?.ToString();
        }

        throw new AgenixSystemException($"Unable to find header '{_headerName}' in request message");
    }

    /// <summary>
    ///     Sets the header name used for mapping key extraction.
    /// </summary>
    /// <param name="headerName">The name of the header to be used for mapping key extraction.</param>
    public void SetHeaderName(string headerName)
    {
        _headerName = headerName;
    }
}
