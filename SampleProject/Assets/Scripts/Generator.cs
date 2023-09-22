using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FranticFortressFrenzy.WaveFunctionCollapse;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class NewBehaviourScript : MonoBehaviour
{
    private GridPathGenerator<Vector2Int> _generator;
    private Dictionary<Vector2Int, GameObject> _objects = new();

    void Awake()
    {
        _generator = new GridPathGenerator<Vector2Int>(1, 1, new Vector2IntNeighborGetter(), Vector2Int.zero);
    }

    private void Start()
    {
        _generator.Initialize();
    }

    private void OnEnable()
    {
        _generator.onNodeAdded.AddListener(OnNodeAdded);
        _generator.onNodeRemoved.AddListener(OnNodeRemoved);
        _generator.onNodeChildrenChanged.AddListener(OnNodeChildrenChanged);
        _generator.onNodeEnabled.AddListener(OnNodeEnabled);
        _generator.onNodeDisabled.AddListener(OnNodeDisabled);
    }

    private void OnDisable()
    {
        _generator.onNodeAdded.RemoveListener(OnNodeAdded);
        _generator.onNodeRemoved.RemoveListener(OnNodeRemoved);
        _generator.onNodeChildrenChanged.RemoveListener(OnNodeChildrenChanged);
        _generator.onNodeEnabled.RemoveListener(OnNodeEnabled);
        _generator.onNodeDisabled.RemoveListener(OnNodeDisabled);
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

    void Update()
    {
        if (Input.GetMouseButtonDown((int)MouseButton.LeftMouse))
        {
            var positions = _generator.GetExpandablePositions().ToList();
            _generator.Expand(positions[Random.Range(0, positions.Count)]);
        }
    }
}