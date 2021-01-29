using FluentAssertions;
using MPP.Acceptance.Test.API.Specs.Drivers;
using MPP.Acceptance.Test.API.Specs.Interactions;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MPP.Acceptance.Test.API.Specs.Support;
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
            _scenarioContext[Variables.CurrentCreateUserRequest.ToString()] = table.CreateSet<CreateUserRowObject>();
        }


        [When("the operator attempts to create an user over API")]
        public void WhenTheOperatorAttemptsToCreateAnUserOverApi()
        {
            IEnumerable<CreateUserRowObject> rows = (IEnumerable<CreateUserRowObject>)_scenarioContext[Variables.CurrentCreateUserRequest.ToString()];

            IRestResponse response = _actor.WhoCanCallRegistrationApi().Calls(CreateUser.With(rows.First()));

            _scenarioContext[Variables.CurrentCreateUserResponse.ToString()] = response;
        }

        [Then(@"the operator should see the http response code '(.*)'")]
        public void ThenTheOperatorShouldSeeTheHttpResponseCode(int statusCode)
        {
            IRestResponse response = (IRestResponse)_scenarioContext[Variables.CurrentCreateUserResponse.ToString()];
            response.IsSuccessful.Should().BeTrue();
            response.StatusCode.Should().Be(statusCode);
        }


        [Then("the user should be created successfully")]
        public void ThenTheUserShouldBeCreatedSuccessfully()
        {
            
        }
    }
}
