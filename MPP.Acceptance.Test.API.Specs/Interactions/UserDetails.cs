using Boa.Constrictor.RestSharp;
using Boa.Constrictor.Screenplay;
using MPP.Acceptance.Test.API.Specs.Drivers;
using MPP.Acceptance.Test.API.Specs.Support;
using RestSharp;

namespace MPP.Acceptance.Test.API.Specs.Interactions
{
    public class UserDetails : IQuestion<IRestResponse>
    {
        private readonly string _userId;

        public UserDetails(string userId)
        {
            _userId = userId;
        }

        public IRestResponse RequestAs(IActor actor)
        {
            var resolvedResource = ((MPPActor) actor).GeTestContextDriver().GetTestContext
                .ReplaceDynamicContentInString($"{Endpoint.Users}/{_userId}");

            var restRequest = new RestRequest(resolvedResource, Method.GET);
            restRequest.AddHeader(ContentType.AcceptName, ContentType.Json.ToString());

            var response = actor.Calls(Rest.Request(restRequest));

            ((MPPActor) actor).LogLastRequestAndResponse();

            return response;
        }

        public static UserDetails ForId(string id)
        {
            return new(id);
        }
    }
}