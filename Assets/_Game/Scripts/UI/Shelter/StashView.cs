using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StashView : MonoBehaviour
{
    [SerializeField] private ItemsList _chestList;
    [SerializeField] private ItemsList _stashList;

    private void Awake()
    {
        _chestList.Initialize(Vector2.one * 16f);
        _stashList.Initialize(new Vector2(20f, 16f));
    }
}
