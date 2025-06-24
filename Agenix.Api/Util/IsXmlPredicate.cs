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

namespace Agenix.Api.Util;

/// Singleton class to test if a string represents a XML.
/// An empty string is considered to be a valid XML.
/// /
public class IsXmlPredicate
{
    /// <summary>
    ///     Lazy initialization object for the singleton instance of the <c>IsXmlPredicate</c> class.
    /// </summary>
    private static readonly Lazy<IsXmlPredicate> Lazy = new(() => new IsXmlPredicate());

    private IsXmlPredicate()
    {
        // Singleton
    }

    /// <summary>
    ///     Gets the singleton instance of the <c>IsXmlPredicate</c> class.
    /// </summary>
    public static IsXmlPredicate Instance => Lazy.Value;

    /// <summary>
    ///     Tests if a given string represents an XML. An empty string is considered to be valid XML.
    /// </summary>
    /// <param name="toTest">The string to test for XML validity.</param>
    /// <return>True if the string is valid XML or an empty string; false otherwise.</return>
    public bool Test(string toTest)
    {
        toTest = toTest?.Trim();
        return toTest != null && (toTest.Length == 0 || toTest.StartsWith('<'));
    }
}
