using FleetPay.Acceptance.Test.API.Specs.Drivers;
using FleetPay.Acceptance.Test.API.Specs.Interactions;
using TechTalk.SpecFlow;

namespace FleetPay.Acceptance.Test.API.Specs.Steps
{
    [Binding]
    public sealed class CalculatorStepDefinitions
    {
        private readonly IEnvironmentConfigurationDriver _environmentConfigurationDriver;

        private readonly IFleetPayActor _fleetPayActor;

        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;

        public CalculatorStepDefinitions(ScenarioContext scenarioContext,
            IEnvironmentConfigurationDriver environmentConfigurationDriver, IFleetPayActor fleetPayActor)
        {
            _scenarioContext = scenarioContext;
            _environmentConfigurationDriver = environmentConfigurationDriver;
            _fleetPayActor = fleetPayActor;
        }

        [Given("the first number is (.*)")]
        public void GivenTheFirstNumberIs(int number)
        {
            _fleetPayActor.WhoCanCallRegistrationApi.AttemptsTo(ExecuteSampleTask.With());
        }

        [Given("the second number is (.*)")]
        public void GivenTheSecondNumberIs(int number)
        {
            //TODO: implement arrange (precondition) logic
            // For storing and retrieving scenario-specific data see https://go.specflow.org/doc-sharingdata
            // To use the multiline text or the table argument of the scenario,
            // additional string/Table parameters can be defined on the step definition
            // method. 
        }

        [When("the two numbers are added")]
        public void WhenTheTwoNumbersAreAdded()
        {
            //TODO: implement act (action) logic
        }

        [Then("the result should be (.*)")]
        public void ThenTheResultShouldBe(int result)
        {
            //TODO: implement assert (verification) logic
        }
    }
}