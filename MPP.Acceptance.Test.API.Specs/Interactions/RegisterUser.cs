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
    public class RegisterUser : IQuestion<IRestResponse>
    {
        private readonly RegisterUserRowObject _registerUserRowObject;

        private RegisterUser(RegisterUserRowObject registerUserRowObject)
        {
            _registerUserRowObject = registerUserRowObject;
        }

        public IRestResponse RequestAs(IActor actor)
        {
            var response = actor.Calls(Rest.Request(RegisterUserRestRequest()));

            ((MPPActor)actor).LogLastRequestAndResponse();

            return response;
        }

        public static RegisterUser With(RegisterUserRowObject registerUserRowObject)
        {
            return new(registerUserRowObject);
        }

        private RestRequest RegisterUserRestRequest()
        {
            var registerUserRequest = RegisterUserRequestFromRowObject(_registerUserRowObject);

            var registerUserJson = JsonConvert.SerializeObject(registerUserRequest,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            var restRequest = new RestRequest(Endpoint.Register.ToString(), Method.POST);
            restRequest.AddHeader(ContentType.ContentTypeName, ContentType.Json.ToString());
            restRequest.AddJsonBody(registerUserJson);
            return restRequest;
        }

        private static RegisterUserRequest RegisterUserRequestFromRowObject(RegisterUserRowObject registerUserRowObject)
        {
            var registerUser = new RegisterUserRequest
            {
                Email = registerUserRowObject.Email,
                Password = registerUserRowObject.Password
            };

            return registerUser;
        }


    }
}
