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
        generator.onNodeChildrenChanged.AddListener(OnNodeChildrenChanged);
        generator.onNodeEnabled.AddListener(OnNodeEnabled);
        generator.onNodeDisabled.AddListener(OnNodeDisabled);
    }

    private void OnDisable()
    {
        generator.onNodeAdded.RemoveListener(OnNodeAdded);
        generator.onNodeRemoved.RemoveListener(OnNodeRemoved);
        generator.onNodeChildrenChanged.RemoveListener(OnNodeChildrenChanged);
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

    private void OnNodeAdded(Vector2Int pos)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(transform);
        cube.transform.localPosition = new Vector3(pos.x, 0, pos.y);
        _objects.Add(pos, cube);
    }
}