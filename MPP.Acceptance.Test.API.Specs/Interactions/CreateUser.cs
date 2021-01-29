using Boa.Constrictor.RestSharp;
using Boa.Constrictor.Screenplay;
using MPP.Acceptance.Test.API.Specs.Drivers;
using MPP.Acceptance.Test.API.Specs.Model;
using MPP.Acceptance.Test.API.Specs.Steps;
using MPP.Acceptance.Test.API.Specs.Support;
using Newtonsoft.Json;
using RestSharp;

namespace MPP.Acceptance.Test.API.Specs.Interactions
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
            var response = actor.Calls(Rest.Request(CreateUserRestRequest()));

            ((MPPActor) actor).LogLastRequestAndResponse();

            return response;
        }

        public static CreateUser With(CreateUserRowObject createUserRowObject)
        {
            return new(createUserRowObject);
        }

        private RestRequest CreateUserRestRequest()
        {
            var createUserRequest = CreateUserRequestFromRowObject(_createUserRowObject);

            var createUserJson = JsonConvert.SerializeObject(createUserRequest,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

            var restRequest = new RestRequest(Endpoint.Users.ToString(), Method.POST);
            restRequest.AddHeader(ContentType.ContentTypeName, ContentType.Json.ToString());
            restRequest.AddJsonBody(createUserJson);
            return restRequest;
        }

        private static CreateUserRequest CreateUserRequestFromRowObject(CreateUserRowObject createUserRowObject)
        {
            var createUser = new CreateUserRequest
            {
                Name = createUserRowObject.Name,
                Job = createUserRowObject.Job
            };

            return createUser;
        }
    }
}