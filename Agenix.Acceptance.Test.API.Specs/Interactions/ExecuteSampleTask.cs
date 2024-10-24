using System;
using Boa.Constrictor.Screenplay;
using FleetPay.Acceptance.Test.API.Specs.Drivers;

namespace FleetPay.Acceptance.Test.API.Specs.Interactions
{
    public class ExecuteSampleTask : ITask
    {
        private ExecuteSampleTask()
        {
        }

        public void PerformAs(IActor actor)
        {
            Console.WriteLine("Executing Sample Task");

            ((IFleetPayActor) actor).IsAttemptingTo(ExecuteSampleTask2.With());
        }


        public static ITask With()
        {
            return new ExecuteSampleTask();
        }
    }
}