using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

namespace RepoXR.UI.Settings;

public class SettingOption : MonoBehaviour
{
    public string settingCategory;
    public string settingName;
    public string? settingDisplayName;
    public TextMeshProUGUI settingText;

    public RectTransform rectTransform;
    
    private void Start()
    {
        settingText.text = settingDisplayName ?? Utils.ToHumanReadable(settingName);
    }

    public void FetchBoolOption()
    {
        var option = GetComponent<MenuTwoOptions>();
        option.startSettingFetch = (bool)Plugin.Config.File[settingCategory, settingName].BoxedValue;
    }

    public void UpdateBool(bool value)
    {
        Plugin.Config.File[settingCategory, settingName].BoxedValue = value;
    }

    public void UpdateInt(int value)
    {
        Plugin.Config.File[settingCategory, settingName].BoxedValue = value;
    }

    public void UpdateFloat(float value)
    {
        Plugin.Config.File[settingCategory, settingName].BoxedValue = value;
    }

    public void UpdateSlider()
    {
        var slider = GetComponent<FloatMenuSlider>();

        if (slider.isInteger)
            UpdateInt(Mathf.RoundToInt(slider.currentValue));
        else
            UpdateFloat(slider.currentValue);
    }
}

public class RuntimeSettingOption : MonoBehaviour
{
    public RectTransform rectTransform;
    public TextMeshProUGUI runtimeText;

    private FloatMenuSlider slider;
    private OpenXR.Runtimes runtimes;

    private void Awake()
    {
        slider = GetComponent<FloatMenuSlider>();
        runtimes = OpenXR.GetRuntimes();

        slider.hasBar = false;
        slider.wrapAround = true;
        slider.customOptions =
        [
            new FloatMenuSlider.CustomOption
            {
                customOptionText = "System Default"
            },
            ..runtimes.Select(runtime => new FloatMenuSlider.CustomOption
            {
                customOptionText = runtime.Name
            })
        ];

        slider.currentValue =
            runtimes.TryGetRuntimeByPath(Plugin.Config.OpenXRRuntimeFile.Value, out var currentRuntime)
                ? runtimes.IndexOf(currentRuntime) + 1
                : 0;

        UpdateSlider();
    }

    public void UpdateSlider()
    {
        if (slider.currentValue == 0)
        {
            Plugin.Config.OpenXRRuntimeFile.Value = "";
            runtimeText.text = "System Default";
            return;
        }

        if (!runtimes.TryGetRuntime(slider.customOptions[(int)slider.currentValue].customOptionText, out var runtime))
            return;

        Plugin.Config.OpenXRRuntimeFile.Value = runtime.Path;
        runtimeText.text = runtime.Name;
    }
}