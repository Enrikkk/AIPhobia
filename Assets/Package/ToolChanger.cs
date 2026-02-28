using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities; 
using UnityEngine.UI; 
using System;

public class ToolChanger : MonoBehaviour
{
    [Header("Your Tools!")]
    public GameObject[] tools; 
    
    [Header("UI Frames (The Borders)")]
    public Image[] toolFrames; 
    public Color frameSelected = Color.yellow; // Turns yellow when holding the tool
    public Color frameUnselected = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark gray when not holding

    [Header("UI Icons (The Art)")]
    public Graphic[] toolIcons; // 'Graphic' works for both Image and RawImage!
    public Color iconSelected = new Color(1f, 1f, 1f, 1f); // 100% Brightness and opaque
    public Color iconUnselected = new Color(0.5f, 0.5f, 0.5f, 0.5f); // 50% darker and semi-transparent
    // --------------------

    private int actualID; 
    private IDisposable anyButtonListener; 

    void Start() 
    {
        actualID = -1;
        SwitchTool(-1);
    }

    void Update()
    {
        if (Mouse.current != null) 
        {
            float scrollValue = Mouse.current.scroll.ReadValue().y;
            if (scrollValue > 0) CycleTool(1);
            else if (scrollValue < 0) CycleTool(-1);
        }
    }

    void CycleTool(int direction)
    {
        if (tools.Length == 0) return;

        if (actualID == -1)
        {
            if (direction > 0) SwitchTool(0);
            else SwitchTool(tools.Length - 1);
            return;
        }
        
        int nextID = actualID + direction;
        if (nextID >= tools.Length) nextID = 0;
        else if (nextID < 0) nextID = tools.Length - 1;

        SwitchTool(nextID);
    }

    void OnEnable()
    {
        anyButtonListener = InputSystem.onAnyButtonPress.Call(control => 
        {
            if (control.device is Keyboard) 
            {
                switch (control.name)
                {
                    case "1": SwitchTool(0); break; 
                    case "2": SwitchTool(1); break; 
                    case "3": SwitchTool(2); break; 
                }
            }
        });
    }

    void OnDisable() 
    {
        anyButtonListener?.Dispose();
    }

    void SwitchTool(int index)
    {
        if (index < -1 || index >= tools.Length) return; 

        if (index == actualID || index == -1) 
        {
            for (int i = 0; i < tools.Length; i++) tools[i].SetActive(false);
            actualID = -1;
            UpdateUI(); 
            return;
        }

        for (int i = 0; i < tools.Length; i++)
        {
            if (i == index)
            {
                tools[i].SetActive(true);
                actualID = index;
            }
            else tools[i].SetActive(false);
        }

        UpdateUI(); 
    }

    void UpdateUI()
    {
        // Safety check to make sure you assigned everything in the Inspector
        if (toolFrames.Length == 0 || toolIcons.Length == 0) return;

        for (int i = 0; i < tools.Length; i++)
        {
            if (i == actualID)
            {
                // Active Tool: Bright Frame, Bright Icon
                toolFrames[i].color = frameSelected;
                toolIcons[i].color = iconSelected;
            }
            else
            {
                // Inactive Tool: Dark Frame, Dim Icon
                toolFrames[i].color = frameUnselected;
                toolIcons[i].color = iconUnselected;
            }
        }
    }
}