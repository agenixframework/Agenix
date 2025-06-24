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

using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Tasks;

public class WhenInstrumentingTasksTests
{
    [Test]
    public void ANonInstrumentedClassTaskExecution()
    {
        IPerformable basicTask = new EatsAMango();

        Actor.Named("Annie").AttemptsTo(basicTask);
    }

    [Test]
    public void ForClassesWithoutDefaultConstructorUseExplicitlyInstrumentedClass()
    {
        // When
        Actor.Named("Annie").AttemptsTo(EatsAPear.OfSize("large"));
    }

    [Test]
    public void ANonInstrumentedClassWithABuilder()
    {
        // When
        Actor.Named("Annie").AttemptsTo(EatsFruit.Loudly());
    }

    [Test]
    public void ANestedTask()
    {
        // When
        Actor.Named("Annie").AttemptsTo(new Eats(new EatsAMango()));
    }

    [Test]
    public void ATaskWithParameters()
    {
        // When
        Actor.Named("Annie").AttemptsTo(EatsAWatermelon.Quietly(), EatsAWatermelon.Noisily());
    }
}
