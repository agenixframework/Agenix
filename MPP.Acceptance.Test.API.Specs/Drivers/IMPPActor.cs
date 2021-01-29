using Boa.Constrictor.RestSharp;
using Boa.Constrictor.Screenplay;
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
    }
}
