using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FranticFortressFrenzy.WaveFunctionCollapse;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaneClicker : MonoBehaviour, IPointerClickHandler
{
    private Generator _generatorObject;

    private void Start()
    {
        _generatorObject = FindObjectOfType<Generator>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var hitPos = eventData.pointerCurrentRaycast.worldPosition;
        var pos = new Vector2Int((int)Math.Round(hitPos.x), (int)Math.Round(hitPos.z));

        _generatorObject.Expand(pos);
    }
}