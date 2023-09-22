using System;
using System.Collections.Generic;
using System.Linq;
using FranticFortressFrenzy.WaveFunctionCollapse;
using UnityEngine;

public class Generator : MonoBehaviour
{
    public GridPathGenerator<Vector2Int> generator;
    private Dictionary<Vector2Int, GameObject> _objects = new();
    [SerializeField] private GameObject rootNodePrefab;
    [SerializeField] private GameObject nodePrefab;

    void Awake()
    {
        generator = new GridPathGenerator<Vector2Int>(5, 1, new Vector2IntNeighborGetter(), Vector2Int.zero);
    }

    private void Start()
    {
        var delta = generator.Initialize();
        ApplyDelta(delta);
    }

    public void EnableNodeExpand(Vector2Int pos)
    {
        if (!generator.GetEnableablePositions().Contains(pos))
            return;
        var delta = generator.EnableNode(pos);
        ApplyDelta(delta);
    }

    private void ApplyDelta(NodesDelta<Vector2Int> delta)
    {
        foreach (var node in delta.removedNodes)
        {
            RemoveNode(node.Position);
        }

        foreach (var added in delta.addedNodes)
        {
            AddNode(added.node.Position, added.parent);
        }

        foreach (var enabled in delta.enabledNodes)
        {
            EnableNode(enabled.Position);
        }

        foreach (var node in _objects.Values)
        {
            node.GetComponent<Node>()?.SetEnableable(false);
        }

        foreach (var node in generator.GetEnableablePositions().Select(p => _objects[p]))
        {
            node.GetComponent<Node>()?.SetEnableable(true);
        }
    }

    private void EnableNode(Vector2Int pos)
    {
        _objects[pos].GetComponent<Node>()?.EnableNode();
    }

    private void RemoveNode(Vector2Int pos)
    {
        Destroy(_objects[pos]);
        _objects.Remove(pos);
    }

    private void AddNode(Vector2Int pos, PathNode<Vector2Int, NodeData> parent)
    {
        var prefab = parent == null ? rootNodePrefab : nodePrefab;
        var node = Instantiate(prefab, transform, true);
        node.transform.localPosition = new Vector3(pos.x, 0, pos.y);
        if (parent != null)
        {
            var delta = pos - parent.Position;
            var angle = (delta.x, delta.y) switch
            {
                (1, 0) => 0,
                (0, -1) => 1,
                (-1, 0) => 2,
                (0, 1) => 3,
                _ => throw new ArgumentOutOfRangeException()
            };
            node.transform.localRotation = Quaternion.Euler(0, angle * 90f, 0);
        }

        _objects.Add(pos, node);
    }
}