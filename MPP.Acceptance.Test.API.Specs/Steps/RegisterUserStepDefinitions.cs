using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MPP.Acceptance.Test.API.Specs.Drivers;
using MPP.Acceptance.Test.API.Specs.Interactions;
using MPP.Acceptance.Test.API.Specs.Model;
using MPP.Acceptance.Test.API.Specs.Support;
using RestSharp;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace MPP.Acceptance.Test.API.Specs.Steps
{
    [Binding]
    public class RegisterUserStepDefinitions
    {
        private readonly IMPPActor _actor;

        public RegisterUserStepDefinitions(IMPPActor actor)
        {
            _actor = actor;
        }

        [Given(@"the following user details")]
        public void GivenTheFollowingUserDetails(Table table)
        {
            _actor.Remembers(InMemory.CURRENT_SENT_REQUESTS).That(table.CreateSet<RegisterUserRowObject>());
        }

        [When(@"operator registers the user over API")]
        public void WhenOperatorRegistersTheUserOverApi()
        {
            var rows = _actor.Recall<IEnumerable<RegisterUserRowObject>>(InMemory.CURRENT_SENT_REQUESTS);

            var response = _actor.WhoCanCallRegistrationApi.Calls(RegisterUser.With(rows.First()));

            _actor.Remembers(InMemory.CURRENT_RECEIVED_RESPONSES).That(response);
        }

        [Then(@"operator receives (.*) response code")]
        public void ThenOperatorReceivesResponseCode(int statusCode)
        {
            var response = _actor.Recall<IRestResponse>(InMemory.CURRENT_RECEIVED_RESPONSES);
            response.StatusCode.Should().Be(statusCode);
        }

        [Then(@"user is registered successfully")]
        public void ThenUserIsRegisteredSuccessfully()
        {
        }
    }
}