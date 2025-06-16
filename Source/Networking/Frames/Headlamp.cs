using Photon.Pun;

namespace RepoXR.Networking.Frames;

[Frame(FrameHelper.FrameHeadlamp)]
public class Headlamp : IFrame
{
    public bool HeadlampEnabled;

    public void Serialize(PhotonStream stream)
    {
        stream.SendNext(HeadlampEnabled);
    }

    public void Deserialize(PhotonStream stream)
    {
        HeadlampEnabled = (bool)stream.ReceiveNext();
    }
}