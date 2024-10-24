using Boa.Constrictor.Screenplay;
using FleetPay.Acceptance.Test.API.Specs.Support;
using RestSharp;

namespace FleetPay.Acceptance.Test.API.Specs.Drivers
{
    public interface IFleetPayActor
    {
        public FleetPayActor WhoCanCallRegistrationApi { get; }

        public FleetPayActor WhoCanCallShellApiGateway { get; }

        public FleetPayActor WhoCanCallStationLocatorApi { get; }

        public ITestContextDriver GeTestContextDriver { get; }

        public IEnvironmentConfigurationDriver GetEnvironmentConfigurationDriver { get; }

        public FleetPayActor IsAttemptingTo(ITask task);

        public IRestResponse SeeLastReceivedResponse();

        public void LogRequest(IRestRequest request, IRestResponse response);

        public void LogLastRequestAndResponse();

        public FleetPayActor.RememberVariableSetter Remembers(InMemory key);

        public FleetPayActor.RememberVariableSetter Remembers(string key);

        public void RememberLastReceivedResponse(InMemory key);

        public T Recall<T>(InMemory memory);

        public void Echo(string message);
    }
}