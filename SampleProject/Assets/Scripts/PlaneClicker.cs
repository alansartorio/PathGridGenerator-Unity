using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlaneClicker : MonoBehaviour, IPointerClickHandler, IDragHandler, IBeginDragHandler, IScrollHandler
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
        cam.transform.position -= new Vector3(delta.x, 0, delta.y) * 0.0015f * cam.transform.position.y;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStart = eventData.position;
    }

    public void OnScroll(PointerEventData eventData)
    {
        var newPos = Camera.main.transform.position;
        newPos.y -= eventData.scrollDelta.y;
        if (newPos.y < 1)
            newPos.y = 1;
        Camera.main.transform.position = newPos;
    }
}