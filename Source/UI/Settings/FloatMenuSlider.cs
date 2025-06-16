using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace RepoXR.UI.Settings;

public class FloatMenuSlider : MonoBehaviour
{
    [Serializable]
    public class CustomOption
    {
        public string customOptionText;
        public UnityEvent onOption;
        public int customValueInt;
    }
    
    public Transform barSize;
    public Transform barPointer;
    public RectTransform barSizeRectTransform;
    public Transform settingsBar;

    public TextMeshProUGUI segmentText;
    public TextMeshProUGUI segmentMaskText;
    public RectTransform maskRectTransform;

    public float startValue;
    public float endValue;

    public string stringAtStartOfValue;
    public string stringAtEndOfValue;

    public float buttonSegmentJump = 1;
    public float pointerSegmentJump = 1;

    public bool displayPercentage;
    public bool isInteger;
    public bool hasBar = true;
    public bool wrapAround;
    public bool hasCustomOptions;

    [Space] 
    public UnityEvent onChange;
    public List<CustomOption> customOptions;
    
    public float currentValue;
    
    private float prevCurrentValue;
    private bool valueChangedImpulse;

    private bool hovering;

    private MenuSelectableElement menuSelectableElement;
    private RectTransform rectTransform;
    private MenuPage parentPage;

    private Vector3 originalPosition;

    private bool startPositionSetup;
    private int settingSegments;

    private float PercentageValue => Mathf.InverseLerp(startValue, endValue, currentValue);
    private string DisplayValue => displayPercentage ? $"{PercentageValue * 100}" : $"{currentValue}";

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        parentPage = GetComponentInParent<MenuPage>();
        menuSelectableElement = GetComponent<MenuSelectableElement>();
        barSizeRectTransform = barSize.GetComponent<RectTransform>();
        
        settingSegments = Mathf.RoundToInt(endValue - startValue);

        if (hasCustomOptions)
        {
            settingSegments = Mathf.Max(customOptions.Count - 1, 1);
            startValue = 0;
            endValue = customOptions.Count - 1;
            buttonSegmentJump = 1;
            
            if (Mathf.Max(customOptions.Count - 1, 1) != settingSegments)
                Debug.LogWarning("Segment text count is not equal to setting segments count");
            else
            {
                var index = Mathf.RoundToInt(currentValue);
                var text = customOptions[index].customOptionText;
                if (text.Length > 16)
                    text = text[..16] + "...";

                segmentText.text = text;
            }
        }
        else
            segmentText.text = $"{stringAtStartOfValue}{DisplayValue}{stringAtEndOfValue}";

        segmentText.enableAutoSizing = false;
        segmentMaskText.enableAutoSizing = false;
        
        if (!hasBar && segmentText)
            Destroy(segmentText.gameObject);

        prevCurrentValue = currentValue;

        SetStartPositions();
    }

    public void SetStartPositions()
    {
        if (startPositionSetup)
            return;

        barSizeRectTransform.localPosition = new Vector3(barSizeRectTransform.localPosition.x + 3,
            barSizeRectTransform.localPosition.y, barSizeRectTransform.localPosition.z);
        originalPosition = rectTransform.position;
        originalPosition = new Vector3(originalPosition.x, originalPosition.y - 1.01f, originalPosition.z);
        
        startPositionSetup = true;
    }

    private void Update()
    {
        if (!Mathf.Approximately(prevCurrentValue, currentValue) || valueChangedImpulse)
        {
            valueChangedImpulse = false;
            onChange.Invoke();
            prevCurrentValue = currentValue;
        }

        if (hasBar)
        {
            settingsBar.localScale = Vector3.Lerp(settingsBar.localScale,
                new Vector3(PercentageValue, settingsBar.localScale.y, settingsBar.localScale.z), 20 * Time.deltaTime);
            maskRectTransform.sizeDelta = new Vector2(barSizeRectTransform.sizeDelta.x * PercentageValue,
                maskRectTransform.sizeDelta.y);
        }

        if (SemiFunc.UIMouseHover(parentPage, barSizeRectTransform, menuSelectableElement.menuID, 5, 5))
        {
            if (!hovering)
                MenuManager.instance.MenuEffectHover(SemiFunc.MenuGetPitchFromYPos(rectTransform));

            hovering = true;

            SemiFunc.MenuSelectionBoxTargetSet(parentPage, barSizeRectTransform, new Vector2(-3, 0),
                new Vector2(20, 10));

            if (hasBar)
                PointerLogic();
            else if (XRRayInteractorManager.Instance?.GetTriggerDown() ?? UnityEngine.Input.GetMouseButtonDown(0))
            {
                MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Action, parentPage);
                OnIncrease();
            }
        }
        else
        {
            hovering = false;
            if (barPointer.gameObject.activeSelf)
            {
                barPointer.localPosition = new Vector3(-999, barPointer.localPosition.y, barPointer.localPosition.z);
                barPointer.gameObject.SetActive(false);
            }
        }

        if (segmentMaskText && segmentMaskText.text != segmentText.text)
            segmentMaskText.text = segmentText.text;
    }

    private void PointerLogic()
    {
        if (!barPointer)
            return;
        
        if (!barPointer.gameObject.activeSelf)
            barPointer.gameObject.SetActive(true);

        var position = SemiFunc.UIMouseGetLocalPositionWithinRectTransform(barSizeRectTransform);
        var numSteps = (endValue - startValue) / pointerSegmentJump;
        var percentage = Mathf.Clamp01(position.x / barSizeRectTransform.sizeDelta.x);
        percentage = Mathf.Round(percentage * numSteps) / numSteps;
        
        var newXPos =
            Mathf.Clamp(barSizeRectTransform.localPosition.x + percentage * barSizeRectTransform.sizeDelta.x,
                barSizeRectTransform.localPosition.x,
                barSizeRectTransform.localPosition.x + barSizeRectTransform.sizeDelta.x);

        barPointer.localPosition = new Vector3(newXPos - 2, barPointer.localPosition.y, barPointer.localPosition.z);

        if (XRRayInteractorManager.Instance?.GetTriggerButton()?? UnityEngine.Input.GetMouseButton(0))
        {
            prevCurrentValue = currentValue;
            currentValue = Mathf.Lerp(startValue, endValue, percentage);
            currentValue = startValue +
                           Mathf.RoundToInt((currentValue - startValue) / pointerSegmentJump) * pointerSegmentJump;
            currentValue = Mathf.Clamp(currentValue, startValue, endValue);

            if (!Mathf.Approximately(prevCurrentValue, currentValue))
                MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, parentPage);

            var index = Mathf.RoundToInt(currentValue);

            if (hasCustomOptions && index < customOptions.Count)
                segmentText.text = customOptions[index].customOptionText;
            else
                segmentText.text = $"{stringAtStartOfValue}{DisplayValue}{stringAtEndOfValue}";

            if (hasCustomOptions)
            {
                UpdateSegmentTextAndValue();
            }
        }
    }

    private void UpdateSegmentTextAndValue()
    {
        if (hasCustomOptions)
        {
            var value = Mathf.RoundToInt(currentValue);
            if (value < customOptions.Count)
            {
                var text = customOptions[value].customOptionText;
                if (text.Length > 16)
                    text = text[..16] + "...";

                segmentText.text = text;
            }
        }
        else
            segmentText.text = stringAtStartOfValue + DisplayValue + stringAtEndOfValue;
    }

    public void OnIncrease()
    {
        valueChangedImpulse = true;

        if (wrapAround)
            currentValue = Mathf.Approximately(currentValue, endValue) ? startValue : currentValue + buttonSegmentJump;
        else
            currentValue += buttonSegmentJump;
        
        currentValue = startValue +
                       Mathf.RoundToInt((currentValue - startValue) / buttonSegmentJump) * buttonSegmentJump;
        currentValue = Mathf.Clamp(currentValue, startValue, endValue);
        
        UpdateSegmentTextAndValue();
    }

    public void OnDecrease()
    {
        valueChangedImpulse = true;

        if (wrapAround)
            currentValue = Mathf.Approximately(currentValue, startValue) ? endValue : currentValue - buttonSegmentJump;
        else
            currentValue -= buttonSegmentJump;
        
        currentValue = startValue +
                       Mathf.RoundToInt((currentValue - startValue) / buttonSegmentJump) * buttonSegmentJump;
        currentValue = Mathf.Clamp(currentValue, startValue, endValue);
        
        UpdateSegmentTextAndValue();
    }

    public void SetBarScaleInstant()
    {
        settingsBar.localScale = new Vector3(PercentageValue, settingsBar.localScale.y, settingsBar.localScale.z);
    }
}