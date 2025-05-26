using Agenix.Api.Log;
using Agenix.ReqnrollPlugin;
using Microsoft.Extensions.Logging;
using Reqnroll.Bindings;
using Reqnroll.Plugins;
using Reqnroll.UnitTestProvider;

[assembly: RuntimePlugin(typeof(Plugin))]

namespace Agenix.ReqnrollPlugin;

/// <summary>
///     Registered Reqnroll plugin from configuration file.
/// </summary>
public class Plugin : IRuntimePlugin
{
    public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters,
        UnitTestProviderConfiguration unitTestProviderConfiguration)
    {
        runtimePluginEvents.CustomizeGlobalDependencies += CustomizeGlobalDependencies;
        runtimePluginEvents.CustomizeScenarioDependencies += CustomizeScenarioDependencies;
    }

    /// <summary>
    /// Configures the global dependencies within the plugin for runtime execution.
    /// </summary>
    /// <param name="sender">The sender of the event that triggered the customization.</param>
    /// <param name="e">The event arguments containing details about the dependency customization process.</param>
    private void CustomizeGlobalDependencies(object sender, CustomizeGlobalDependenciesEventArgs e)
    {
        // Add this line to ensure your assembly is included
        e.ReqnrollConfiguration.AdditionalStepAssemblies.Add(typeof(AgenixHooks).Assembly.GetName().Name);

        e.ObjectContainer.RegisterTypeAs<SafeBindingInvoker, IAsyncBindingInvoker>();
    }

    private void CustomizeScenarioDependencies(object sender, CustomizeScenarioDependenciesEventArgs e)
    {
        var bindingProvider = new BindingInstanceProvider();
        
        // Store reference to the container
        bindingProvider.SetObjectContainer(e.ObjectContainer);
        
        e.ObjectContainer.RegisterInstanceAs(bindingProvider);
        
        // Load all binding instances if desired
        bindingProvider.GetAllBindingInstances();
    }
}