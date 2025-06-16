using System;
using System.Reflection;
using BepInEx.Configuration;
using HarmonyLib;
using RepoXR.Assets;
using RepoXR.UI.Settings;
using TMPro;
using UnityEngine;

namespace RepoXR.UI.Menu;

public class SettingsCategoryPage : MonoBehaviour
{
    public string categoryName;
    public TextMeshProUGUI categoryText;

    public Transform scrollerTransform;
    
    private void Start()
    {
        categoryText.text = categoryName;

        var positionOffset = -50f;

        if (categoryName == "General")
        {
            var setting = Instantiate(AssetCollection.RuntimeSetting, scrollerTransform).GetComponent<RuntimeSettingOption>();
            setting.rectTransform.anchoredPosition += new Vector2(0, positionOffset);

            positionOffset -= 30f;
        }
        
        foreach (var (def, val) in Plugin.Config.File)
        {
            if (def.Section != categoryName)
                continue;

            if (AccessTools.Property(typeof(Config), def.Key) is not { } property)
                continue;

            if (property.GetCustomAttribute<ConfigDescriptorAttribute>() is not { } descriptor)
                continue;

            var setting = (val.SettingType == typeof(bool)
                ? Instantiate(AssetCollection.BoolSetting, scrollerTransform)
                : Instantiate(AssetCollection.SliderSetting, scrollerTransform)).GetComponent<SettingOption>();

            setting.settingCategory = categoryName;
            setting.settingName = def.Key;
            setting.settingDisplayName = descriptor.CustomName;
            setting.rectTransform.anchoredPosition += new Vector2(0, positionOffset);

            if (setting.GetComponent<MenuTwoOptions>() is { } menuTwoOptions)
                menuTwoOptions.enabled = true; // We have to defer the OnEnable until after the config definition has been set

            if (val.SettingType == typeof(bool))
            {
                var menuOptions = setting.GetComponent<MenuTwoOptions>();

                menuOptions.option1Text = descriptor.TrueText;
                menuOptions.option2Text = descriptor.FalseText;
            }
            else if (val.SettingType != typeof(bool))
            {
                var slider = setting.GetComponent<FloatMenuSlider>();

                slider.displayPercentage = descriptor.Percentage;
                slider.stringAtEndOfValue = descriptor.Percentage ? "%" : descriptor.Suffix;
                slider.buttonSegmentJump = descriptor.StepSize;
                slider.pointerSegmentJump = descriptor.PointerSize;

                if (val.SettingType.IsEnum)
                {
                    foreach (var variant in Enum.GetNames(val.SettingType))
                        slider.customOptions.Add(new FloatMenuSlider.CustomOption
                        {
                            customOptionText = Utils.ToHumanReadable(variant)
                        });

                    slider.hasCustomOptions = true;
                    slider.currentValue = (int)val.BoxedValue;
                    slider.isInteger = true;
                    slider.hasBar = !descriptor.EnumDisableBar;
                    slider.wrapAround = true;
                    
                    positionOffset -= 30f;
                    continue;
                }
                
                switch (val.Description.AcceptableValues)
                {
                    case AcceptableValueRange<float> rangeFloat:
                        slider.startValue = rangeFloat.MinValue;
                        slider.endValue = rangeFloat.MaxValue;
                        slider.currentValue = (float)val.BoxedValue;
                        break;
                    case AcceptableValueRange<int> rangeInt:
                        slider.startValue = rangeInt.MinValue;
                        slider.endValue = rangeInt.MaxValue;
                        slider.currentValue = (int)val.BoxedValue;
                        slider.isInteger = true;
                        break;
                    default:
                        positionOffset -= 30f;
                        continue;
                }
            }
            
            positionOffset -= 30f;
        }
    }
}