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

using NHamcrest;

namespace Agenix.Validation.NHamcrest.Validation.Matcher;

/// <summary>
///     A wrapper class that adapts an <see cref="IMatcher{}" /> instance
///     to become an <see cref="IMatcher{object}" /> by handling type casting internally.
/// </summary>
/// <remarks>
///     This class is used to bridge the gap between matchers expecting string input
///     and cases where the input type is object. It ensures that the encapsulated
///     string matcher can still be used in broader matching scenarios involving object types.
/// </remarks>
public class StringToObjectMatcherWrapper : IMatcher<object>
{
    private readonly IMatcher<string> _innerMatcher;

    public StringToObjectMatcherWrapper(IMatcher<string> innerMatcher)
    {
        _innerMatcher = innerMatcher;
    }

    public bool Matches(object item)
    {
        // Cast the input to string and pass it to the inner matcher's Matches method.
        return item is string str && _innerMatcher.Matches(str);
    }

    public void DescribeMismatch(object item, IDescription mismatchDescription)
    {
    }

    public void DescribeTo(IDescription description)
    {
        // Delegate to the inner matcher’s DescribeTo.
        _innerMatcher.DescribeTo(description);
    }
}
