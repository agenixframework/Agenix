using Boa.Constrictor.RestSharp;
using Boa.Constrictor.Screenplay;
using RestSharp;

namespace MPP.Acceptance.Test.API.Specs.Drivers
{
    public interface IMPPActor
    {
        public MPPActor WhoCanCallRegistrationAPI();

        public MPPActor IsAttemptingTo(ITask task);
    }
}
