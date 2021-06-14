using System;
using Boa.Constrictor.Screenplay;

namespace FleetPay.Acceptance.Test.API.Specs.Interactions
{
    public class ExecuteSampleTask2 : ITask
    {
        private ExecuteSampleTask2()
        {
        }

        public void PerformAs(IActor actor)
        {
            Console.WriteLine("Executing Sample Task 2");
        }


        public static ITask With()
        {
            return new ExecuteSampleTask2();
        }
    }
}