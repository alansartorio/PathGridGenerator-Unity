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

        public PathNode<TC, NodeData> Initialize()
        {
            return Root;
        }

        public NodesDelta<TC> Expand(TC position)
        {
            var node = _tree.GetNodeInPosition(position);
            if (node == null)
                throw new ArgumentException("Position does not exist on path tree");

            if (node.Data.Expanded)
                return null;

            var neighs = new HashSet<TC>(_neighborGetter.GetNeighbors(node.Position));

            if (node.Parent != null)
                neighs.Remove(node.Parent.Position);

            neighs.RemoveWhere(n => _tree.GetNodeInPosition(n) != null);
            node.Data.Expanded = true;

            if (neighs.Count == 0)
            {
                return new NodesDelta<TC>(
                    Enumerable.Empty<NodeWithParent<TC>>(),
                    Enumerable.Empty<PathNode<TC, NodeData>>(),
                    Enumerable.Repeat(node, 1)
                );
            }

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
            }

            return new NodesDelta<TC>(
                chosenNeighs.Select(pos => new NodeWithParent<TC>(_tree.GetNodeInPosition(pos), node)),
                Enumerable.Empty<PathNode<TC, NodeData>>(),
                Enumerable.Repeat(node, 1)
            );
        }

        public IEnumerable<TC> GetExpandablePositions()
        {
            return Root.Leaves().Where(n => !n.Data.Expanded).Select(n => n.Position);
        }
    }

    public class NodeWithParent<TC>
    {
        public PathNode<TC, NodeData> node;
        public PathNode<TC, NodeData> parent;

        public NodeWithParent(PathNode<TC, NodeData> node, PathNode<TC, NodeData> parent)
        {
            this.node = node;
            this.parent = parent;
        }
    }

    public class NodesDelta<TC>
    {
        public IEnumerable<NodeWithParent<TC>> addedNodes;
        public IEnumerable<PathNode<TC, NodeData>> removedNodes;
        public IEnumerable<PathNode<TC, NodeData>> enabledNodes;

        public NodesDelta(IEnumerable<NodeWithParent<TC>> addedNodes, IEnumerable<PathNode<TC, NodeData>> removedNodes,
            IEnumerable<PathNode<TC, NodeData>> enabledNodes)
        {
            this.addedNodes = addedNodes;
            this.removedNodes = removedNodes;
            this.enabledNodes = enabledNodes;
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
}