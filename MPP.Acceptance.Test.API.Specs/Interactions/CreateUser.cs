using System;
using Boa.Constrictor.RestSharp;
using Boa.Constrictor.Screenplay;
using MPP.Acceptance.Test.API.Specs.Model;
using MPP.Acceptance.Test.API.Specs.Steps;
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
            Console.WriteLine("CreateUser task Request...");

            var createUserRequest = CreateUserRequestFromRowObject(_createUserRowObject);
            var isoJson = JsonConvert.SerializeObject(createUserRequest,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});

            Console.WriteLine("CreateUser request serealiazed in String Json..." + isoJson);


            var restRequest = new RestRequest("/mpp-user/users", Method.POST);
            restRequest.AddHeader("Participant-Code", createUserRequest.ParticipantCode);
            restRequest.AddHeader("Content-Language", "application/json");
            restRequest.AddJsonBody(isoJson);

            var restResponse = actor.Calls(Rest.Request(restRequest));
            actor.Logger.Info("Current response" + restResponse.Content);

            return restResponse;
        }

        public static CreateUser With(CreateUserRowObject createUserRowObject)
        {
            return new(createUserRowObject);
        }

        private CreateUserRequest CreateUserRequestFromRowObject(CreateUserRowObject createUserRowObject)
        {
            var createUser = new CreateUserRequest();
            createUser.ParticipantCode = createUserRowObject.ParticipantCode;
            createUser.ParticipantReferenceId = createUserRowObject.ParticipantReferenceId;
            createUser.FullName = createUserRowObject.FullName;
            createUser.UserName = createUserRowObject.UserName;
            createUser.Email = createUserRowObject.PhoneNumber;
            createUser.Password = createUserRowObject.Password;

            return createUser;
        }
    }
}