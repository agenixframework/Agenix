using System;
using System.Collections.Generic;
using Boa.Constrictor.Screenplay;
using MPP.Acceptance.Test.API.Specs.Drivers;

namespace MPP.Acceptance.Test.API.Specs.Interactions
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
