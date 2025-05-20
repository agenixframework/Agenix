using Agenix.Api.Exceptions;
using Agenix.Api.Validation;
using Agenix.Core;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.NHamcrest.Tests;

public abstract class AbstractNUnitSetUp
{
    private readonly TestContextFactory _testContextFactory;
    protected TestContext Context;

    public AbstractNUnitSetUp()
    {
        // Initialize the TestContextFactory using the method
        _testContextFactory = CreateTestContextFactory();
    }

    [SetUp]
    public void Setup()
    {
        Context = CreateTestContext();
    }

    [TearDown]
    public void TearDown()
    {
        Context.Clear();
    }

    private TestContextFactory CreateTestContextFactory()
    {
        var factory = TestContextFactory.NewInstance();
        factory.MessageValidatorRegistry.AddMessageValidator("all", new DefaultTextEqualsMessageValidator());
        return factory;
    }

    private TestContext CreateTestContext()
    {
        try
        {
            return _testContextFactory.GetObject();
        }
        catch (Exception e)
        {
            throw new AgenixSystemException("Failed to create test context", e);
        }
    }
}