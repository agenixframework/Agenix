using System.IO;
using BoDi;
using Microsoft.Extensions.Configuration;
using MPP.Acceptance.Test.API.Specs.Drivers;
using MPP.Acceptance.Test.API.Specs.Drivers.Integrated;
using MPP.Acceptance.Test.API.Specs.Support.Matchers;
using MPP.Core;
using TechTalk.SpecFlow;
using TechTalk.SpecRun;

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
                .AddJsonFile(Path.Combine(_testRunContext.TestDirectory, "appsettings.json"), true, true)
                .Build();

            var testContext = TestContextFactory.NewInstance().GetObject();

            RegisterCustomValidationMatchers(testContext);

            objectContainer.RegisterInstanceAs(config);
            objectContainer.RegisterTypeAs<IntegratedEnvironmentConfiguration, IEnvironmentConfigurationDriver>();
            objectContainer.RegisterTypeAs<IntegratedTestContext, ITestContextDriver>();


            IMPPActor actor = new MPPActor(new IntegratedEnvironmentConfiguration(config),
                new IntegratedTestContext(testContext));
            objectContainer.RegisterInstanceAs(actor);
        }

        /// <summary>
        ///     Register custom validation matchers
        /// </summary>
        /// <param name="testContext"></param>
        private static void RegisterCustomValidationMatchers(TestContext testContext)
        {
            testContext.ValidationMatcherRegistry.GetLibraryForPrefix("").Members
                .Add("AssertThat", new NullValueMatcher());
            testContext.ValidationMatcherRegistry.GetLibraryForPrefix("").Members
                .Add("ContainsItem", new ContainsItemMatcher());
            testContext.ValidationMatcherRegistry.GetLibraryForPrefix("").Members
                .Add("HasItems", new HasItemsMatcher());
            testContext.ValidationMatcherRegistry.GetLibraryForPrefix("").Members
                .Add("HasItemsInAnyOrder", new HasItemsInAnyOrderMatcher());
            testContext.ValidationMatcherRegistry.GetLibraryForPrefix("").Members
                .Add("EqualsTo", new EqualsToMatcher());

        }
    }
}