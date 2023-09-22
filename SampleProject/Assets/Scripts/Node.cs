using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField]
    private GameObject node;
    [SerializeField]
    private GameObject parentEdge;
    
    private bool _expandable = false;
    private bool _enabled = false;

    private Material _nodeMaterial;
    private Material _parentEdgeMaterial;
    
    private Color _nodeColor;
    private Color _edgeColor;

    private void Awake()
    {
        _nodeMaterial = node.GetComponent<MeshRenderer>().material;
        _parentEdgeMaterial = parentEdge.GetComponent<MeshRenderer>().material;

        _nodeColor = _nodeMaterial.color;
        _edgeColor = _parentEdgeMaterial.color;
    }

    void Start()
    {
        UpdateColor();
    }
    
    private void UpdateColor()
    {
        var alpha = !_enabled ? 0.2f : 1.0f;
        _nodeMaterial.color = _expandable ? Color.green : _nodeColor.WithAlphaMultiplied(alpha);
        _parentEdgeMaterial.color = _edgeColor.WithAlphaMultiplied(alpha);
    }

    public void EnableNode()
    {
        _enabled = true;
        UpdateColor();
    }

    public void DisableNode()
    {
        _enabled = false;
        UpdateColor();
    }

    public void SetExpandable(bool expandable)
    {
        _expandable = expandable;
        UpdateColor();
    }
}