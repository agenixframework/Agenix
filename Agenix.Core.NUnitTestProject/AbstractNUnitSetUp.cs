using System;
using Agenix.Core.Exceptions;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject;

public class AbstractNUnitSetUp
{
    protected readonly TestContextFactory TestContextFactory = TestContextFactory.NewInstance();
    protected TestContext Context;

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