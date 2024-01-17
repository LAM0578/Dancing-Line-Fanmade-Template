using System;
using System.Reflection;
using DancingLineSample.Attributes;
using DancingLineSample.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ChangeButtonColors : ScriptableWizard
{
#pragma warning disable
    
    [SerializeField] private GameObject _gameObject;
    [Space]
    [SerializeField] private ColorBlock _colorBlock = new ColorBlock()
    {
        normalColor = Color.white,
        highlightedColor = Color.white,
        pressedColor = Color.white.WithAlpha(200),
        selectedColor = Color.white,
        disabledColor = Color.white.WithAlpha(128),
        colorMultiplier = 1,
        fadeDuration = 0.1f
    };
    
#pragma warning restore

    private void ApplyColorAtButtons()
    {
        var buttons = _gameObject.GetComponentsInChildren<Button>(true);
        Debug.Log("Button Count: " + buttons.Length);
        foreach (var button in buttons)
        {
            button.colors = _colorBlock;
        }
    }

    [MenuItem("EditorTools/ChangeButtonColors")]
    private static void CreateWizard()
    {
        var window = DisplayWizard<ChangeButtonColors>(
            "Change Button Colors", "", "Apply");
        window.Show();
    }

    private void OnWizardUpdate()
    {
        helpString = "Select the parent GameObject and set the button colors.";
    }
    
    private void OnWizardOtherButton()
    {
        ApplyColorAtButtons();
    }
}