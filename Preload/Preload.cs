using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using Mono.Cecil;

namespace RepoXR.Preload;

public static class Preload
{
    public static IEnumerable<string> TargetDLLs { get; } = [];

    private const string VR_MANIFEST = """
                                       {
                                         "name": "OpenXR XR Plugin",
                                         "version": "1.10.0",
                                         "libraryName": "UnityOpenXR",
                                         "displays": [
                                           {
                                             "id": "OpenXR Display"
                                           }
                                         ],
                                         "inputs": [
                                           {
                                             "id": "OpenXR Input"
                                           }
                                         ]
                                       }
                                       """;

    private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("RepoXR.Preload");

    public static void Initialize()
    {
        Logger.LogInfo("Setting up VR runtime assets");

        SetupRuntimeAssets();
        
        Logger.LogInfo("We're done here. Goodbye!");
    }

    /// <summary>
    /// Place required runtime libraries and configuration in the game files to allow VR to be started
    /// </summary>
    private static void SetupRuntimeAssets()
    {
        var root = Path.Combine(Paths.GameRootPath, "REPO_Data");
        var subsystems = Path.Combine(root, "UnitySubsystems");
        if (!Directory.Exists(subsystems))
            Directory.CreateDirectory(subsystems);

        var openXr = Path.Combine(subsystems, "UnityOpenXR");
        if (!Directory.Exists(openXr))
            Directory.CreateDirectory(openXr);

        var manifest = Path.Combine(openXr, "UnitySubsystemsManifest.json");
        if (!File.Exists(manifest))
            File.WriteAllText(manifest, VR_MANIFEST);

        var plugins = Path.Combine(root, "Plugins");
        var oxrPluginTarget = Path.Combine(plugins, "UnityOpenXR.dll");
        var oxrLoaderTarget = Path.Combine(plugins, "openxr_loader.dll");

        var current = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var oxrPlugin = Path.Combine(current, "RuntimeDeps/UnityOpenXR.dll");
        var oxrLoader = Path.Combine(current, "RuntimeDeps/openxr_loader.dll");
        
        File.Copy(oxrPlugin, oxrPluginTarget, true);
        File.Copy(oxrLoader, oxrLoaderTarget, true);
    }
    
    public static void Patch(AssemblyDefinition assembly)
    {
        // No-op
    }
}
