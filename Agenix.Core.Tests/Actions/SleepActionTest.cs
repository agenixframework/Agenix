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
using Agenix.Core.Actions;
using Agenix.Core.Util;
using NUnit.Framework;

namespace Agenix.Core.Tests.Actions;

public class SleepActionTest : AbstractNUnitSetUp
{
    [Test]
    public void TestSleepDuration()
    {
        var sleep = new SleepAction.Builder()
            .Time(TimeSpan.FromMilliseconds(200))
            .Build();

        sleep.Execute(Context);
    }

    [Test]
    public void TestSleep()
    {
        var sleep = new SleepAction.Builder()
            .Milliseconds(100L)
            .Build();

        sleep.Execute(Context);
    }

    [Test]
    public void TestSleepVariablesSupport()
    {
        var sleep = new SleepAction.Builder()
            .Milliseconds("${time}")
            .Build();

        Context.SetVariable("time", "100");

        sleep.Execute(Context);
    }

    [Test]
    public void TestSleepDecimalValueSupport()
    {
        var sleep = new SleepAction.Builder()
            .Time("500.0", ScheduledExecutor.TimeUnit.MILLISECONDS)
            .Build();

        sleep.Execute(Context);

        /*sleep = new SleepAction.Builder()
            .Time("0.5", ScheduledExecutor.TimeUnit.SECONDS)
            .Build();

        sleep.Execute(Context);*/

        /*sleep = new SleepAction.Builder()
            .Time("0.01", ScheduledExecutor.TimeUnit.MINUTES)
            .Build();

        sleep.Execute(Context);*/
    }

    [Test]
    public void TestSleepLegacy()
    {
        var sleep = new SleepAction.Builder()
            .Seconds(0.1)
            .Build();

        sleep.Execute(Context);
    }

    [Test]
    public void TestSleepLegacyVariablesSupport()
    {
        var sleep = new SleepAction.Builder()
            .Time("${time}", ScheduledExecutor.TimeUnit.SECONDS)
            .Build();

        Context.SetVariable("time", "1");

        sleep.Execute(Context);
    }
}
