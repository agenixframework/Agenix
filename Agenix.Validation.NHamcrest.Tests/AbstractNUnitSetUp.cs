using Agenix.Core;
using Agenix.Core.Exceptions;
using TestContext = Agenix.Core.TestContext;

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
        return TestContextFactory.NewInstance();
    }

    private TestContext CreateTestContext()
    {
        try
        {
            return _testContextFactory.GetObject();
        }
        catch (Exception e)
        {
            throw new CoreSystemException("Failed to create test context", e);
        }
    }
}