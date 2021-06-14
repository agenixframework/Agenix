using System.Net;
using Boa.Constrictor.RestSharp;
using Boa.Constrictor.Screenplay;
using FleetPay.Acceptance.Test.API.Specs.Drivers;
using FleetPay.Acceptance.Test.API.Specs.Model;
using FleetPay.Acceptance.Test.API.Specs.Support;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp;

namespace FleetPay.Acceptance.Test.API.Specs.Interactions
{
    /// <summary>
    ///     To access the Shell services 3rd parties need to go via the Shell API Platform to access the API end points. All
    ///     Digital Payment Enablement, Station Locator and Fueling API calls will require the Shell API Platform OAuth 2.0
    ///     token.
    /// </summary>
    public class GetShellApiAccessToken : IQuestion<AccessTokenResponse>
    {
        /// <summary>
        ///     After registering your app, you will receive a client ID and a client secret. The client ID is considered public
        ///     information
        /// </summary>
        private readonly string _clientId;

        /// <summary>
        ///     After registering your app, you will receive a client ID and a client secret. The client ID is considered public
        ///     information.
        /// </summary>
        private readonly string _clientSecret;


        /// <summary>
        ///     In OAuth 2.0, the term grant type refers to the way an application gets an access token. OAuth 2.0 defines several
        ///     grant types, including the authorization code flow.
        /// </summary>
        private readonly string _grantType;

        public GetShellApiAccessToken(string grantType, string clientId, string clientSecret)
        {
            _grantType = grantType;
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        public AccessTokenResponse RequestAs(IActor actor)
        {
            var restRequest = new RestRequest(Endpoint.ShellApiGatewayAccessToken.ToString(), Method.POST);
            restRequest.AddParameter("client_id", _clientId, ParameterType.GetOrPost);
            restRequest.AddParameter("client_secret", _clientSecret, ParameterType.GetOrPost);
            restRequest.AddParameter("grant_type", _grantType, ParameterType.GetOrPost);

            var restResponse = actor.Calls(Rest.Request(restRequest));

            ((FleetPayActor) actor).LogLastRequestAndResponse();

            Assert.IsTrue(restResponse.StatusCode == HttpStatusCode.OK,
                "Shell API Gateway access token could not be obtained, Actual Http Status Code: " +
                restResponse.StatusCode);

            return JsonConvert.DeserializeObject<AccessTokenResponse>(restResponse.Content);
        }
    }
} 