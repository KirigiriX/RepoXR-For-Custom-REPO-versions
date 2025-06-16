using RepoXR.Assets;
using RepoXR.ThirdParty.MRTK;
using UnityEngine;

namespace RepoXR.UI.Menu;

public class InputKeyboard : MonoBehaviour
{
    public static InputKeyboard instance;
    
    public MenuTextInput menuInput;
    
    private NonNativeKeyboard keyboard;

    private float animLerp;
    private bool closing;
    
    private void Awake()
    {
        if (instance)
            Destroy(instance.gameObject);

        instance = this;
        
        keyboard = GetComponentInChildren<NonNativeKeyboard>(true);
        keyboard.SubmitOnEnter = false;

        transform.localScale = Vector3.zero;
    }

    private void Start()
    {
        keyboard.OnKeyboardValueKeyPressed += OnKeyPressed;
        keyboard.OnKeyboardFunctionKeyPressed += OnFunctionKeyPressed;
        
        keyboard.PresentKeyboard();
    }

    private void OnDestroy()
    {
        keyboard.OnKeyboardValueKeyPressed -= OnKeyPressed;
        keyboard.OnKeyboardFunctionKeyPressed -= OnFunctionKeyPressed;
    }

    private void Update()
    {
        switch (animLerp)
        {
            case < 1 when !closing:
                animLerp += Time.deltaTime * 4;
                break;
            case > 0 when closing:
                animLerp -= Time.deltaTime * 4;
                break;
        }
            
        animLerp = Mathf.Clamp01(animLerp);
        transform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one,
            AssetCollection.KeyboardAnimation.Evaluate(animLerp));

        if (animLerp == 0 && closing)
            Destroy(gameObject);
    }

    public void Close()
    {
        closing = true;
    }
    
    private void OnKeyPressed(KeyboardValueKey key)
    {
        menuInput.textCurrent += keyboard.IsShifted ? key.ShiftValue : key.Value;
    }

    private void OnFunctionKeyPressed(KeyboardKeyFunc key)
    {
        switch (key.ButtonFunction)
        {
            case KeyboardKeyFunc.Function.Backspace:
                menuInput.textCurrent = menuInput.textCurrent.Remove(Mathf.Max(menuInput.textCurrent.Length - 1, 0));
                
                break;
            
            case KeyboardKeyFunc.Function.Enter:
                if (menuInput.GetComponentInParent<MenuPageServerListCreateNew>() is {} createPage)
                    createPage.ButtonConfirm();
                else if (menuInput.GetComponentInParent<MenuPageServerListSearch>() is {} searchPage)
                    searchPage.ButtonConfirm();
                    
                break;
        }

        // No other handlers needed
    }
}