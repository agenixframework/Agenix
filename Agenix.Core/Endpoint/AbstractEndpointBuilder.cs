namespace Agenix.Core.Endpoint
{
    public abstract class AbstractEndpointBuilder<T> where T : IEndpoint, IEndpointBuilder<T>
    {
        /// <summary>
        ///     Sets the endpoint name.
        /// </summary>
        /// <param name="endpointName">the endpoint name</param>
        /// <returns></returns>
        public AbstractEndpointBuilder<T> Name(string endpointName)
        {
            GetEndpoint().Name = endpointName;
            return this;
        }

        public T Build()
        {
            return GetEndpoint();
        }

        /// <summary>
        ///     Gets the target endpoint instance.
        /// </summary>
        /// <returns></returns>
        protected abstract T GetEndpoint();
    }
}