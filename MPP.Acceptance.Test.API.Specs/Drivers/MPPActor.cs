using Boa.Constrictor.Logging;
using Boa.Constrictor.RestSharp;
using Boa.Constrictor.Screenplay;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using RestSharp;

namespace MPP.Acceptance.Test.API.Specs.Drivers
{
    public class MPPActor : Actor, IMPPActor
    {
        private readonly IEnvironmentConfigurationDriver _environmentConfigurationDriver;

        private CallRestApi _callRestApi;

        public MPPActor(string name, ILogger logger, IEnvironmentConfigurationDriver environmentConfigurationDriver) : base(name, logger)
        {
            _environmentConfigurationDriver = environmentConfigurationDriver;
        }

        public MPPActor(IEnvironmentConfigurationDriver environmentConfigurationDriver): base("MPPActor", new ConsoleLogger())
        {
            _environmentConfigurationDriver = environmentConfigurationDriver;
        }

        public MPPActor IsAttemptingTo(ITask task)
        {
            this.AttemptsTo(task);
            return this;
        }

        public IRestResponse SeeLastReceivedResponse()
        {
            return _callRestApi.LastResponse;
        }

        public IRestRequest SeeLastSentRequest()
        {
            return _callRestApi.LastRequest;
        }

        public void LogRequest(IRestRequest request, IRestResponse response)
        {
            var requestToLog = new
            {
                resource = request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = request.Method.ToString(),
            };

            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage,
            };


            this.Logger.Info(string.Format("Request: {0}, Response: {1}", JsonConvert.SerializeObject(requestToLog),
                JsonConvert.SerializeObject(responseToLog)));
        }

        public void LogLastRequestAndResponse()
        {
            LogRequest(this.SeeLastSentRequest(), this.SeeLastReceivedResponse());
        }

        public MPPActor WhoCanCallRegistrationApi()
        {
            return WhoCanCallApi(_environmentConfigurationDriver.RegistrationAPIUrl);
        }

        private MPPActor WhoCanCallApi(string baseUrl)
        {
            var restSharpAbility = CallRestApi.At(baseUrl);
            restSharpAbility.DumpingRequestsTo(Directory.GetCurrentDirectory());

            this.Can(restSharpAbility);

            _callRestApi = restSharpAbility;

            return this;
        }
    }
}
