using FleetPay.Core;

namespace FleetPay.Acceptance.Test.API.Specs.Drivers
{
    public interface ITestContextDriver
    {
        public TestContext GetTestContext { get; set; }
    }
}