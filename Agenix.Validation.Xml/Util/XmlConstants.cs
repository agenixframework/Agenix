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

namespace Agenix.Validation.Xml.Util;

/// <summary>
///     XML constants equivalent to Java's XMLConstants.
/// </summary>
public static class XmlConstants
{
    /// <summary>
    ///     Namespace URI to use to represent that there is no Namespace.
    /// </summary>
    public const string NullNsUri = "";

    /// <summary>
    ///     Prefix to use to represent the default XML Namespace.
    /// </summary>
    public const string DefaultNsPrefix = "";

    /// <summary>
    ///     The official XML Namespace name URI.
    /// </summary>
    public const string XmlNsUri = "http://www.w3.org/XML/1998/namespace";

    /// <summary>
    ///     The official XML Namespace prefix.
    /// </summary>
    public const string XmlNsPrefix = "xml";

    /// <summary>
    ///     The official XML attribute used for specifying XML Namespace declarations.
    /// </summary>
    public const string XmlnsAttribute = "xmlns";

    /// <summary>
    ///     The official XML attribute used for specifying XML Namespace declarations, XMLConstants.XMLNS_ATTRIBUTE, Namespace
    ///     name URI.
    /// </summary>
    public const string XmlnsAttributeNsUri = "http://www.w3.org/2000/xmlns/";
}
