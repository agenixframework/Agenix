using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPP.Acceptance.Test.API.Specs.Drivers;
using MPP.Acceptance.Test.API.Specs.Support;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace MPP.Acceptance.Test.API.Specs.Steps
{
    [Binding]
    class VariableStepDefinitions
    {
        private readonly IMPPActor _actor;

        public VariableStepDefinitions(IMPPActor actor)
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
            var datTableRows= table.Rows.ToDictionary(r => r[0], r => r[1]);

            foreach (var (name, value)  in datTableRows)
            {
                _actor.Remembers(name).That(value);
            }
        }

        [Then(@"echo ""(.*)""")]
        public void Print(string message)
        {
            _actor.Echo(message);
        }
    }
}
