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

using Agenix.Api.Validation;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Validation.Xml;

namespace Agenix.Validation.Xml.Dsl;

/// <summary>
///     Provides a set of static methods to support XML-related validation processes.
/// </summary>
public static class XmlSupport
{
    /// <summary>
    ///     Entrance for all XML-related validation functionalities.
    /// </summary>
    /// <returns>XML message validation context builder</returns>
    public static XmlMessageValidationContext.Builder Xml()
    {
        return XmlMessageValidationContext.Builder.Xml();
    }

    /// <summary>
    ///     Marshaling validation processor builder entrance.
    /// </summary>
    /// <typeparam name="T">The type to validate</typeparam>
    /// <param name="validationProcessor">The validation processor</param>
    /// <returns>XML marshaling validation processor builder</returns>
    public static XmlMarshallingValidationProcessor<T>.Builder<T> Validate<T>(
        GenericValidationProcessor<T> validationProcessor)
    {
        return XmlMarshallingValidationProcessor<T>.Builder<T>.Validate(validationProcessor);
    }
}
