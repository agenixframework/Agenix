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
using Agenix.Validation.NHamcrest.Validation;

namespace Agenix.Validation.NHamcrest.Tests.Validation;

public class ValueMatcherTest
{
    [Test]
    public void TestLookup()
    {
        var validators = IValueMatcher.Lookup();

        Assert.That(validators, Is.Not.Null);
        Assert.That(validators, Is.Not.Empty);
        Assert.That(validators.Count, Is.EqualTo(1));
        Assert.That(validators.ContainsKey("nhamcrest"), Is.True);
        Assert.That(validators["nhamcrest"].GetType(), Is.EqualTo(typeof(NHamcrestValueMatcher)));
    }
}
