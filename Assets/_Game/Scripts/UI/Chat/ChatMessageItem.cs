using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChatMessageItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private void Awake()
    {
        // SELF INIT FOR TEST
        // Initialize("GRIN", "Epic Boss Fight ubey Grishu i viigray");
    }

    public void Initialize(string playerName, string messageText)
    {
        _text.SetText($"<color=#707070>{playerName}:</color> {messageText}");
    }
    
}
