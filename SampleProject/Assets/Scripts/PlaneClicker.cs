using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaneClicker : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        var hitPos = eventData.pointerCurrentRaycast.worldPosition;
        var pos = new Vector2Int((int)Math.Round(hitPos.x), (int)Math.Round(hitPos.z));

        Debug.Log(hitPos);
        Debug.Log(pos);
        var generator = FindObjectOfType<Generator>().generator;
        if (!generator.GetExpandablePositions().Contains(pos))
            return;
        generator.Expand(pos);
    }
}