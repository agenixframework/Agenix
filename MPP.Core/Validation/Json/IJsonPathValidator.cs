using System.Collections.Generic;
using FleetPay.Core.Message;

namespace FleetPay.Core.Validation.Json
{
    internal interface IJsonPathValidator : IMessageProcessor
    {
        new void Process(string payload, TestContext context)
        {
            Validate(payload, context);
        }

        void Validate(string payload, TestContext context);

        interface IBuilder<out T, out TB> where T : IJsonPathValidator where TB : IMessageProcessor.IBuilder<T, TB>
        {
            TB Expressions(Dictionary<string, string> expressions);

            TB Expression(string expression, string value);

            T Build();
        }
    }
}