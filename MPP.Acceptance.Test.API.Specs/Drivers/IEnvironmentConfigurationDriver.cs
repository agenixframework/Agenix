namespace MPP.Acceptance.Test.API.Specs.Drivers
{
    public interface IEnvironmentConfigurationDriver
    {
        string RegistrationAPIUrl { get; }

        string Environment { get; }
    }
}
