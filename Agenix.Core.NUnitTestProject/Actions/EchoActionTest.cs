using NUnit.Framework.Legacy;
using NUnit.Framework;
using System.Collections.Generic;
using Agenix.Core.Actions;
using Agenix.Core.Exceptions;

namespace Agenix.Core.NUnitTestProject.Actions
{
    public class EchoActionTest : AbstractNUnitSetUp
    {

        [Test]
        public void TestEchoMessage()
        {
          EchoAction echo = new EchoAction.Builder().Message("Hello Agenix!").Build();

          echo.Execute(Context);
        }

        [Test]
        public void TestEchoMessageWithVariables()
        {
            EchoAction echo = new EchoAction.Builder().Message("${greeting} Agenix!").Build();
            Context.SetVariable("greeting", "Hello");

            echo.Execute(Context);
        }

        [Test]
        public void TestEchoMessageWithFunctions()
        {
            EchoAction echo = new EchoAction.Builder().Message("Today is core:CurrentDate()").Build();
      
            echo.Execute(Context);
        }

        [Test]
        public void TestEchoMessageWithUnkonwnVariables()
        {
            EchoAction echo = new EchoAction.Builder().Message("${greeting} Agenix").Build();

            Assert.Throws<CoreSystemException>(() => echo.Execute(Context)); 
        }
    }
}
