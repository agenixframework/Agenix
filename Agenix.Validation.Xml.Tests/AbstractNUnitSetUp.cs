using Agenix.Api.Exceptions;
using Agenix.Core;
using Agenix.Core.Validation;
using Agenix.Validation.Xml.Validation.Xhtml;
using Agenix.Validation.Xml.Validation.Xml;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Xml.Tests;

public abstract class AbstractNUnitSetUp
{
    protected TestContext Context;
    protected TestContextFactory TestContextFactory;


    [SetUp]
    public void Setup()
    {
        TestContextFactory = CreateTestContextFactory();
        Context = CreateTestContext();
    }

    [TearDown]
    public void TearDown()
    {
        Context.Clear();
    }

    protected virtual TestContextFactory CreateTestContextFactory()
    {
        var factory = TestContextFactory.NewInstance();
        factory.MessageValidatorRegistry.AddMessageValidator("header", new DefaultMessageHeaderValidator());
        factory.MessageValidatorRegistry.AddMessageValidator("xml", new DomXmlMessageValidator());
        factory.MessageValidatorRegistry.AddMessageValidator("xpath", new XpathMessageValidator());
        factory.MessageValidatorRegistry.AddMessageValidator("xhtml", new XhtmlMessageValidator());
        factory.MessageValidatorRegistry.AddMessageValidator("xhtml-xpath", new XhtmlXpathMessageValidator());
        return factory;
    }

    protected virtual TestContext CreateTestContext()
    {
        try
        {
            return TestContextFactory.GetObject();
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to create test context", e);
        }
    }
}
