#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
