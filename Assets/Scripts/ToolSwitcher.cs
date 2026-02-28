using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities; // Makes .Call work.
using System;

public class ToolSwitcher : MonoBehaviour
{
    
    [Header("Your Tools!")]
    public GameObject[] tools; // List of all of the tools that you will use while playing.
    private int actualID; // Number identifier for the actual tool being used.
    private IDisposable anyButtonListener; // Variable so OnDisable knows what do destroy.

    void Start() // The best way to start this is through making the player have no tools on it's hands (code -1).
    {
        SwitchTool(-1);
        actualID = -1;
    }

    // Update is called once per frame
    void Update()
    {
        // Used to switch tools when scrolling the mouse wheel.
        if (Mouse.current != null) // Make sure mouse signal is detected.
        {
            float scrollValue = Mouse.current.scroll.ReadValue().y;

            if (scrollValue > 0) // Scroll up.
            {
                CycleTool(1);
            }
            else if (scrollValue < 0) // Scroll down.
            {
                CycleTool(-1);
            }
        }
    }

    // Helper function to handle mouse wheel scrolling.
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

        
        if (nextID >= tools.Length)
        {
            nextID = 0;
        }
        
        else if (nextID < 0)
        {
            nextID = tools.Length - 1;
        }

        SwitchTool(nextID);
    }

    void OnEnable()
    {
        anyButtonListener = InputSystem.onAnyButtonPress.Call(control => // Listens to any button pressed.
        {
            if (control.device is Keyboard) // Filter to only keyboard pressed keys.
            {
                string keyName = control.name; // For example, "w", "space", "escape"

                switch (keyName)
                {
                    case "1": SwitchTool(0); break; // Lantern.
                    case "2": SwitchTool(1); break; // Vacuum.
                    case "3": SwitchTool(2); break; // EMF reader.
                    default: break; // Do nothing.
                }
            }
        });
    }

    void OnDisable() // Important to stop listening to avoid too much memory usage.
    {
        anyButtonListener?.Dispose();
    }

    void SwitchTool(int index)
    {
        if (index < -1 || index >= tools.Length) return; // Safety check, do not try to select a tool that doesn't exist.

        if (index == actualID || index == -1) // If pressed the button for the active tool or passed the -1 value as index, unselect every weapon.
        {
            for (int i = 0; i < tools.Length; i++)
            {
                tools[i].SetActive(false);
            }
            actualID = -1;
            return;
        }

        for (int i = 0; i < tools.Length; i++)
        {
            if (i == index)
            {
                tools[i].SetActive(true);
                actualID = index;
            }
            else
            {
                tools[i].SetActive(false);
            }
        }
    }

}
