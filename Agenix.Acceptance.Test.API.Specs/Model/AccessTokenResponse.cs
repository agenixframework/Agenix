using System.Runtime.Serialization;

namespace FleetPay.Acceptance.Test.API.Specs.Model
{
    [DataContract]
    public class AccessTokenResponse
    {
        [DataMember(Name = "access_token")] public string AccessToken { get; set; }

        [DataMember(Name = "expires_in")] public string ExpiresIn { get; set; }

        [DataMember(Name = "token_type")] public string TokenType { get; set; }
    }
}