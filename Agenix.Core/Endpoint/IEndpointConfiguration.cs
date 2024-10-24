namespace Agenix.Core.Endpoint
{
    public interface IEndpointConfiguration
    {
        /// <summary>
        /// Gets/ Sets the timeout either for sending or receiving messages.
        /// </summary>
        long Timeout { get; set; }
    }
}