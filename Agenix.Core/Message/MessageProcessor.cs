namespace Agenix.Core.Message;

internal interface IMessageProcessor : IMessageTransformer
{
    void Process(string payload, TestContext context);

    interface IBuilder<out T, TB> where T : IMessageProcessor where TB : IBuilder<T, TB>
    {
        /// <summary>
        ///     Builds new message processor instance.
        /// </summary>
        /// <returns></returns>
        T Build();
    }
}