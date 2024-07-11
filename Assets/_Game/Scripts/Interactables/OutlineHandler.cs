using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using Utils.Outline;

public class OutlineHandler : MonoBehaviour
{
    private List<OutlineFx> _outlineFxes = new();
    
    private void Awake()
    {
        var meshRenderers = transform.GetComponentsInChildren<Renderer>();
        
        foreach (var meshRenderer in meshRenderers)
        {
            if(meshRenderer is VFXRenderer)
                continue;

            var outline = meshRenderer.gameObject.AddComponent<OutlineFx>();
            outline.enabled = false;
            _outlineFxes.Add(outline);
        }
    }

    public void EnableOutline()
    {
        ChangeOutlineState(true);
    }

    public void DisableOutline()
    {
       ChangeOutlineState(false);
    }

    private void ChangeOutlineState(bool state)
    {
        foreach (var outline in _outlineFxes)
        {
            outline.enabled = state;
        }
    }
}
