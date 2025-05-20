using System.Reflection;
using log4net;

namespace Agenix.Api;

/// <summary>
///     Provides version management for the Agenix application.
/// </summary>
public static class AgenixVersion
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(AgenixVersion));

    static AgenixVersion()
    {
        try
        {
            // Using reflection to get version information
            var assembly = Assembly.GetExecutingAssembly();

            // Retrieve the AssemblyVersion
            var versionAttribute = assembly.GetName().Version;
            Version = versionAttribute?.ToString() ?? "Version not found";

            // Optionally, retrieve the AssemblyInformationalVersion if you wish
            var infoVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            var infoVersion = infoVersionAttribute?.InformationalVersion ?? "Info version not set";

            if (string.IsNullOrWhiteSpace(infoVersion) || infoVersion == Version)
                Log.Warn("Agenix version has not been updated from the default yet");
        }
        catch (Exception e)
        {
            Log.Warn("Unable to read Agenix version information", e);
            Version = "";
        }
    }

    public static string Version { get; }
}