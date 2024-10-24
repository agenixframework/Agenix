using System.Linq;
using FleetPay.Acceptance.Test.API.Specs.Drivers;
using TechTalk.SpecFlow;

namespace FleetPay.Acceptance.Test.API.Specs.Steps
{
    [Binding]
    internal class VariableStepDefinitions
    {
        private readonly IFleetPayActor _actor;

        public VariableStepDefinitions(FleetPayActor actor)
        {
            _actor = actor;
        }

        [Given(@"variable (.*) is ""(.*)""")]
        public void Variable(string name, string value)
        {
            _actor.Remembers(name).That(value);
        }

        [Given(@"variables")]
        public void Variables(Table table)
        {
            var datTableRows = table.Rows.ToDictionary(r => r[0], r => r[1]);

            foreach (var (name, value) in datTableRows) _actor.Remembers(name).That(value);
        }

        [Then(@"echo ""(.*)""")]
        public void Print(string message)
        {
            _actor.Echo(message);
        }
    }
}
    
