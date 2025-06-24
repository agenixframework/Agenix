using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Core.Message;
using Agenix.Core.Validation.Json;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Json;

public class JsonMappingValidationProcessorTest : AbstractNUnitSetUp
{
    private readonly JsonSerializer _jsonSerializer = new();
    private readonly IMessage _message = new DefaultMessage("{\"name\": \"John\", \"age\": 23}");

    [Test]
    public void ShouldValidate()
    {
        var processor = JsonMappingValidationProcessor<Person>
            .Validate()
            .JsonSerializer(_jsonSerializer)
            .Validator((person, headers, context) =>
            {
                Assert.That(person.Name, Is.EqualTo("John"));
                Assert.That(person.Age, Is.EqualTo(23));
            })
            .Build();

        processor.Validate(_message, Context);
    }

    [Test]
    public void ShouldResolveMapper()
    {
        var referenceResolver = new SimpleReferenceResolver();
        referenceResolver.Bind("serializer", _jsonSerializer);

        var processor = JsonMappingValidationProcessor<Person>
            .Validate()
            .WithReferenceResolver(referenceResolver)
            .Validator((person, headers, context) =>
            {
                Assert.That(person.Name, Is.EqualTo("John"));
                Assert.That(person.Age, Is.EqualTo(23));
            })
            .Build();

        processor.Validate(_message, Context);
    }

    [Test]
    public void ShouldFailValidation()
    {
        var processor = JsonMappingValidationProcessor<Person>
            .Validate()
            .JsonSerializer(_jsonSerializer)
            .Validator((person, headers, context) => { Assert.That(person.Age, Is.EqualTo(-1)); })
            .Build();

        Assert.Throws<AssertionException>(() => processor.Validate(_message, Context));
    }

    /// <summary>
    ///     Represents a person with a name and an age.
    /// </summary>
    private sealed record Person
    {
        // Auto-implemented properties in C#
        public int Age { get; set; }
        public string Name { get; set; }
    }
}
