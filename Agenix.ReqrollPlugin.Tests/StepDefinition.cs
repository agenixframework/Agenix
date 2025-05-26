using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Log;
using Agenix.Core.Actions;
using Microsoft.Extensions.Logging;
using Reqnroll;

namespace Agenix.Reqroll.Plugin.Tests
{
    [Binding]
    public sealed class StepDefinition
    {
        [AgenixResource] private ITestCaseRunner _testCaseRunner;
        
        private static readonly ILogger Log = LogManager.GetLogger(typeof(Hooks));
        
        [Given("I have entered (.*) into the calculator")]
        public void GivenIHaveEnteredSomethingIntoTheCalculator(int number)
        {
            _testCaseRunner.Given(EchoAction.Builder.Echo(number.ToString()));
        }

        [When("I press add")]
        public void WhenIPressAdd()
        {
           
        }

        [Then("the result should be (.*) on the screen")]
        public void ThenTheResultShouldBe(int result)
        {
          
        }

        [Then(@"I execute failed step")]
        public void ThenIExecuteFailedTest()
        {
            throw new Exception("This step raises an exception.");
        }

    }
}
