using System;
using FleetPay.Acceptance.Test.API.Specs.Support;
using Microsoft.Extensions.Configuration;

namespace FleetPay.Acceptance.Test.API.Specs.Drivers.Integrated
{
    internal class IntegratedEnvironmentConfiguration : IEnvironmentConfigurationDriver
    {
        private readonly IConfiguration _configuration;

        public IntegratedEnvironmentConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        ///     Get Station Locator basic Auth username based on environment name
        /// </summary>
        public string StationLocatorBasicAuthUsername =>
            GetConfigurationValue(Constants.AppSettingProperties.StationLocatorBasicAuthUsernamePropertyName);

        /// <summary>
        ///     Get Station Locator Basic Auth password based on environment name
        /// </summary>
        public string StationLocatorBasicAuthPassword =>
            GetConfigurationValue(Constants.AppSettingProperties.StationLocatorBasicAuthPasswordPropertyName);

        /// <summary>
        ///     Is station locator Basic Auth enabled?
        /// </summary>
        public bool IsStationLocatorBasicAuthEnabled => Convert.ToBoolean(
            GetConfigurationValue(Constants.AppSettingProperties.IsStationLocatorBasicAuthEnabledPropertyName));

        public string Environment
        {
            get
            {
                var environmentName = _configuration.GetSection(Constants.AppSettingProperties.EnvironmentPropertyName)
                    .Value;

                return environmentName ??
                       throw new ArgumentNullException("Cannot get environment value from appsettings.json",
                           nameof(environmentName));
            }
        }

        /// <summary>
        ///     Get Reqres.in API URL from the appsettings.json based on environment name
        /// </summary>
        public string RegistrationApiUrl =>
            GetConfigurationValue(Constants.AppSettingProperties.RegistrationApiUrlPropertyName);

        /// <summary>
        ///     Get Station locator service API URL from the appsettings.json based on environment name
        /// </summary>
        public string StationLocatorApiUrl =>
            GetConfigurationValue(Constants.AppSettingProperties.StationLocatorApiUrlPropertyName);

        /// <summary>
        ///     Get Shell API Gateway URL from the appsettings.json based on environment name
        /// </summary>
        public string ShellApiGatewayUrl =>
            GetConfigurationValue(Constants.AppSettingProperties.ShellApiGatewayUrlPropertyName);

        private string GetConfigurationValue(string propertyName)
        {
            if (GetSpecificEnvironmentSection().GetSection(propertyName).Value != null)
                return GetSpecificEnvironmentSection().GetSection(propertyName).Value;

            if (GetAllEnvironmentSection().GetSection(propertyName).Value != null)
                return GetAllEnvironmentSection().GetSection(propertyName).Value;

            throw new ArgumentNullException(
                $"Cannot get \"{propertyName}\" value from appsettings.json for environment name: {Environment}");
        }

        private IConfigurationSection GetAllEnvironmentSection()
        {
            return GetConfigurationSection(Constants.AppSettingProperties.EnvironmentAllPropertyName);
        }

        private IConfigurationSection GetSpecificEnvironmentSection()
        {
            return GetConfigurationSection(Environment);
        }

        private IConfigurationSection GetConfigurationSection(string environmentName)
        {
            return GetEnvironmentsSection().GetSection(environmentName) ??
                   throw new ArgumentNullException(
                       $"Cannot get {environmentName} environment section from appsettings.json");
        }

        private IConfigurationSection GetEnvironmentsSection()
        {
            var environmentsSection =
                _configuration.GetSection(Constants.AppSettingProperties.EnvironmentsPropertyName);

            return environmentsSection ??
                   throw new ArgumentNullException("Environments section is present in appsettings.json",
                       nameof(environmentsSection));
        }
    }
}