using MPP.Core.Exceptions;
using NUnit.Framework;
using System;
using MPP.Core.Session;

namespace MPP.Core.NUnitTestProject
{
    public class AbstractNUnitSetUp
    {
        protected TestContext Context;

        protected TestContextFactory TestContextFactory = TestContextFactory.NewInstance();

        [SetUp]
        public void Setup()
        {
            Context = CreateTestContext();
        }

        [TearDown]
        public void TearDown()
        {
            ObjectBag.ClearCurrentSession();
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
}