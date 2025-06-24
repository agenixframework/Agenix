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

using System.Collections.Concurrent;
using Agenix.Api.Exceptions;

namespace Agenix.Api.Validation.Matcher;

/// <summary>
///     Library holding a set of validation matchers. Each library defines a validation prefix as namespace, so
///     there will be no naming conflicts when using multiple libraries at a time.
/// </summary>
public class ValidationMatcherLibrary
{
    /// <summary>
    ///     Dictionary of validationMatchers in this library
    /// </summary>
    private ConcurrentDictionary<string, IValidationMatcher> _members = [];

    /// <summary>
    ///     Name of ValidationMatcher library
    /// </summary>
    private string _name = "standard";

    /// <summary>
    ///     ValidationMatcher library prefix
    /// </summary>
    private string _prefix = "";

    /// <summary>
    ///     Dictionary of validationMatchers in this library
    /// </summary>
    public ConcurrentDictionary<string, IValidationMatcher> Members
    {
        get => _members;
        set => _members = value;
    }

    /// <summary>
    ///     Name of ValidationMatcher library
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    /// <summary>
    ///     ValidationMatcher library prefix
    /// </summary>
    public string Prefix
    {
        get => _prefix;
        set => _prefix = value;
    }

    /// <summary>
    ///     Try to find validationMatcher in library by name.
    /// </summary>
    /// <param name="validationMatcherName">validationMatcher name.</param>
    /// <returns>the validationMatcher instance.</returns>
    public IValidationMatcher GetValidationMatcher(string validationMatcherName)
    {
        if (!_members.TryGetValue(validationMatcherName, out var matcher))
        {
            throw new NoSuchValidationMatcherException(
                "Can not find validation matcher " + validationMatcherName + " in library " + _name + " (" +
                _prefix + ")");
        }

        return matcher;
    }

    /// <summary>
    ///     Does this library know a validationMatcher with the given name.
    /// </summary>
    /// <param name="validationMatcherName">name to search for</param>
    /// <returns>boolean flag to mark existence.</returns>
    public bool KnowsValidationMatcher(string validationMatcherName)
    {
        if (validationMatcherName.Contains(':'))
        {
            var validationMatcherPrefix =
                validationMatcherName[..(validationMatcherName.IndexOf(':') + 1)];

            var startIndex = validationMatcherName.IndexOf(':');
            var length = validationMatcherName.IndexOf('(') - startIndex - 1;

            return validationMatcherPrefix.Equals(_prefix) && _members.ContainsKey(
                validationMatcherName.Substring(startIndex + 1, length));
        }

        return _members.ContainsKey(validationMatcherName[..validationMatcherName.IndexOf('(')]);
    }
}
