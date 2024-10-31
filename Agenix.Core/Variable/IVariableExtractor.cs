using System.Collections.Generic;
using Agenix.Core.Message;

namespace Agenix.Core.Variable;

internal interface IVariableExtractor : IMessageProcessor
{
    new void Process(string payload, TestContext context)
    {
        ExtractVariables(payload, context);
    }

    void ExtractVariables(object payload, TestContext context);

    interface IBuilder<out T, out TB> where T : IVariableExtractor where TB : IMessageProcessor.IBuilder<T, TB>
    {
        TB Expressions(Dictionary<string, string> expressions);

        TB Expression(string expression, string value);

        T Build();
    }
}