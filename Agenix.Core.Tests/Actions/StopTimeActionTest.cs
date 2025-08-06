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

using System.Threading.Tasks;
using Agenix.Core.Actions;
using NUnit.Framework;

namespace Agenix.Core.Tests.Actions;

public class StopTimeActionTest : AbstractNUnitSetUp
{
    [Test]
    public async Task TestDefaultTimeline()
    {
        var stopTime = new StopTimeAction.Builder().Build();

        Assert.That(Context.GetVariables().ContainsKey(StopTimeAction.DefaultTimeLineId), Is.False);
        Assert.That(
            Context.GetVariables()
                .ContainsKey(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix),
            Is.False);

        await stopTime.ExecuteAsync(Context);
        Assert.That(Context.GetVariables().ContainsKey(StopTimeAction.DefaultTimeLineId), Is.True);
        Assert.That(
            Context.GetVariables()
                .ContainsKey(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix),
            Is.True);
        Assert.That(
            Context.GetVariable<long>(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix),
            Is.EqualTo(0L));

        await Task.Delay(100);

        await stopTime.ExecuteAsync(Context);
        Assert.That(Context.GetVariables().ContainsKey(StopTimeAction.DefaultTimeLineId), Is.True);
        Assert.That(
            Context.GetVariables()
                .ContainsKey(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix),
            Is.True);
        Assert.That(
            Context.GetVariable<long>(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix),
            Is.GreaterThanOrEqualTo(100L));

        await Task.Delay(100);

        await stopTime.ExecuteAsync(Context);
        Assert.That(Context.GetVariables().ContainsKey(StopTimeAction.DefaultTimeLineId), Is.True);
        Assert.That(
            Context.GetVariables()
                .ContainsKey(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix),
            Is.True);
        Assert.That(
            Context.GetVariable<long>(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix),
            Is.GreaterThanOrEqualTo(200L));
    }

    [Test]
    public async Task TestCustomTimeline()
    {
        var stopTime = new StopTimeAction.Builder()
            .WithId("stopMe")
            .WithSuffix("_time")
            .Build();

        Assert.That(Context.GetVariables().ContainsKey("stopMe"), Is.False);
        Assert.That(Context.GetVariables().ContainsKey("stopMe_time"), Is.False);

        await stopTime.ExecuteAsync(Context);
        Assert.That(Context.GetVariables().ContainsKey("stopMe"), Is.True);
        Assert.That(Context.GetVariables().ContainsKey("stopMe_time"), Is.True);
        Assert.That(Context.GetVariable<long>("stopMe_time"), Is.EqualTo(0L));

        await Task.Delay(100);

        await stopTime.ExecuteAsync(Context);
        Assert.That(Context.GetVariables().ContainsKey("stopMe"), Is.True);
        Assert.That(Context.GetVariables().ContainsKey("stopMe_time"), Is.True);
        Assert.That(Context.GetVariable<long>("stopMe_time"), Is.GreaterThanOrEqualTo(100L));

        await Task.Delay(100);

        await stopTime.ExecuteAsync(Context);
        Assert.That(Context.GetVariables().ContainsKey("stopMe"), Is.True);
        Assert.That(Context.GetVariables().ContainsKey("stopMe_time"), Is.True);
        Assert.That(Context.GetVariable<long>("stopMe_time"), Is.GreaterThanOrEqualTo(200L));
    }

    [Test]
    public async Task TestMultipleTimelines()
    {
        var stopTime1 = new StopTimeAction.Builder()
            .WithId("stopThem")
            .Build();

        var stopTime2 = new StopTimeAction.Builder()
            .WithId("stopUs")
            .Build();

        Assert.That(Context.GetVariables().ContainsKey("stopThem"), Is.False);
        Assert.That(Context.GetVariables().ContainsKey("stopThem_VALUE"), Is.False);
        Assert.That(Context.GetVariables().ContainsKey("stopUs"), Is.False);
        Assert.That(Context.GetVariables().ContainsKey("stopUs_VALUE"), Is.False);

        await stopTime1.ExecuteAsync(Context);
        Assert.That(Context.GetVariables().ContainsKey("stopThem"), Is.True);
        Assert.That(Context.GetVariables().ContainsKey("stopThem_VALUE"), Is.True);
        Assert.That(Context.GetVariables().ContainsKey("stopUs"), Is.False);
        Assert.That(Context.GetVariables().ContainsKey("stopUs_VALUE"), Is.False);
        Assert.That(Context.GetVariable<long>("stopThem_VALUE"), Is.EqualTo(0L));

        await Task.Delay(100);

        await stopTime2.ExecuteAsync(Context);
        Assert.That(Context.GetVariables().ContainsKey("stopThem"), Is.True);
        Assert.That(Context.GetVariables().ContainsKey("stopThem_VALUE"), Is.True);
        Assert.That(Context.GetVariables().ContainsKey("stopUs"), Is.True);
        Assert.That(Context.GetVariables().ContainsKey("stopUs_VALUE"), Is.True);
        Assert.That(Context.GetVariable<long>("stopThem_VALUE"), Is.EqualTo(0L));
        Assert.That(Context.GetVariable<long>("stopUs_VALUE"), Is.EqualTo(0L));

        await Task.Delay(100);

        await stopTime1.ExecuteAsync(Context);
        await stopTime2.ExecuteAsync(Context);
        Assert.That(Context.GetVariables().ContainsKey("stopThem"), Is.True);
        Assert.That(Context.GetVariables().ContainsKey("stopThem_VALUE"), Is.True);
        Assert.That(Context.GetVariables().ContainsKey("stopUs"), Is.True);
        Assert.That(Context.GetVariables().ContainsKey("stopUs_VALUE"), Is.True);
        Assert.That(Context.GetVariable<long>("stopThem_VALUE"), Is.GreaterThanOrEqualTo(200L));
        Assert.That(Context.GetVariable<long>("stopUs_VALUE"), Is.GreaterThanOrEqualTo(100L));
    }
}
