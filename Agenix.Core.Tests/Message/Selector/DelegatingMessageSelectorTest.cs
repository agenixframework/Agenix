using System.Collections.Generic;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Core.Message;
using Agenix.Core.Message.Selector;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Core.Tests.Message.Selector;

public class DelegatingMessageSelectorTest : AbstractNUnitSetUp
{
    private readonly IReferenceResolver _resolver = new Mock<IReferenceResolver>().Object;

    [Test]
    public void TestHeaderMatchingSelector()
    {
        var messageSelector = new DelegatingMessageSelector("operation = 'foo'", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foobar");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }

    [Test]
    public void TestHeaderMatchingSelectorAndOperation()
    {
        var messageSelector = new DelegatingMessageSelector("foo = 'bar' AND operation = 'foo'", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("FooTest")
            .SetHeader("operation", "foo");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }

    [Test]
    public void TestPayloadMatchingDelegation()
    {
        var messageSelector = new DelegatingMessageSelector("foo = 'bar' AND payload = 'FooTest'", Context);

        var acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("BarTest")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));

        messageSelector = new DelegatingMessageSelector("payload = 'FooTest'", Context);

        acceptMessage = new DefaultMessage("FooTest")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        declineMessage = new DefaultMessage("BarTest")
            .SetHeader("operation", "foo");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }


    [Test]
    public void TestPayloadAndHeaderMatchingDelegation()
    {
        var messageSelector = new DelegatingMessageSelector("header:payload = 'foo' AND payload = 'foo'", Context);

        ClassicAssert.IsTrue(messageSelector.Accept(new DefaultMessage("foo")
            .SetHeader("payload", "foo")));

        ClassicAssert.IsFalse(messageSelector.Accept(new DefaultMessage("foo")
            .SetHeader("payload", "bar")));

        ClassicAssert.IsFalse(messageSelector.Accept(new DefaultMessage("bar")
            .SetHeader("payload", "foo")));
    }

    [Test]
    public void TestCustomMessageSelectorDelegation()
    {
        var factories = new Dictionary<string, IMessageSelector.IMessageSelectorFactory>
        {
            { "customSelectorFactory", new CustomMessageSelectorFactory() }
        };

        Mock.Get(_resolver).Setup(r => r.ResolveAll<IMessageSelector.IMessageSelectorFactory>()).Returns(factories);

        Context.SetReferenceResolver(_resolver);
        var messageSelector = new DelegatingMessageSelector("x:foo = 'bar'", Context);

        var acceptMessage = new DefaultMessage("FooBar")
            .SetHeader("foo", "bar")
            .SetHeader("operation", "foo");

        var declineMessage = new DefaultMessage("FooBar")
            .SetHeader("foo", "bars")
            .SetHeader("operation", "foo");

        ClassicAssert.IsTrue(messageSelector.Accept(acceptMessage));
        ClassicAssert.IsFalse(messageSelector.Accept(declineMessage));
    }

    private class CustomMessageSelectorFactory : IMessageSelector.IMessageSelectorFactory
    {
        public bool Supports(string key)
        {
            return key.StartsWith("x:");
        }

        /// <summary>
        ///     Factory method to create an instance of <see cref="CustomerMessageSelector" /> based on provided parameters.
        /// </summary>
        /// <param name="key">The configuration key.</param>
        /// <param name="value">The configuration value.</param>
        /// <param name="context">The test context.</param>
        /// <returns>An instance of <see cref="CustomerMessageSelector" />.</returns>
        public IMessageSelector Create(string key, string value, TestContext context)
        {
            return new CustomerMessageSelector(value);
        }
    }

    /// <summary>
    ///     A message selector implementation that checks if a message satisfies a specific condition.
    /// </summary>
    private class CustomerMessageSelector(string value) : IMessageSelector
    {
        public bool Accept(IMessage message)
        {
            return message.GetHeaders()["foo"].Equals(value);
        }
    }
}
