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
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Message.Selector;

public class HeaderMatchingMessageSelectorTest : AbstractNUnitSetUp
{
    [Test]
    public void TestHeaderMatchingSelector()
    {
        var messageSelector = new HeaderMatchingMessageSelector("operation", "foo", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foobar");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }

    [Test]
    public void TestHeaderMatchingSelectorValidationMatcher()
    {
        var messageSelector = new HeaderMatchingMessageSelector("operation", "@Contains(foo)@", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "barfoobar");

        var declineMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "bar");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }

    [Test]
    public void TestHeaderMatchingSelectorMultipleValues()
    {
        var messageSelector = new HeaderMatchingMessageSelector("foo", "bar", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foo");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }

    [Test]
    public void TestHeaderMatchingSelectorMissingHeader()
    {
        var messageSelector = new HeaderMatchingMessageSelector("operation", "foo", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("FooTest");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }

    [Test]
    public void TestHeaderMatchingSelectorWithMessageObjectPayload()
    {
        var messageSelector = new HeaderMatchingMessageSelector("operation", "foo", Context);

        var acceptMessage = new DefaultMessage(new DefaultMessage("FooTest")
            .SetHeader("operation", "foo"));

        var declineMessage = new DefaultMessage(new DefaultMessage("FooTest")
            .SetHeader("operation", "foobar"));

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));

        messageSelector = new HeaderMatchingMessageSelector(MessageHeaders.Id, acceptMessage.Id, Context);

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }
}
