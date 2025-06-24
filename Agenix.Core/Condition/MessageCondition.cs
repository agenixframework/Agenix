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

using Agenix.Api.Context;

namespace Agenix.Core.Condition;

/// Condition that verifies if a specific message is present in the test context message store. Messages are
/// automatically stored when sending and receiving messages with corresponding test actions. This condition
/// can be used to await the arrival or dispatch of a message.
/// The message is identified by its name as stored in the message store.
/// /
public class MessageCondition : AbstractCondition
{
    /// The name of the message used to identify it within the message store.
    /// /
    private string _messageName;

    /// Evaluates whether the condition is satisfied based on the provided test context.
    /// <param name="context">The test context used for evaluating the condition.</param>
    /// <return>true if the condition is satisfied; otherwise, false.</return>
    /// /
    public override bool IsSatisfied(TestContext context)
    {
        return context.MessageStore.GetMessage(context.ReplaceDynamicContentInString(_messageName)) != null;
    }

    /// Constructs and returns a success message indicating that a specified message was found in the message store.
    /// <param name="context">
    ///     The test context that provides the necessary environment for message evaluation and dynamic
    ///     content replacement.
    /// </param>
    /// <return>A success message string that includes the found message's name after performing dynamic content replacements.</return>
    public override string GetSuccessMessage(TestContext context)
    {
        return
            $"Message condition success - found message '{context.ReplaceDynamicContentInString(_messageName)}' in message store";
    }

    /// Generates an error message indicating that the message condition has failed.
    /// <param name="context">The test context used to evaluate the message condition.</param>
    /// <return>A string containing the formatted error message indicating the failure due to a missing message in the store.</return>
    public override string GetErrorMessage(TestContext context)
    {
        return
            $"Message condition failed - unable to find message '{context.ReplaceDynamicContentInString(_messageName)}' in message store";
    }

    /// Sets the name of the message that should be present in the message store.
    /// <param name="msgName">The message name to set.</param>
    public void SetMessageName(string msgName)
    {
        _messageName = msgName;
    }

    /// Retrieves the name of the message that is expected to be present in the message store.
    /// <return>The message name.</return>
    public string GetMessageName()
    {
        return _messageName;
    }

    public override string ToString()
    {
        return "MessageCondition{" +
               "messageName='" + _messageName + '\'' +
               ", name='" + GetName() + '\'' +
               '}';
    }
}
