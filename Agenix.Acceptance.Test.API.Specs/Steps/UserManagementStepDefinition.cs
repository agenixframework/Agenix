using System.Collections.Generic;
using System.Linq;
using FleetPay.Acceptance.Test.API.Specs.Drivers;
using FleetPay.Acceptance.Test.API.Specs.Interactions;
using FleetPay.Acceptance.Test.API.Specs.Model;
using FleetPay.Acceptance.Test.API.Specs.Support;
using FluentAssertions;
using RestSharp;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace FleetPay.Acceptance.Test.API.Specs.Steps
{
    [Binding]
    public sealed class UserManagementStepDefinition
    {
        private readonly IFleetPayActor _actor;
        private readonly ScenarioContext _scenarioContext;

        public UserManagementStepDefinition(ScenarioContext scenarioContext, IFleetPayActor actor)
        {
            _scenarioContext = scenarioContext;
            _actor = actor;
        }

        [Given("the following user list")]
        public void GivenTheFollowingUserList(Table table)
        {
            _actor.Remembers(InMemory.CURRENT_SENT_REQUESTS).That(table.CreateSet<CreateUserRowObject>());
        }

        [When("the operator attempts to create an user over API")]
        public void WhenTheOperatorAttemptsToCreateAnUserOverApi()
        {
            var rows = _actor.Recall<IEnumerable<CreateUserRowObject>>(InMemory.CURRENT_SENT_REQUESTS);

            var response = _actor.WhoCanCallRegistrationApi.Calls(CreateUser.With(rows.First()));

            _actor.Remembers(InMemory.CURRENT_RECEIVED_RESPONSES).That(response);
        }

        [When(@"the (operator|participant) attempts to retrieve the user details by Id ""(.*)"" over API")]
        public void WhenTheOperatorAttemptsToRetrieveTheUserDetailsByIdOverApi(string actor, string userId)
        {
            var response = _actor.WhoCanCallRegistrationApi.AsksFor(UserDetails.ForId(userId));

            _actor.Remembers(InMemory.CURRENT_RECEIVED_RESPONSES).That(response);
        }


        [Then(@"the operator should see the http response code '(.*)'")]
        public void ThenTheOperatorShouldSeeTheHttpResponseCode(int statusCode)
        {
            var response = _actor.Recall<IRestResponse>(InMemory.CURRENT_RECEIVED_RESPONSES);

            response.IsSuccessful.Should().BeTrue();
            response.StatusCode.Should().Be(statusCode);
        }

        [Then("the user should be created successfully")]
        public void ThenTheUserShouldBeCreatedSuccessfully()
        {
        }
    }
}