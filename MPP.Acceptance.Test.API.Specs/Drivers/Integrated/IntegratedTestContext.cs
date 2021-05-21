using MPP.Core;

namespace MPP.Acceptance.Test.API.Specs.Drivers.Integrated
{
    public class IntegratedTestContext : ITestContextDriver
    {
        public IntegratedTestContext(TestContext testContext)
        {
            GetTestContext = testContext;
        }

        public TestContext GetTestContext { get; set; }
    }
}