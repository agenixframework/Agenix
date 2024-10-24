using Boa.Constrictor.RestSharp;
using Boa.Constrictor.Screenplay;
using FleetPay.Acceptance.Test.API.Specs.Drivers;
using FleetPay.Acceptance.Test.API.Specs.Model;
using FleetPay.Acceptance.Test.API.Specs.Support;
using Newtonsoft.Json;
using RestSharp;

namespace FleetPay.Acceptance.Test.API.Specs.Interactions
{
    public class CreateUser : IQuestion<IRestResponse>
    {
        private readonly CreateUserRowObject _createUserRowObject;

        private CreateUser(CreateUserRowObject createUserRowObject)
        {
            _createUserRowObject = createUserRowObject;
        }

        public IRestResponse RequestAs(IActor actor)
        {
            var response = actor.Calls(Rest.Request(CreateUserRestRequest((FleetPayActor) actor)));

            ((FleetPayActor) actor).LogLastRequestAndResponse();

            return response;
        }

        public static CreateUser With(CreateUserRowObject createUserRowObject)
        {
            return new(createUserRowObject);
        }

        private RestRequest CreateUserRestRequest(IFleetPayActor actor)
        {
            var createUserRequest = CreateUserRequestFromRowObject(_createUserRowObject, actor);

            var createUserJson = JsonConvert.SerializeObject(createUserRequest,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

            var restRequest = new RestRequest(Endpoint.Users.ToString(), Method.POST);
            restRequest.AddHeader(ContentType.ContentTypeName, ContentType.Json.ToString());
            restRequest.AddJsonBody(createUserJson);
            return restRequest;
        }

        private static CreateUserRequest CreateUserRequestFromRowObject(CreateUserRowObject createUserRowObject,
            IFleetPayActor actor)
        {
            var createUser = new CreateUserRequest
            {
                Name = actor.GeTestContextDriver.GetTestContext.ReplaceDynamicContentInString(createUserRowObject.Name),
                Job = actor.GeTestContextDriver.GetTestContext.ReplaceDynamicContentInString(createUserRowObject.Job)
            };

            return createUser;
        }
    }
}