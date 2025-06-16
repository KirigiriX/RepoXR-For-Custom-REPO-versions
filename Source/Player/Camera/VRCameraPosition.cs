using HarmonyLib;
using RepoXR.Patches;
using UnityEngine;

namespace RepoXR.Player.Camera;

public class VRCameraPosition : MonoBehaviour
{
    public static VRCameraPosition instance;

    public CameraPosition original;
    public Vector3 additionalOffset;

    private Vector3 currentPosition;

    private void Awake()
    {
        instance = this;
        original = GetComponent<CameraPosition>();
    }

    private void Update()
    {
        var smoothing = original.positionSmooth;
        if (original.tumbleSetTimer > 0f)
        {
            smoothing *= 0.5f;
            original.tumbleSetTimer -= Time.deltaTime;
        }
        
        var targetPosition = original.playerTransform.localPosition + original.playerOffset;
        
        if (SemiFunc.MenuLevel() && CameraNoPlayerTarget.instance)
            targetPosition = CameraNoPlayerTarget.instance.transform.position;

        currentPosition = Vector3.Slerp(currentPosition, targetPosition, smoothing * Time.deltaTime);

        transform.localPosition = currentPosition + additionalOffset;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.identity, smoothing * Time.deltaTime);
        
        if (SemiFunc.MenuLevel())
            transform.localPosition = targetPosition;
    }
}

[RepoXRPatch]
internal static class CameraPositionPatches
{
    /// <summary>
    /// Attach a <see cref="VRCameraPosition"/> to any <see cref="CameraPosition"/> game object
    /// </summary>
    [HarmonyPatch(typeof(CameraPosition), nameof(CameraPosition.Awake))]
    [HarmonyPostfix]
    private static void OnCreateCameraPosition(CameraPosition __instance)
    {
        __instance.gameObject.AddComponent<VRCameraPosition>();
    }

    /// <summary>
    /// Disable the base functionality as we'll reimplement it in <see cref="VRCameraPosition"/>
    /// </summary>
    [HarmonyPatch(typeof(CameraPosition), nameof(CameraPosition.Update))]
    [HarmonyPrefix]
    private static bool DisableCameraPosition()
    {
        return false;
    }
}