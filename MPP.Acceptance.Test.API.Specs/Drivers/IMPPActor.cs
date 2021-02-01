using Boa.Constrictor.Screenplay;
using MPP.Acceptance.Test.API.Specs.Support;
using RestSharp;

namespace MPP.Acceptance.Test.API.Specs.Drivers
{
    public interface IMPPActor
    {
        public MPPActor WhoCanCallRegistrationApi();

        public MPPActor IsAttemptingTo(ITask task);

        public IRestResponse SeeLastReceivedResponse();

        public void LogRequest(IRestRequest request, IRestResponse response);

        public void LogLastRequestAndResponse();

        public MPPActor.RememberVariableSetter Remembers(InMemory key);

        public void RememberLastReceivedResponse(InMemory key);

        public T Recall<T>(InMemory memory);
    }
}