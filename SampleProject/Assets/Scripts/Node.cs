using Unity.VisualScripting;
using UnityEngine;

public class Node : MonoBehaviour
{
    [SerializeField] private GameObject node;
    [SerializeField] private GameObject parentEdge;

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
        _nodeMaterial.color = _expandable
            ? Color.green
            : new Color(_nodeColor.r * alpha, _nodeColor.g * alpha, _nodeColor.b * alpha);
        _parentEdgeMaterial.color = new Color(_edgeColor.r * alpha, _edgeColor.g * alpha, _edgeColor.b * alpha);
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

    public void SetEnableable(bool expandable)
    {
        _expandable = expandable;
        UpdateColor();
    }
}