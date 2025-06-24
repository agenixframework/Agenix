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

namespace Agenix.Api.Validation.Matcher;

/// <summary>
///     ValidationMatcher registry holding all available validation matcher libraries.
/// </summary>
public class ValidationMatcherRegistry
{
    /// <summary>
    ///     List of libraries providing custom validation matchers
    /// </summary>
    private List<ValidationMatcherLibrary> _validationMatcherLibraries = new();

    /// <summary>
    ///     List of libraries providing custom validation matchers
    /// </summary>
    public List<ValidationMatcherLibrary> ValidationMatcherLibraries
    {
        get => _validationMatcherLibraries;
        set => _validationMatcherLibraries = value;
    }

    public ValidationMatcherLibrary GetLibraryForPrefix(string validationMatcherPrefix)
    {
        if (_validationMatcherLibraries == null)
        {
            throw new NoSuchValidationMatcherLibraryException(
                $"Can not find ValidatorMatcher library for prefix '{validationMatcherPrefix}'");
        }

        foreach (var validationMatcherLibrary in _validationMatcherLibraries.Where(validationMatcherLibrary =>
                     validationMatcherLibrary.Prefix.Equals(validationMatcherPrefix)))
        {
            return validationMatcherLibrary;
        }

        throw new NoSuchValidationMatcherLibraryException(
            $"Can not find ValidatorMatcher library for prefix '{validationMatcherPrefix}'");
    }

    /// <summary>
    ///     Adds given validation matcher library to this registry.
    /// </summary>
    /// <param name="validationMatcherLibrary"></param>
    public void AddValidationMatcherLibrary(ValidationMatcherLibrary validationMatcherLibrary)
    {
        var prefixAlreadyUsed =
            _validationMatcherLibraries.Any(lib => lib.Prefix.Equals(validationMatcherLibrary.Prefix));

        if (prefixAlreadyUsed)
        {
            throw new AgenixSystemException(
                $"Validation matcher library prefix '{validationMatcherLibrary.Prefix} is already bound to another instance. " +
                "Please choose another prefix.");
        }

        _validationMatcherLibraries.Add(validationMatcherLibrary);
    }
}
