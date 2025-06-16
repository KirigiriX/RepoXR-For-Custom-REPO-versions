using HarmonyLib;
using RepoXR.Networking;

namespace RepoXR.Patches.Player;

[RepoXRPatch]
internal static class PlayerNetworkPatches
{
    /// <summary>
    /// Announce that we are a VR player again since a new player joined the lobby
    /// </summary>
    [HarmonyPatch(typeof(NetworkManager), nameof(NetworkManager.PlayerSpawnedRPC))]
    [HarmonyPostfix]
    private static void OnPlayerJoined()
    {
        NetworkSystem.instance.AnnounceVRPlayer();
    }
}