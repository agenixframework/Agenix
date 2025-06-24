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

using Agenix.Api.Message.Correlation;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message.Correlation;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Message.Correlation;

public class PollingCorrelationManagerTest
{
    private Mock<IObjectStore<string>> _mockObjectStore;

    [SetUp]
    public void SetUp()
    {
        _mockObjectStore = new Mock<IObjectStore<string>>();
    }

    [Test]
    public void TestFind()
    {
        var pollableEndpointConfiguration = new DirectSyncEndpointConfiguration
        {
            PollingInterval = 100L,
            Timeout = 500L
        };

        var correlationManager = new PollingCorrelationManager<string>(pollableEndpointConfiguration, "Try again");

        // Testing find method with empty key
        ClassicAssert.IsNull(correlationManager.Find(""));

        // Storing and finding objects in correlation manager
        correlationManager.Store("foo", "bar");
        ClassicAssert.IsNull(correlationManager.Find("bar"));
        ClassicAssert.AreEqual("bar", correlationManager.Find("foo"));

        // 2nd invocation with same correlation key
        ClassicAssert.IsNull(correlationManager.Find("foo"));

        // Storing multiple items and retrieving them in different order
        foreach (var key in new[] { "1", "2", "3", "4", "5" })
        {
            correlationManager.Store(key, "value" + key);
        }

        foreach (var key in new[] { "1", "5", "3", "2", "4" })
        {
            ClassicAssert.AreEqual("value" + key, correlationManager.Find(key));
            ClassicAssert.IsNull(correlationManager.Find(key));
        }
    }

    [Test]
    public void TestFindWithRetry()
    {
        var pollableEndpointConfiguration = new DirectSyncEndpointConfiguration
        {
            PollingInterval = 100L,
            Timeout = 500L
        };

        var correlationManager = new PollingCorrelationManager<string>(pollableEndpointConfiguration, "Try again");
        correlationManager.SetObjectStore(_mockObjectStore.Object);

        _mockObjectStore.Reset();

        _mockObjectStore
            .SetupSequence(store => store.Remove("foo"))
            .Returns((string)null)
            .Returns("bar");

        ClassicAssert.AreEqual("bar", correlationManager.Find("foo"));
    }

    [Test]
    public void TestNotFindWithRetry()
    {
        var pollableEndpointConfiguration = new DirectSyncEndpointConfiguration
        {
            PollingInterval = 100L,
            Timeout = 300L
        };

        var correlationManager = new PollingCorrelationManager<string>(pollableEndpointConfiguration, "Try again");
        correlationManager.SetObjectStore(_mockObjectStore.Object);

        _mockObjectStore.Reset();

        _mockObjectStore.Setup(store => store.Remove("foo")).Returns((string)null);

        ClassicAssert.IsNull(correlationManager.Find("foo"));
    }
}
