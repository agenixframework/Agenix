using Boa.Constrictor.Logging;
using Boa.Constrictor.RestSharp;
using Boa.Constrictor.Screenplay;
using System.Net;

namespace MPP.Acceptance.Test.API.Specs.Drivers
{
    public class MPPActor : Actor, IMPPActor
    {
        readonly IEnvironmentConfigurationDriver _environmentConfigurationDriver;

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

     
        public MPPActor WhoCanCallRegistrationAPI()
        {
            return WhoCanCallAPI(_environmentConfigurationDriver.RegistrationAPIUrl);
        }

        private MPPActor WhoCanCallAPI(string baseUrl)
        {
            var restSharpAbility = CallRestApi.At(baseUrl);

            restSharpAbility.Client.Proxy = new WebProxy("18.132.87.24", 8888);

            this.Can(restSharpAbility);

            return this;
        }
    }
}
