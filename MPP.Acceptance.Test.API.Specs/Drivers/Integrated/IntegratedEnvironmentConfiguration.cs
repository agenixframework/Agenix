using Microsoft.Extensions.Configuration;
using System;

namespace MPP.Acceptance.Test.API.Specs.Drivers.Integrated
{
    class IntegratedEnvironmentConfiguration : IEnvironmentConfigurationDriver
    {
        private readonly IConfiguration _configuration;

        public IntegratedEnvironmentConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public String Environment
        {
            get
            {
                var environmentName = _configuration.GetSection("environment").Value;

                return environmentName ?? throw new ArgumentNullException("Cannot get environment value from appsettings.json", nameof(environmentName));
            }
        }

        public string RegistrationAPIUrl => GetSpecificEnvironmentSection().GetSection("RegistrationAPIUrl").Value ??
                throw new ArgumentNullException("Cannot get AppUrl value from appsettings.json for environment name: " + Environment);

        private IConfigurationSection GetSpecificEnvironmentSection()
        {
            return GetEnvironmentsSection().GetSection(Environment) ?? 
                throw new ArgumentNullException("Cannot get specific environment section from appsettings.json");
        }

        private IConfigurationSection GetEnvironmentsSection()
        {
            var environmentsSection = _configuration.GetSection("environments");

            return environmentsSection ?? throw new ArgumentNullException("Environments section is present in appsettings.json", nameof(environmentsSection));
        }
    }
}
