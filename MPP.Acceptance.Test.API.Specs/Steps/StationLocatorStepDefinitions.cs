using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MPP.Acceptance.Test.API.Specs.Drivers;
using MPP.Acceptance.Test.API.Specs.Interactions;
using MPP.Acceptance.Test.API.Specs.Model;
using MPP.Acceptance.Test.API.Specs.Support;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace MPP.Acceptance.Test.API.Specs.Steps
{
    [Binding]
    public sealed class StationLocatorStepDefinitions
    {
        private readonly IMPPActor _actor;
        private readonly ScenarioContext _scenarioContext;

        public StationLocatorStepDefinitions(ScenarioContext scenarioContext, IMPPActor actor)
        {
            _scenarioContext = scenarioContext;
            _actor = actor;
        }

        [Given(@"FleetPay mobile app attempts to retrieve the shell petrol stations over API using the query parameters:")]
        public void GivenFleetPayMobileAppAttemptsToRetrieveTheShellPetrolStationsOverApiUsingTheQueryParameters(Table table)
        {
            var shellStations = table.CreateSet<ShellStationRowObject>();

            var shellStationRestResponse = _actor.WhoCanCallStationLocatorApi()
                .AsksFor(ShellPetrolStations.UsingQueryParameters(shellStations.First()));

             _actor.Remembers(InMemory.CurrentReceivedResponses).That(shellStationRestResponse);
        }

    }
}
