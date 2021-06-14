using System;
using Boa.Constrictor.Screenplay;
using MPP.Acceptance.Test.API.Specs.Drivers;

namespace MPP.Acceptance.Test.API.Specs.Interactions
{
    public class ExecuteSampleTask : ITask
    {
        private ExecuteSampleTask()
        {
        }

        public void PerformAs(IActor actor)
        {
            Console.WriteLine("Executing Sample Task");

            ((IMPPActor) actor).IsAttemptingTo(ExecuteSampleTask2.With());
        }


        public static ITask With()
        {
            return new ExecuteSampleTask();
        }
    }
}