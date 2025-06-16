using RepoXR.Managers;

namespace RepoXR.Networking;

public static class NetworkingExtensions
{
    public static bool IsVRPlayer(this PlayerAvatar player)
    {
        return (player.isLocal && VRSession.InVR) || (!player.isLocal && NetworkSystem.instance.IsVRPlayer(player));
    }
}