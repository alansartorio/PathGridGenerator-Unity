using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FranticFortressFrenzy.WaveFunctionCollapse;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public static class EnumerableExtensions
{
    public static IEnumerable<T> GetFirst<T>(this IEnumerable<T> enumerable, out T first)
    {
        // var enumerator = enumerable.GetEnumerator();
        first = enumerable.FirstOrDefault();
        // return first == null ? Enumerable.Empty<T>() : Enumerable.Repeat(first, 1).Concat(new Enumerable(enumerator));
        return first == null ? Enumerable.Empty<T>() : enumerable;
    }
}

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

        public NodesDelta<TC> Initialize()
        {
            return new NodesDeltaBuilder<TC>()
                .WithAddedNodes(
                    Enumerable.Repeat(new NodeWithParent<TC>(Root, null), 1)
                )
                .Build().ApplyDelta(RegenerateDisabledTree());
            // EnableNode(Root.Position);
        }

        private IEnumerable<NodeWithParent<TC>> ExpandLeavesOnce(PathNode<TC, NodeData> node, out bool anyClosed)
        {
            anyClosed = false;
            var created = Enumerable.Empty<NodeWithParent<TC>>();
            var leaves = node.Leaves().ToList();
            foreach (var leaf in leaves)
            {
                created = created.Concat(ExpandUnchecked(node).GetFirst(out var first));
                if (first == null)
                {
                    anyClosed = true;
                }
            }

            return created;
        }

        private NodesDelta<TC> RegenerateDisabledTree()
        {
            return new NodesDeltaBuilder<TC>().Build();
        }

        public NodesDelta<TC> EnableNode(TC position)
        {
            var node = _tree.GetNodeInPosition(position);
            if (node == null)
                throw new ArgumentException("Position does not exist on path tree");

            if (!GetEnableablePositions().Contains(node.Position))
                return null;

            node.Data.Enabled = true;

            var created = ExpandLeavesOnce(node, out var anyClosed);

            var delta = new NodesDeltaBuilder<TC>()
                .WithAddedNodes(created)
                .WithEnabledNodes(Enumerable.Repeat(node, 1))
                .Build();
            if (anyClosed)
            {
                delta = delta.ApplyDelta(RegenerateDisabledTree());
            }

            return delta;
        }

        private IEnumerable<NodeWithParent<TC>> ExpandUnchecked(PathNode<TC, NodeData> node)
        {
            var neighs = new HashSet<TC>(_neighborGetter.GetNeighbors(node.Position));

            if (node.Parent != null)
                neighs.Remove(node.Parent.Position);

            neighs.RemoveWhere(n => _tree.GetNodeInPosition(n) != null);
            node.Data.Expanded = true;

            if (neighs.Count == 0)
            {
                return Enumerable.Empty<NodeWithParent<TC>>();
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

            return chosenNeighs.Select(pos => new NodeWithParent<TC>(_tree.GetNodeInPosition(pos), node));
        }

        public IEnumerable<TC> GetEnableablePositions()
        {
            return Root.Leaves()
                .Where(n => !n.Data.Enabled && (n.Parent?.Data.Enabled ?? true))
                .Select(n => n.Position);
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

        public NodesDelta<TC> ApplyDelta(NodesDelta<TC> other)
        {
            var removed = other.removedNodes.ToHashSet();
            return new NodesDelta<TC>(
                addedNodes.Where(n => !removed.Contains(n.node)).Concat(other.addedNodes),
                removedNodes.Concat(removed),
                enabledNodes.Where(n => !removed.Contains(n)).Concat(other.enabledNodes)
            );
        }
    }

    public class NodesDeltaBuilder<TC>
    {
        private IEnumerable<NodeWithParent<TC>> _addedNodes = Enumerable.Empty<NodeWithParent<TC>>();
        private IEnumerable<PathNode<TC, NodeData>> _removedNodes = Enumerable.Empty<PathNode<TC, NodeData>>();
        private IEnumerable<PathNode<TC, NodeData>> _enabledNodes = Enumerable.Empty<PathNode<TC, NodeData>>();

        public NodesDeltaBuilder<TC> WithAddedNodes(IEnumerable<NodeWithParent<TC>> addedNodes)
        {
            _addedNodes = addedNodes;
            return this;
        }

        public NodesDeltaBuilder<TC> WithRemovedNodes(IEnumerable<PathNode<TC, NodeData>> removedNodes)
        {
            _removedNodes = removedNodes;
            return this;
        }

        public NodesDeltaBuilder<TC> WithEnabledNodes(IEnumerable<PathNode<TC, NodeData>> enabledNodes)
        {
            _enabledNodes = enabledNodes;
            return this;
        }

        public NodesDelta<TC> Build()
        {
            return new NodesDelta<TC>(
                _addedNodes, _removedNodes, _enabledNodes
            );
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