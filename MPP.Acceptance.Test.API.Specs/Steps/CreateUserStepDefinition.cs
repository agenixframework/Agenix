using FluentAssertions;
using MPP.Acceptance.Test.API.Specs.Drivers;
using MPP.Acceptance.Test.API.Specs.Interactions;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace MPP.Acceptance.Test.API.Specs.Steps
{
    [Binding]
    public sealed class CreateUserStepDefinition
    {
        private readonly ScenarioContext _scenarioContext;

        private readonly IMPPActor _actor;

        public CreateUserStepDefinition(ScenarioContext scenarioContext, IMPPActor actor)
        {
            _scenarioContext = scenarioContext;
            _actor = actor;
        }

        [Given("the following user list")]
        public void GivenTheFollowingUserList(Table table)
        {
            _scenarioContext["CurrentCreateUserRequest"] = table.CreateSet<CreateUserRowObject>();
        }


        [When("When the operator attempts to create an user over API")]
        public void WhenTheOperatorAttemptsToCreateAnUserOverApi()
        {
            IEnumerable<CreateUserRowObject> rows = (IEnumerable<CreateUserRowObject>)_scenarioContext["CurrentCreateUserRequest"];

            IRestResponse response = _actor.WhoCanCallRegistrationAPI().Calls(CreateUser.With(rows.First()));

            _scenarioContext["CurrentCreateUserResponse"] = response;
        }

        [Then("the user should be created successfully")]
        public void ThenTheUserShouldBeCreatedSuccessfully()
        {
            IRestResponse response = (IRestResponse)_scenarioContext["CurrentCreateUserResponse"];

            response.IsSuccessful.Should().BeTrue();
        }
    }
}
