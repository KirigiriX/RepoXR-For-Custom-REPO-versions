using Photon.Pun;
using UnityEngine;

namespace RepoXR.Networking.Frames;

[Frame(FrameHelper.FrameRig)]
public class Rig : IFrame
{
    public Vector3 LeftPosition;
    public Vector3 RightPosition;
    
    public Quaternion LeftRotation;
    public Quaternion RightRotation;
    
    public void Serialize(PhotonStream stream)
    {
        stream.SendNext(LeftPosition);
        stream.SendNext(LeftRotation);
        stream.SendNext(RightPosition);
        stream.SendNext(RightRotation);
    }

    public void Deserialize(PhotonStream stream)
    {
        LeftPosition = (Vector3)stream.ReceiveNext();
        LeftRotation = (Quaternion)stream.ReceiveNext();
        RightPosition = (Vector3)stream.ReceiveNext();
        RightRotation = (Quaternion)stream.ReceiveNext();
    }
}