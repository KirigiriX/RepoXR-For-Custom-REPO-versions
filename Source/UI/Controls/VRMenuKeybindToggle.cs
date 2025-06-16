using RepoXR.Input;
using UnityEngine;

namespace RepoXR.UI.Controls;

public class VRMenuKeybindToggle: MonoBehaviour
{
    public string inputAction;

    public void EnableToggle()
    {
        VRInputSystem.instance.InputToggleRebind(inputAction, true);
    }

    public void DisableToggle()
    {
        VRInputSystem.instance.InputToggleRebind(inputAction, false);
    }
    
    public void FetchSetting()
    {
        GetComponent<MenuTwoOptions>().startSettingFetch = VRInputSystem.instance.InputToggleGet(inputAction);
    }
}