using System;
using System.IO;
using System.Linq;
using Boa.Constrictor.Logging;
using Boa.Constrictor.RestSharp;
using Boa.Constrictor.Screenplay;
using FleetPay.Acceptance.Test.API.Specs.Support;
using FleetPay.Core.Session;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators;

namespace FleetPay.Acceptance.Test.API.Specs.Drivers
{
    public class FleetPayActor : Actor, IFleetPayActor
    {
        private CallRestApi _callRestApi;

        public FleetPayActor(string name, ILogger logger,
            IEnvironmentConfigurationDriver environmentConfigurationDriver,
            ITestContextDriver testContextDriver) :
            base(name, logger)
        {
            GetEnvironmentConfigurationDriver = environmentConfigurationDriver;
            GeTestContextDriver = testContextDriver;
        }

        public FleetPayActor(IEnvironmentConfigurationDriver environmentConfigurationDriver,
            ITestContextDriver testContextDriver) : base("FleetPayActor",
            new ConsoleLogger())
        {
            GetEnvironmentConfigurationDriver = environmentConfigurationDriver;
            GeTestContextDriver = testContextDriver;
        }

        public FleetPayActor IsAttemptingTo(ITask task)
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


            Logger.Info(
                $"Request: {JsonConvert.SerializeObject(requestToLog)}, Response: {JsonConvert.SerializeObject(responseToLog)}");
        }

        public void LogLastRequestAndResponse()
        {
            LogRequest(SeeLastSentRequest(), SeeLastReceivedResponse());
        }

        public FleetPayActor WhoCanCallRegistrationApi =>
            WhoCanCallApi(GetEnvironmentConfigurationDriver.RegistrationApiUrl);

        public FleetPayActor WhoCanCallShellApiGateway =>
            WhoCanCallApi(GetEnvironmentConfigurationDriver.ShellApiGatewayUrl);

        public FleetPayActor WhoCanCallStationLocatorApi =>
            WhoCanCallApi(GetEnvironmentConfigurationDriver.StationLocatorApiUrl);

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
            Console.WriteLine(GeTestContextDriver.GetTestContext.ReplaceDynamicContentInString(message));
        }

        public ITestContextDriver GeTestContextDriver { get; }

        public IEnvironmentConfigurationDriver GetEnvironmentConfigurationDriver { get; }

        public IRestRequest SeeLastSentRequest()
        {
            return _callRestApi.LastRequest;
        }

        private FleetPayActor WhoCanCallApi(string baseUrl)
        {
            var restSharpAbility = CallRestApi.At(baseUrl);
            restSharpAbility.DumpingRequestsTo(Directory.GetCurrentDirectory());

            if (GetEnvironmentConfigurationDriver.IsStationLocatorBasicAuthEnabled)
                restSharpAbility.Client.Authenticator = new HttpBasicAuthenticator(
                    GetEnvironmentConfigurationDriver.StationLocatorBasicAuthUsername,
                    GetEnvironmentConfigurationDriver.StationLocatorBasicAuthPassword);

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