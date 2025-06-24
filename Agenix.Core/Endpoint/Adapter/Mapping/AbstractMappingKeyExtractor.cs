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

using Agenix.Api.Endpoint.Adapter.Mapping;
using Agenix.Api.Message;

namespace Agenix.Core.Endpoint.Adapter.Mapping;

/// <summary>
///     Abstract mapping key extractor adds common mapping prefix and suffix added to evaluated mapping key. Subclasses do
///     evaluate mapping key from incoming request message and optional prefix and/ or suffix are automatically added to
///     resulting mapping key.
/// </summary>
public abstract class AbstractMappingKeyExtractor : IMappingKeyExtractor
{
    /// Represents an optional prefix that is automatically added to the extracted mapping key.
    /// /
    private string _mappingKeyPrefix = "";

    private string _mappingKeySuffix = "";

    /// Extracts the mapping key from the incoming request message, incorporating optional prefixes and suffixes.
    /// @param request The incoming request message.
    /// @return The extracted mapping key with optional prefix and suffix.
    /// /
    public string ExtractMappingKey(IMessage request)
    {
        return _mappingKeyPrefix + GetMappingKey(request) + _mappingKeySuffix;
    }

    /// Provides the mapping key from an incoming request message. Subclasses must implement this method.
    /// <param name="request">The incoming request message.</param>
    /// <return>The mapping key extracted from the request.</return>
    protected abstract string GetMappingKey(IMessage request);

    /// Sets the static mapping key prefix that will automatically be added to the extracted mapping key.
    /// @param mappingKeyPrefix The prefix to be added to the mapping key.
    /// /
    public void SetMappingKeyPrefix(string mappingKeyPrefix)
    {
        _mappingKeyPrefix = mappingKeyPrefix;
    }

    /// Sets the static mapping key suffix that will automatically be added to the extracted mapping key.
    /// <param name="mappingKeySuffix">The suffix to be added to the mapping key.</param>
    public void SetMappingKeySuffix(string mappingKeySuffix)
    {
        _mappingKeySuffix = mappingKeySuffix;
    }
}
