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

using Agenix.Api.Message;
using Agenix.Core.Message;
using Agenix.Core.Message.Selector;
using Agenix.Core.Util;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Message;

public class DefaultMessageQueueTest
{
    private TestContext _context;

    [SetUp]
    public void SetupMocks()
    {
        _context = new TestContext();
    }

    [Test]
    public void TestReceiveSelected()
    {
        var queue = new DefaultMessageQueue("testQueue");
        queue.SetPollingInterval(100L);

        queue.Send(new DefaultMessage("FooMessage").SetHeader("foo", "bar"));

        var selector = new HeaderMatchingMessageSelector("foo", "bar", _context);

        var receivedMessage = queue.Receive(message => selector.Accept(message), 1000L);

        ClassicAssert.AreEqual(receivedMessage.GetPayload<string>(), "FooMessage");
        ClassicAssert.AreEqual(receivedMessage.GetHeaders()["foo"], "bar");
    }

    [Test]
    public void TestWithRetry()
    {
        var queue = new DefaultMessageQueue("testQueue");
        queue.SetPollingInterval(100L);

        queue.Send(new DefaultMessage("FooMessage").SetHeader("foo", "bar"));

        var retries = new AtomicLong();
        var selector = new CustomHeaderMatchingMessageSelectorGreaterThan7("foo", "bar", _context, retries);

        var receivedMessage = queue.Receive(message => selector.Accept(message), 1000L);

        ClassicAssert.AreEqual(receivedMessage.GetPayload<string>(), "FooMessage");
        ClassicAssert.AreEqual(receivedMessage.GetHeaders()["foo"], "bar");
        ClassicAssert.AreEqual(retries.Get(), 8L);
    }

    [Test]
    public void TestRetryExceeded()
    {
        var queue = new DefaultMessageQueue("testQueue");
        queue.SetPollingInterval(500L);

        queue.Send(new DefaultMessage("FooMessage").SetHeader("foos", "bars"));

        var retries = new AtomicLong();
        var selector = new CustomHeaderMatchingMessageSelector("foo", "bar", _context, retries);

        var receivedMessage = queue.Receive(message => selector.Accept(message), 1000L);

        ClassicAssert.IsNull(receivedMessage);
        ClassicAssert.AreEqual(retries.Get(), 3L);
    }

    [Test]
    public void TestRetryExceededWithTimeoutRest()
    {
        var queue = new DefaultMessageQueue("testQueue");
        queue.SetPollingInterval(400L);

        queue.Send(new DefaultMessage("FooMessage").SetHeader("foos", "bars"));

        var retries = new AtomicLong();
        var selector = new CustomHeaderMatchingMessageSelector("foo", "bar", _context, retries);

        var receivedMessage = queue.Receive(message => selector.Accept(message), 1000L);

        ClassicAssert.IsNull(receivedMessage);
        ClassicAssert.AreEqual(retries.Get(), 4L);
    }

    private class CustomHeaderMatchingMessageSelector(
        string headerName,
        string headerValue,
        TestContext context,
        AtomicLong retries)
        : HeaderMatchingMessageSelector(headerName, headerValue, context)
    {
        public override bool Accept(IMessage message)
        {
            retries.IncrementAndGet();
            return base.Accept(message);
        }
    }

    private class CustomHeaderMatchingMessageSelectorGreaterThan7(
        string headerName,
        string headerValue,
        TestContext context,
        AtomicLong retries)
        : HeaderMatchingMessageSelector(headerName, headerValue, context)
    {
        public override bool Accept(IMessage message)
        {
            return retries.IncrementAndGet() > 7;
        }
    }
}
