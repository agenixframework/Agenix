using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MPP.Acceptance.Test.API.Specs.Drivers;
using MPP.Acceptance.Test.API.Specs.Interactions;
using MPP.Acceptance.Test.API.Specs.Support;
using RestSharp;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace MPP.Acceptance.Test.API.Specs.Steps
{
    [Binding]
    public sealed class CreateUserStepDefinition
    {
        private readonly IMPPActor _actor;
        private readonly ScenarioContext _scenarioContext;

        public CreateUserStepDefinition(ScenarioContext scenarioContext, IMPPActor actor)
        {
            _scenarioContext = scenarioContext;
            _actor = actor;
        }

        [Given("the following user list")]
        public void GivenTheFollowingUserList(Table table)
        {
            _actor.Remembers(InMemory.CurrentCreateUserRequest, table.CreateSet<CreateUserRowObject>());
        }


        [When("the operator attempts to create an user over API")]
        public void WhenTheOperatorAttemptsToCreateAnUserOverApi()
        {
            var rows = _actor.Recall<IEnumerable<CreateUserRowObject>>(InMemory.CurrentCreateUserRequest);

            var response = _actor.WhoCanCallRegistrationApi().Calls(CreateUser.With(rows.First()));

            _actor.Remembers(InMemory.CurrentCreateUserResponse, response);
        }

        [Then(@"the operator should see the http response code '(.*)'")]
        public void ThenTheOperatorShouldSeeTheHttpResponseCode(int statusCode)
        {
            var response = _actor.Recall<IRestResponse>(InMemory.CurrentCreateUserResponse);

            response.IsSuccessful.Should().BeTrue();
            response.StatusCode.Should().Be(statusCode);
        }


        [Then("the user should be created successfully")]
        public void ThenTheUserShouldBeCreatedSuccessfully()
        {
        }
    }
}