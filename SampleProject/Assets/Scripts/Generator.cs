using System;
using System.Collections.Generic;
using System.Linq;
using FranticFortressFrenzy.WaveFunctionCollapse;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Generator : MonoBehaviour
{
    public GridPathGenerator<Vector2Int> generator;
    private Dictionary<Vector2Int, GameObject> _objects = new();
    [SerializeField] private GameObject rootNodePrefab;
    [SerializeField] private GameObject nodePrefab;

    void Awake()
    {
        generator = new GridPathGenerator<Vector2Int>(1, 1, new Vector2IntNeighborGetter(), Vector2Int.zero);
    }

    private void Start()
    {
        generator.Initialize();
    }

    private void OnEnable()
    {
        generator.onNodeAdded.AddListener(OnNodeAdded);
        generator.onNodeRemoved.AddListener(OnNodeRemoved);
        generator.onNodeEnabled.AddListener(OnNodeEnabled);
        generator.onNodeDisabled.AddListener(OnNodeDisabled);
    }

    private void OnDisable()
    {
        generator.onNodeAdded.RemoveListener(OnNodeAdded);
        generator.onNodeRemoved.RemoveListener(OnNodeRemoved);
        generator.onNodeEnabled.RemoveListener(OnNodeEnabled);
        generator.onNodeDisabled.RemoveListener(OnNodeDisabled);
    }

    private void OnNodeDisabled(Vector2Int arg0)
    {
        throw new NotImplementedException();
    }

    private void OnNodeEnabled(Vector2Int arg0)
    {
        throw new NotImplementedException();
    }

    private void OnNodeChildrenChanged(Vector2Int arg0, PathNode<Vector2Int, NodeData> arg1)
    {
        throw new NotImplementedException();
    }

    private void OnNodeRemoved(Vector2Int pos)
    {
        Destroy(_objects[pos]);
    }

    private void OnNodeAdded(Vector2Int pos, PathNode<Vector2Int, NodeData> parent)
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