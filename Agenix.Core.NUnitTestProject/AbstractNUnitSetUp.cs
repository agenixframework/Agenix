using System;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;

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
            throw new AgenixSystemException("Failed to create test context", e);
        }
    }
}