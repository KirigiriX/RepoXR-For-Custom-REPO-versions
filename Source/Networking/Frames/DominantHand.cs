using Photon.Pun;

namespace RepoXR.Networking.Frames;

[Frame(FrameHelper.FrameDominantHand)]
public class DominantHand : IFrame
{
    public bool LeftHanded;

    public void Serialize(PhotonStream stream)
    {
        stream.SendNext(LeftHanded);
    }

    public void Deserialize(PhotonStream stream)
    {
        LeftHanded = (bool)stream.ReceiveNext();
    }
}