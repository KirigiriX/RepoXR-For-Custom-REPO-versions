using HarmonyLib;
using RepoXR.Assets;
using RepoXR.Input;
using RepoXR.Managers;
using RepoXR.Networking;
using RepoXR.Patches;
using RepoXR.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace RepoXR;

[RepoXRPatch]
internal static class Entrypoint
{
    public static void OnSceneLoad(string _)
    {
        if (Plugin.Flags.HasFlag(Flags.VR))
            SetupDefaultSceneVR();

        SetupDefaultSceneUniversal();
    }

    /// <summary>
    /// The default setup for VR for every scene
    /// </summary>
    private static void SetupDefaultSceneVR()
    {
        // We grab all these references manually as most of the instances aren't set yet
        // Since most of them run in the "Start" lifetime function

        // Disable blocking UI
        GameObject.Find("UI/UI/Canvas").GetComponent<Canvas>().enabled = false;

        var canvas = GameObject.Find("UI/HUD/HUD Canvas").transform;
        var fade = canvas.Find("Fade");
        var video = canvas.Find("Render Texture Video");
        var loading = canvas.Find("Loading");

        // The overlay camera is always in the same position in the hierarchy, in every scene
        var overlayCamera = canvas.parent.Find("Camera Overlay").GetComponent<Camera>();
        var mainCamera = Camera.main!;

        // Add tracking to camera
        var poseDriver = mainCamera.gameObject.AddComponent<TrackedPoseDriver>();
        poseDriver.positionAction = Actions.Instance.HeadPosition;
        poseDriver.rotationAction = Actions.Instance.HeadRotation;
        poseDriver.trackingStateInput = new InputActionProperty(Actions.Instance.HeadTrackingState);

        // Parent overlay to main camera
        overlayCamera.transform.SetParent(mainCamera.transform, false);
        overlayCamera.transform.localPosition = Vector3.zero;

        overlayCamera.depth = 2;
        overlayCamera.farClipPlane = 1000;
        overlayCamera.orthographic = false;
        overlayCamera.clearFlags = CameraClearFlags.Depth;
        overlayCamera.targetTexture = null;
        overlayCamera.nearClipPlane = 0.01f;

        // Disable post-processing layer on UI camera (it's sort of broken)
        Object.Destroy(overlayCamera.GetComponent<PostProcessLayer>());

        // Make sure main camera renders to VR
        mainCamera.targetTexture = null;

        // Create blocking overlay (fade + static video)
        var overlayCanvas = new GameObject("VR Overlay Canvas") { layer = 5 }.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        overlayCanvas.worldCamera = overlayCamera;
        overlayCanvas.sortingOrder = 5; // Put a little higher up the order so it renders on top

        fade.SetParent(overlayCanvas.transform, false);
        video.SetParent(overlayCanvas.transform, false);

        // Replace original material since that one has some transparency issues
        video.GetComponent<RawImage>().material = AssetCollection.VideoOverlay;

        // Make sure the components on the overlay fill the entire screen
        overlayCanvas.transform.GetComponentsInChildren<RectTransform>(true).Do(rect =>
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
        });

        // Create loading canvas
        var loadingCanvas = new GameObject("VR Loading Canvas") { layer = 5 }.AddComponent<Canvas>();
        loadingCanvas.renderMode = RenderMode.WorldSpace;
        loadingCanvas.sortingOrder = 6; // Loading canvas needs to render on top of literally everything
        loadingCanvas.transform.localPosition = Vector3.zero;
        loadingCanvas.transform.localEulerAngles = Vector3.zero;
        loadingCanvas.transform.localScale = Vector3.one * 0.01f;
        loadingCanvas.transform.SetParent(Camera.main!.transform.parent, false);
        loadingCanvas.gameObject.AddComponent<UI.LoadingUI>();
        loadingCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(720, 400); // For masking

        loading.SetParent(loadingCanvas.transform, false);

        // Create custom camera (if enabled)
        if (Plugin.Config.CustomCamera.Value)
            Object.Instantiate(AssetCollection.CustomCamera, Camera.main.transform.parent);

        // Create haptic feedback manager
        new GameObject("Haptic Manager").AddComponent<HapticManager>();

        // Create persistent data manager
        new GameObject("Data Manager").AddComponent<DataManager>();
    }

    private static bool hasShownErrorMessage;

    /// <summary>
    /// The default setup for every scene (including for non-vr players)
    /// </summary>
    private static void SetupDefaultSceneUniversal()
    {
        new GameObject("RepoXR Network System").AddComponent<NetworkSystem>();

        ShowVRFailedWarning();

#if DEBUG
        ShowEarlyAccessWarning();
#endif
    }

    /// <summary>
    /// <see cref="GameDirector"/> is always present in the `Main` scene, so we use it as entrypoint
    /// </summary>
    [HarmonyPatch(typeof(GameDirector), nameof(GameDirector.Start))]
    [HarmonyPostfix]
    private static void OnStartup(GameDirector __instance)
    {
        VRInputSystem.instance.ActivateInput();

        if (RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu ||
            RunManager.instance.levelCurrent == RunManager.instance.levelLobbyMenu)
            OnStartupMainMenu();
        else
            OnStartupInGame();
    }

    private static void OnStartupMainMenu()
    {
        HUDCanvas.instance.gameObject.AddComponent<MainMenu>();
        DataManager.instance.ResetData();
    }

    private static void OnStartupInGame()
    {
        GameDirector.instance.gameObject.AddComponent<VRSession>();
    }

    private static void ShowVRFailedWarning()
    {
        if (!Plugin.Flags.HasFlag(Flags.StartupFailed) ||
            hasShownErrorMessage || RunManager.instance.levelCurrent != RunManager.instance.levelMainMenu)
            return;

        hasShownErrorMessage = true;
        MenuManager.instance.PagePopUpScheduled("VR Startup Failed", Color.red,
            "RepoXR tried to launch the game in VR, however an error occured during initialization.\n\nYou can disable VR in the settings if you are not planning to play in VR.",
            "Alright fam",
            true);
    }

#if DEBUG
    private static bool earlyAccessWarningShown;

    private static void ShowEarlyAccessWarning()
    {
        if (earlyAccessWarningShown || !VRSession.InVR)
            return;

        earlyAccessWarningShown = true;
        MenuManager.instance.PagePopUpScheduled("VR Development Build", Color.red,
            "You are using a development build of RepoXR. This build is not finished and might contain bugs or other unforeseen \"features\".\n\nEnter with caution.",
            "Let me play!",
            true);
    }
#endif
}