#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using Agenix.ReqnrollPlugin;
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
    ///     Configures the global dependencies within the plugin for runtime execution.
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
