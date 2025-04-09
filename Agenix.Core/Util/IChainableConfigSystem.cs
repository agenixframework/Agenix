using System.Configuration.Internal;

namespace Agenix.Core.Util
{
    /// <summary>
    /// Implement this interface to create your own, delegating <see cref="IInternalConfigSystem"/>
    /// and set them using <see cref="ConfigurationUtils.SetConfigurationSystem"/>
    /// </summary>
    public interface IChainableConfigSystem : IInternalConfigSystem
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="innerConfigSystem"></param>
        void SetInnerConfigurationSystem(IInternalConfigSystem innerConfigSystem);
    }
}
