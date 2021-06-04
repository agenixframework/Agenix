namespace MPP.Acceptance.Test.API.Specs.Interactions.dsl
{
    public class AccessTokenRequestBuilder
    {
        private string _clientSecret;
        private string _grantType = "client_credentials";

        public AccessTokenRequestBuilder UsingGrantType(string grantType)
        {
            _grantType = grantType;
            return this;
        }

        public AccessTokenRequestBuilder UsingClientSecret(string clientSecret)
        {
            _clientSecret = clientSecret;
            return this;
        }

        public GetShellApiAccessToken ForClientId(string clientId)
        {
            return new(_grantType, _clientSecret, clientId);
        }
    }
}