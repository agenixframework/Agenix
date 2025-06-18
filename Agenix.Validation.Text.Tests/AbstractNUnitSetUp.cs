using Agenix.Api.Exceptions;
using Agenix.Core;
using Agenix.Core.Validation;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Text.Tests;

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
