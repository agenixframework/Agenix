namespace FleetPay.Core.Message
{
    interface IMessageTransformer
    {

        interface IBuilder<out T, TB> where T: IMessageTransformer where TB : IBuilder<T, TB> {

            /// <summary>
            /// Builds new message processor instance.
            /// </summary>
            /// <returns></returns>
            T Build();
        }
    }
}
