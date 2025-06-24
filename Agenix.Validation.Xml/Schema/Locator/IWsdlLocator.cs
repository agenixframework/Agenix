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

namespace Agenix.Validation.Xml.Schema.Locator;

/// <summary>
///     Interface for locating WSDL resources and their imports
/// </summary>
public interface IWsdlLocator : IDisposable
{
    /// <summary>
    ///     Gets the base input source for the WSDL
    /// </summary>
    /// <returns>Stream containing the base WSDL content</returns>
    Stream GetBaseInputSource();

    /// <summary>
    ///     Gets the import input source for imported WSDL or XSD files
    /// </summary>
    /// <param name="parentLocation">The location of the parent WSDL</param>
    /// <param name="importLocation">The location of the import to resolve</param>
    /// <returns>Stream containing the imported content</returns>
    Stream GetImportInputSource(string parentLocation, string importLocation);

    /// <summary>
    ///     Gets the base URI of the WSDL
    /// </summary>
    /// <returns>The base URI as a string</returns>
    string GetBaseUri();

    /// <summary>
    ///     Gets the URI of the latest import that was resolved
    /// </summary>
    /// <returns>The latest import URI as a string, or null if no imports have been resolved</returns>
    string? GetLatestImportUri();

    /// <summary>
    ///     Closes any resources held by this locator
    /// </summary>
    void Close();
}
