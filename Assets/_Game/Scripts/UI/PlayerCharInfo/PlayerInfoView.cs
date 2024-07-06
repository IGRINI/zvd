using System.Collections;
using System.Collections.Generic;
using System.Drawing.Design;
using TMPro;
using UnityEngine;


public class PlayerInfoView : MonoBehaviour
{
    [SerializeField] private TMP_Text _playerName;
    [SerializeField] private GameObject _onlineMark;
    [SerializeField] private TMP_Text _rankNumber;

    public void Initialize(string playerName, int rankNumber)
    {
        _playerName.SetText(playerName);
        _rankNumber.SetText($"Rank {rankNumber}");
    }

    public void SetOnline(bool state)
    {
        _onlineMark.SetActive(state);
    }
}
