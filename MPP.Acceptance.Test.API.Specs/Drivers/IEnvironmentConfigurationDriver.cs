namespace MPP.Acceptance.Test.API.Specs.Drivers
{
    public interface IEnvironmentConfigurationDriver
    {
        string RegistrationApiUrl { get; }

        string StationLocatorApiUrl { get; }

        string ShellApiGatewayUrl { get; }

        string Environment { get; }
    }
}