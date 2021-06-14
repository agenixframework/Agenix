namespace FleetPay.Acceptance.Test.API.Specs.Support
{
    public sealed class Endpoint
    {
        public static readonly Endpoint Users = new(1, "/api/users");
        public static readonly Endpoint Register = new(2, "/api/register");
        public static readonly Endpoint Login = new(2, "/api/login");
        public static readonly Endpoint Unknown = new(2, "/api/unknown");
        public static readonly Endpoint StationLocator = new(2, "/api/stations");
        public static readonly Endpoint ShellApiGatewayAccessToken = new(2, "/oauth/token");
        private readonly string _name;
        private readonly int _value;

        private Endpoint(int value, string name)
        {
            _name = name;
            _value = value;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}