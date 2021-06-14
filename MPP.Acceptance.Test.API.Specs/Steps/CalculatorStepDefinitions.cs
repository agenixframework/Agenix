using MPP.Acceptance.Test.API.Specs.Drivers;
using MPP.Acceptance.Test.API.Specs.Interactions;
using TechTalk.SpecFlow;

namespace MPP.Acceptance.Test.API.Specs.Steps
{
    [Binding]
    public sealed class CalculatorStepDefinitions
    {
        private readonly IEnvironmentConfigurationDriver _environmentConfigurationDriver;

        private readonly IMPPActor _mppActor;

        // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

        private readonly ScenarioContext _scenarioContext;

        public CalculatorStepDefinitions(ScenarioContext scenarioContext,
            IEnvironmentConfigurationDriver environmentConfigurationDriver, IMPPActor mppActor)
        {
            _scenarioContext = scenarioContext;
            _environmentConfigurationDriver = environmentConfigurationDriver;
            _mppActor = mppActor;
        }

        [Given("the first number is (.*)")]
        public void GivenTheFirstNumberIs(int number)
        {
            _mppActor.WhoCanCallRegistrationApi.AttemptsTo(ExecuteSampleTask.With());
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