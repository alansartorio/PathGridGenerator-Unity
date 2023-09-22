using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaneClicker : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler
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

        _generatorObject.EnableNodeExpand(pos);
    }

    private Vector2 dragStart;

    public void OnDrag(PointerEventData eventData)
    {
        var position = eventData.position;
        Vector2 delta = position - dragStart;
        var cam = Camera.main;
        dragStart += delta;
        cam.transform.position -= new Vector3(delta.x, 0, delta.y) * 0.015f;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStart = eventData.position;
    }
}