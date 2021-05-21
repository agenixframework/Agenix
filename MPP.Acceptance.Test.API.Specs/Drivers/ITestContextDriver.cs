using MPP.Core;

namespace MPP.Acceptance.Test.API.Specs.Drivers
{
    public interface ITestContextDriver
    {
        public TestContext GetTestContext { get; set; }
    }
}