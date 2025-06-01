using Agenix.Api.Exceptions;
using Agenix.Core;
using Agenix.Core.Validation;
using Agenix.Validation.Json.Validation;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Json.Tests;

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
        factory.MessageValidatorRegistry.AddMessageValidator("json", new JsonTextMessageValidator());
        factory.MessageValidatorRegistry.AddMessageValidator("jsonPath", new JsonPathMessageValidator());
        return factory;
    }

    private TestContext CreateTestContext()
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
