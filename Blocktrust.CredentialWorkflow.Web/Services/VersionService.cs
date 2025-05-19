using System.Reflection;

namespace Blocktrust.CredentialWorkflow.Web.Services;

public class VersionService
{
    private readonly string _version;

    public VersionService()
    {
        // Get the assembly where this service is defined (the Web project)
        var assembly = Assembly.GetExecutingAssembly();
        
        // The version from .csproj is embedded as AssemblyInformationalVersionAttribute
        // This attribute reflects the <Version> element from the .csproj file
        var infoVersionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        
        if (infoVersionAttribute != null && !string.IsNullOrEmpty(infoVersionAttribute.InformationalVersion))
        {
            // Get just the version number in the format "1.0.1"
            // Information version might contain additional info like git commit hashes
            var versionString = infoVersionAttribute.InformationalVersion;
            
            // Parse the version to ensure we only get the numeric part
            if (Version.TryParse(versionString.Split('+', '-')[0], out var parsedVersion))
            {
                // Format to just Major.Minor.Build (e.g., "1.0.1")
                _version = $"{parsedVersion.Major}.{parsedVersion.Minor}.{parsedVersion.Build}";
            }
            else
            {
                _version = versionString.Split('+', '-')[0]; // Get the first part before any '+' or '-'
            }
        }
        else
        {
            // Fallback to the assembly version if the informational version isn't available
            var assemblyVersion = assembly.GetName().Version;
            if (assemblyVersion != null)
            {
                // Format to just Major.Minor.Build (e.g., "1.0.1")
                _version = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";
            }
            else
            {
                _version = "1.0.0"; // Default fallback
            }
        }
    }

    public string GetVersion()
    {
        return _version;
    }
}