using System;
using System.IO;
using System.Linq;
using Boa.Constrictor.Logging;
using Boa.Constrictor.RestSharp;
using Boa.Constrictor.Screenplay;
using MPP.Acceptance.Test.API.Specs.Support;
using MPP.Core.Session;
using Newtonsoft.Json;
using RestSharp;

namespace MPP.Acceptance.Test.API.Specs.Drivers
{
    public class MPPActor : Actor, IMPPActor
    {
        private readonly IEnvironmentConfigurationDriver _environmentConfigurationDriver;

        private readonly ITestContextDriver _testContextDriver;

        private CallRestApi _callRestApi;


        public MPPActor(string name, ILogger logger, IEnvironmentConfigurationDriver environmentConfigurationDriver,
            ITestContextDriver testContextDriver) :
            base(name, logger)
        {
            _environmentConfigurationDriver = environmentConfigurationDriver;
            _testContextDriver = testContextDriver;
        }

        public MPPActor(IEnvironmentConfigurationDriver environmentConfigurationDriver,
            ITestContextDriver testContextDriver) : base("MPPActor",
            new ConsoleLogger())
        {
            _environmentConfigurationDriver = environmentConfigurationDriver;
            _testContextDriver = testContextDriver;
        }

        public MPPActor IsAttemptingTo(ITask task)
        {
            AttemptsTo(task);
            return this;
        }

        public IRestResponse SeeLastReceivedResponse()
        {
            return _callRestApi.LastResponse;
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
                method = request.Method.ToString()
            };

            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage
            };


            Logger.Info(string.Format("Request: {0}, Response: {1}", JsonConvert.SerializeObject(requestToLog),
                JsonConvert.SerializeObject(responseToLog)));
        }

        public void LogLastRequestAndResponse()
        {
            LogRequest(SeeLastSentRequest(), SeeLastReceivedResponse());
        }

        public MPPActor WhoCanCallRegistrationApi()
        {
            return WhoCanCallApi(_environmentConfigurationDriver.RegistrationApiUrl);
        }

        public MPPActor WhoCanCallShellApiGateway()
        {
            return WhoCanCallApi(_environmentConfigurationDriver.ShellApiGatewayUrl);
        }

        public MPPActor WhoCanCallStationLocatorApi()
        {
            return WhoCanCallApi(_environmentConfigurationDriver.StationLocatorApiUrl);
        }


        public RememberVariableSetter Remembers(InMemory key)
        {
            return new(key);
        }

        public RememberVariableSetter Remembers(string key)
        {
            return new(key);
        }

        public void RememberLastReceivedResponse(InMemory key)
        {
            Remembers(key).That(SeeLastReceivedResponse());
        }

        public T Recall<T>(InMemory memory)
        {
            return ObjectBag.SessionVariableCalled<T>(memory);
        }

        public void Echo(string message)
        {
            Console.WriteLine(GeTestContextDriver().GetTestContext.ReplaceDynamicContentInString(message));
        }

        public ITestContextDriver GeTestContextDriver()
        {
            return _testContextDriver;
        }

        public IRestRequest SeeLastSentRequest()
        {
            return _callRestApi.LastRequest;
        }

        private MPPActor WhoCanCallApi(string baseUrl)
        {
            var restSharpAbility = CallRestApi.At(baseUrl);
            restSharpAbility.DumpingRequestsTo(Directory.GetCurrentDirectory());

            Can(restSharpAbility);

            _callRestApi = restSharpAbility;

            return this;
        }

        public class RememberVariableSetter
        {
            private readonly object _key;

            public RememberVariableSetter(object key)
            {
                _key = key;
            }

            public void That<T>(T value)
            {
                ObjectBag.SetSessionVariable(_key).To(value);
            }
        }
    }
}