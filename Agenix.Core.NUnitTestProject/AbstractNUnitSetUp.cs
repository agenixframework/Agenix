using System;
using Agenix.Core.Exceptions;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject;

public class AbstractNUnitSetUp
{
    protected readonly TestContextFactory TestContextFactory;
    protected TestContext Context;

    public AbstractNUnitSetUp()
    {
        // Initialize the TestContextFactory using the method
        TestContextFactory = CreateTestContextFactory();
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

    protected virtual TestContextFactory CreateTestContextFactory()
    {
        return TestContextFactory.NewInstance();
    }

    protected TestContext CreateTestContext()
    {
        try
        {
            return TestContextFactory.GetObject();
        }
        catch (Exception e)
        {
            throw new CoreSystemException("Failed to create test context", e);
        }
    }
}