using System.Linq;
using FleetPay.Acceptance.Test.API.Specs.Drivers;
using FleetPay.Acceptance.Test.API.Specs.Interactions;
using FleetPay.Acceptance.Test.API.Specs.Model;
using FleetPay.Acceptance.Test.API.Specs.Support;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace FleetPay.Acceptance.Test.API.Specs.Steps
{
    [Binding]
    public sealed class StationLocatorStepDefinitions
    {
        private readonly IFleetPayActor _actor;
        private readonly ScenarioContext _scenarioContext;

        public StationLocatorStepDefinitions(ScenarioContext scenarioContext, IFleetPayActor actor)
        {
            _scenarioContext = scenarioContext;
            _actor = actor;
        }

        [Given(
            @"FleetPay mobile app attempts to retrieve the shell petrol stations over API using the query parameters:")]
        public void GivenFleetPayMobileAppAttemptsToRetrieveTheShellPetrolStationsOverApiUsingTheQueryParameters(
            Table table)
        {
            var shellStations = table.CreateSet<ShellStationRowObject>();

            var shellStationRestResponse = _actor.WhoCanCallStationLocatorApi
                .AsksFor(ShellPetrolStations.UsingQueryParameters(shellStations.First()));

            _actor.Remembers(InMemory.CURRENT_RECEIVED_RESPONSES).That(shellStationRestResponse);
        }
    }
}