using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor.Graphs.AnimationBlendTree;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace FranticFortressFrenzy.WaveFunctionCollapse
{
    public class GridPathGenerator<TC> where TC : IEquatable<TC>
    {
        private Tree<TC, NodeData> _tree;
        public PathNode<TC, NodeData> Root => _tree.Root;
        private INeighborGetter<TC> _neighborGetter;

        public readonly UnityEvent<TC> onNodeAdded = new();
        public readonly UnityEvent<TC> onNodeRemoved = new();
        public readonly UnityEvent<TC, PathNode<TC, NodeData>> onNodeChildrenChanged = new();
        public readonly UnityEvent<TC> onNodeEnabled = new();
        public readonly UnityEvent<TC> onNodeDisabled = new();

        public int MaxRetries { get; private set; }
        public int TargetLookAhead { get; private set; }

        public GridPathGenerator(int targetLookAhead, int maxRetries, INeighborGetter<TC> neighborGetter,
            TC rootPosition)
        {
            _neighborGetter = neighborGetter;
            TargetLookAhead = targetLookAhead;
            MaxRetries = maxRetries;
            _tree = new Tree<TC, NodeData>(rootPosition, new NodeData());
        }

        public void Step()
        {
        }

        public void Initialize()
        {
            onNodeAdded.Invoke(Root.Position);
        }

        public void Expand(TC position)
        {
            var node = _tree.GetNodeInPosition(position);
            if (node == null)
                throw new ArgumentException("Position does not exist on path tree");

            var neighs = new HashSet<TC>(_neighborGetter.GetNeighbors(node.Position));

            if (node.Parent != null)
                neighs.Remove(node.Parent.Position);

            neighs.RemoveWhere(n => _tree.GetNodeInPosition(n) != null);

            if (neighs.Count > 0)
            {
                var amount = Random.Range(1, neighs.Count + 1);
                var neighArray = neighs.ToArray();

                var chosenNeighs = new List<TC>();
                for (var i = 0; i < amount; i++)
                {
                    var chosen = Random.Range(i, neighArray.Length);
                    (neighArray[i], neighArray[chosen]) = (neighArray[chosen], neighArray[i]);
                    chosenNeighs.Add(neighArray[i]);
                }

                foreach (var chosenNeigh in chosenNeighs)
                {
                    var addedNode = node.AddChild(chosenNeigh, new NodeData());
                    _tree.RegisterNode(addedNode);
                    onNodeAdded.Invoke(chosenNeigh);
                }
            }

            node.Data.Expanded = true;
        }

        public IEnumerable<TC> GetExpandablePositions()
        {
            return Root.Leaves().Where(n => !n.Data.Expanded).Select(n => n.Position);
        }
    }


    public interface INeighborGetter<TC>
    {
        public IEnumerable<TC> GetNeighbors(TC position);
    }

    public class Vector2IntNeighborGetter : INeighborGetter<Vector2Int>
    {
        public IEnumerable<Vector2Int> GetNeighbors(Vector2Int position)
        {
            return new[]
            {
                position + Vector2Int.up,
                position + Vector2Int.down,
                position + Vector2Int.left,
                position + Vector2Int.right,
            };
        }
    }

    public class ConstrainedGenerator
    {
        public IEnumerable<Vector2Int> CollapsableCells()
        {
            throw new NotImplementedException();
        }

        // public CollapseCell(Vector2Int cell)
        // {
        //     throw new NotImplementedException();
        // }
    }
}