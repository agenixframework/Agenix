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

using System;
using Agenix.Api.Functions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;

namespace Agenix.Core.Tests.Functions;

public class CurrentDateFunctionTest : AbstractNUnitSetUp
{
    private readonly CurrentDateFunction _currentDateFunction = new();

    [Test]
    public void TestFunction()
    {
        // Basic format tests
        AssertDateFormat("'yyyy-MM-dd'", "yyyy-MM-dd");
        AssertDateFormat("'yyyy-MM-dd HH:mm:ss'", "yyyy-MM-dd HH:mm:ss");
        AssertDateFormat("'yyyy-MM-dd'T'hh:mm:ss'", "yyyy-MM-dd'T'hh:mm:ss");

        // Single offset tests
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+1y'", 1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+1M'", months: 1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+1d'", days: 1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+1h'", hours: 1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+1m'", minutes: 1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+1s'", seconds: 1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+10y'", 10);

        // Multiple offset tests
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+1y+1M'", 1, 1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+1y+1M+1d'", 1, 1, 1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+1y+1M+1d+1h'", 1, 1, 1, 1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+1y+1M+1d+1h+1m'", 1, 1, 1, 1, 1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+1y+1M+1d+1h+1m+1s'", 1, 1, 1, 1, 1, 1);

        // Negative offset tests
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '-1y'", -1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '-1M'", months: -1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '-1d'", days: -1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '-1h'", hours: -1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '-1m'", minutes: -1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '-1s'", seconds: -1);

        // Mixed offset tests
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '-1y+1M-1d'", -1, 1, -1);
        AssertDateOffset("'yyyy-MM-dd HH:mm:ss', '+1y-1M-1d'", 1, -1, -1);
    }

    private void AssertDateFormat(string parameterString, string expectedFormat)
    {
        var result = _currentDateFunction.Execute(FunctionParameterHelper.GetParameterList(parameterString), Context);
        var expected = DateTime.Now.ToString(expectedFormat);
        Assert.That(result, Is.EqualTo(expected));
    }

    private void AssertDateOffset(string parameterString, int years = 0, int months = 0, int days = 0, int hours = 0,
        int minutes = 0, int seconds = 0)
    {
        var result = _currentDateFunction.Execute(FunctionParameterHelper.GetParameterList(parameterString), Context);

        var expectedDateTime = DateTime.Now
            .AddYears(years)
            .AddMonths(months)
            .AddDays(days)
            .AddHours(hours)
            .AddMinutes(minutes)
            .AddSeconds(seconds);

        var expected = expectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void TestNoParameters()
    {
        var result = _currentDateFunction.Execute([], Context);
        var expected = DateTime.Now.ToString("dd.MM.yyyy");
        Assert.That(result, Is.EqualTo(expected));
    }
}
