using System.IO;
using BoDi;
using Microsoft.Extensions.Configuration;
using TechTalk.SpecFlow;
using TechTalk.SpecRun;
using MPP.Acceptance.Test.API.Specs.Drivers;
using MPP.Acceptance.Test.API.Specs.Drivers.Integrated;

namespace MPP.Acceptance.Test.API.Specs.Hooks
{
    [Binding]
    public sealed class DIConfiguration
    {
        private readonly TestRunContext _testRunContext;

        public DIConfiguration(TestRunContext testRunContext)
        {
            _testRunContext = testRunContext;
        }


        [BeforeScenario(Order = 1)]
        public void RegisterDependencies(IObjectContainer objectContainer)
        {
            IConfiguration config = new ConfigurationBuilder()
               .AddJsonFile(Path.Combine(_testRunContext.TestDirectory, "appsettings.json"), optional: true, reloadOnChange: true)
               .Build();

            objectContainer.RegisterInstanceAs(config);
            objectContainer.RegisterTypeAs<IntegratedEnvironmentConfiguration, IEnvironmentConfigurationDriver>();


            IMPPActor actor = new MPPActor(new IntegratedEnvironmentConfiguration(config));
            objectContainer.RegisterInstanceAs(actor);

        }

    }
}
