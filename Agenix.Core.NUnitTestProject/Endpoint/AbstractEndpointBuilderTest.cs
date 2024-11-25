using System;
using Agenix.Core.Annotations;
using Agenix.Core.Endpoint;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Endpoint;

public class AbstractEndpointBuilderTest : AbstractNUnitSetUp
{
    private readonly TestEndpointBuilder _endpointBuilder = new();
    private readonly TestEndpointBuilder.PersonClass _person = new("Peter", 29);
    [AgenixEndpoint("fooEndpoint",
        "message:Hello from Agenix!",
        "number:1:System.Int32",
        "person:testPerson:Agenix.Core.NUnitTestProject.Endpoint.AbstractEndpointBuilderTest+TestEndpointBuilder+PersonClass"
    )]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private IEndpoint _injected;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    protected override TestContextFactory CreateTestContextFactory()
    {
        var contextFactory = base.CreateTestContextFactory();
        contextFactory.GetReferenceResolver().Bind("testBuilder", _endpointBuilder);
        contextFactory.GetReferenceResolver().Bind("testPerson", _person);
        return contextFactory;
    }

    [Test]
    public void BuildFromEndpointProperties()
    {
        AgenixEndpointAnnotations.InjectEndpoints(this, Context);

        Console.WriteLine(typeof(TestEndpointBuilder.PersonClass).FullName);

        ClassicAssert.AreEqual(_injected, _endpointBuilder.MockEndpoint);
        ClassicAssert.AreEqual(_endpointBuilder.Message, "Hello from Agenix!");
        ClassicAssert.AreEqual(_endpointBuilder.Number, 1);
        //ClassicAssert.AreEqual(_endpointBuilder.Person, _person);
    }

    public class TestEndpointBuilder : AbstractEndpointBuilder<IEndpoint>
    {
        public string Message { get; private set; }
        public PersonClass Person { get; private set; }
        public int Number { get; private set; }

        public IEndpoint MockEndpoint { get; } = new Mock<IEndpoint>().Object;

        protected override IEndpoint GetEndpoint()
        {
            return MockEndpoint;
        }

        public override bool Supports(Type endpointType)
        {
            return true;
        }

        public TestEndpointBuilder SetMessage(string message)
        {
            Message = message;
            return this;
        }

        public TestEndpointBuilder SetNumber(int number)
        {
            Number = number;
            return this;
        }

        public TestEndpointBuilder SetPerson(PersonClass person)
        {
            Person = person;
            return this;
        }

        public class PersonClass(string name, int age)
        {
            public string Name { get; } = name;
            public int Age { get; } = age;
        }
    }
}