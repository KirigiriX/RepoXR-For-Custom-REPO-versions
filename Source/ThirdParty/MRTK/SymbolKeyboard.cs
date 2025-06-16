// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#pragma warning disable

using UnityEngine;

namespace RepoXR.ThirdParty.MRTK;

/// <summary>
/// This class switches back and forth between two symbol boards that otherwise do not fit on the keyboard entirely
/// </summary>
public class SymbolKeyboard : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button m_PageBck = null;
    [SerializeField] private UnityEngine.UI.Button m_PageFwd = null;

    private NonNativeKeyboard m_Keyboard;

    private void Awake()
    {
        m_Keyboard = GetComponentInParent<NonNativeKeyboard>();
    }

    private void Update()
    {
        // Visual reflection of state.
        m_PageBck.interactable = m_Keyboard.IsShifted;
        m_PageFwd.interactable = !m_Keyboard.IsShifted;
    }
}