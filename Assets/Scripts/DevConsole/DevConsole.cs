using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DevConsole : MonoBehaviour
{
    public static DevConsole Instance;

    [Header("UI")]
    public GameObject consoleUI;
    public TMP_InputField inputField;
    public TMP_Text logText;
    public ScrollRect scrollRect;

    [Header("Inline Autocomplete")]
    public TMP_Text ghostText;
    public GameObject ghostBackground;

    [Header("Commands")]
    public string[] commands = { "help", "spawn", "give", "clear", "echo" };

    List<string> autoMatches = new();
    int autoIndex = 0;

    List<string> history = new();
    int historyIndex = -1;

    bool isOpen;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        consoleUI.SetActive(false);
        isOpen = false;

        Debug.Log($"[DevConsole] inputField={inputField != null}, logText={logText != null}, scrollRect={scrollRect != null}");
    }

    void OnEnable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += OnTextInput;
    }

    void OnDisable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
    }

    void Update()
    {
        if (!isOpen) return;

        HandleHistory();
        HandleAutocomplete();
        HandleSubmit();
        HandleClose();

        UpdateGhostText();
    }

    void OnTextInput(char c)
    {
        if (c == '/')
        {
            Toggle();
        }
    }

    void HandleSubmit()
    {
        if (!Keyboard.current.enterKey.wasPressedThisFrame) return;

        string cmd = inputField.text.Trim();
        if (string.IsNullOrEmpty(cmd)) return;

        Log($"> {cmd}");
        SubmitCommand(cmd);

        inputField.text = "";
        inputField.ActivateInputField();
    }

    void HandleClose()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Toggle();
        }
    }

    void HandleHistory()
    {
        if (history.Count == 0) return;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            historyIndex = Mathf.Max(0, historyIndex - 1);
            inputField.text = history[historyIndex];
            inputField.caretPosition = inputField.text.Length;
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            historyIndex = Mathf.Min(history.Count, historyIndex + 1);

            inputField.text =
                historyIndex >= history.Count ? "" : history[historyIndex];

            inputField.caretPosition = inputField.text.Length;
        }
    }

    void UpdateGhostText()
    {
        ghostText.text = "";
        ghostBackground.SetActive(false);

        string current = inputField.text;
        if (string.IsNullOrEmpty(current)) return;

        foreach (var cmd in commands)
        {
            if (cmd.StartsWith(current, StringComparison.OrdinalIgnoreCase))
            {
                string remaining = cmd.Substring(current.Length);
                ghostText.text =
                    current + $"<color=#9E9E9E>{remaining}</color>";

                ghostBackground.SetActive(true);
                return;
            }
        }
    }

    void HandleAutocomplete()
    {
        if (!Keyboard.current.tabKey.wasPressedThisFrame) return;
        if (autoMatches.Count == 0) return;

        inputField.text = autoMatches[autoIndex];
        inputField.caretPosition = inputField.text.Length;

        autoIndex = (autoIndex + 1) % autoMatches.Count;
    }

    void SubmitCommand(string cmd)
    {
        history.Add(cmd);
        historyIndex = history.Count;

        CommandRegistry.Execute(cmd);
    }

    void Toggle()
    {
        isOpen = !isOpen;
        consoleUI.SetActive(isOpen);

        if (isOpen)
        {
            inputField.text = "";
            inputField.Select();
            inputField.ActivateInputField();
        }
    }

    public void Log(string msg)
    {
        logText.text += msg + "\n";
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void ClearLog()
    {
        logText.text = "";
    }
}
