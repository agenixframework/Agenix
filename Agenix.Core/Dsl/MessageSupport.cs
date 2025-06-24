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

using Agenix.Core.Validation;
using Agenix.Core.Variable;

namespace Agenix.Core.Dsl;

/// Static entrance for all message-related DSL functionalities.
/// <return>A new instance of MessageSupport</return>
public class MessageSupport
{
    /// Static entrance for all message-related Java DSL functionalities.
    /// <return>A new instance of MessageSupport</return>
    public static MessageSupport Message()
    {
        return new MessageSupport();
    }

    /// Static entrance for all message-header-related DSL functionalities.
    /// <return>A new instance of MessageHeaderVariableExtractor.Builder</return>
    public MessageHeaderVariableExtractor.Builder Headers()
    {
        return MessageHeaderSupport.FromHeaders();
    }

    /// Entry point for extracting a payload variable from the message body.
    /// @return A new instance of DelegatingPayloadVariableExtractor.Builder.
    public DelegatingPayloadVariableExtractor.Builder Body()
    {
        return MessageBodySupport.FromBody();
    }

    /// Static entrance for all message-header-related C# DSL functionalities.
    /// Returns a new instance of MessageHeaderVariableExtractor.Builder.
    /// /
    public static class MessageHeaderSupport
    {
        /// Static entrance for all message-header-related Java DSL functionalities.
        /// <return>A new instance of MessageHeaderVariableExtractor.Builder</return>
        public static MessageHeaderVariableExtractor.Builder FromHeaders()
        {
            return MessageHeaderVariableExtractor.Builder.FromHeaders();
        }
    }

    /// Provides a C# Domain Specific Language (DSL) helper for message bodies.
    /// This class offers static methods to facilitate operations related to message bodies in the DSL.
    /// /
    public static class MessageBodySupport
    {
        /// Static entrance for all messaging body-related DSL functionalities.
        /// <return />
        /// A new instance of DelegatingPayloadVariableExtractor.Builder.
        public static DelegatingPayloadVariableExtractor.Builder FromBody()
        {
            return DelegatingPayloadVariableExtractor.Builder.FromBody();
        }
    }
}
