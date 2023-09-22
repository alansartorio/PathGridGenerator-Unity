using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FranticFortressFrenzy.WaveFunctionCollapse;
using UnityEngine;
using UnityEngine.UIElements;
using Random = System.Random;

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
        private Random rng = new Random(135135);

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
            Root.Data.Enabled = true;
            var rootChildren = ExpandUnchecked(Root).ToHashSet();
            rootChildren.Add(new NodeWithParent<TC>(Root, null));
            return new NodesDeltaBuilder<TC>()
                .WithAddedNodes(rootChildren)
                .WithEnabledNodes(new() { Root })
                .Build().ApplyDelta(RegenerateDisabledTree());
        }

        private IEnumerable<NodeWithParent<TC>> ExpandLeavesOnce(PathNode<TC, NodeData> node, out bool anyClosed)
        {
            anyClosed = false;
            var created = new HashSet<NodeWithParent<TC>>();
            var leaves = node.Leaves().Where(l => !l.Data.Expanded).ToArray();
            foreach (var leaf in leaves)
            {
                created.UnionWith(ExpandUnchecked(leaf).GetFirst(out var first));
                if (first == null)
                {
                    anyClosed = true;
                }
            }

            return created;
        }

        private HashSet<PathNode<TC, NodeData>> CutDisabledTree()
        {
            var removed = new HashSet<PathNode<TC, NodeData>>();

            foreach (var enableableNode in GetEnableablePositions().Select(p => _tree.GetNodeInPosition(p)!).ToArray())
            {
                enableableNode.Data.Expanded = false;
                foreach (var treeRoot in enableableNode.Children.ToArray())
                {
                    removed.UnionWith(_tree.RemoveNode(treeRoot, enableableNode));
                }
            }

            return removed;
        }

        private NodesDelta<TC> RegenerateDisabledTree()
        {
            var removed = CutDisabledTree();

            var added = new HashSet<NodeWithParent<TC>>();
            bool lastTry, anyClosed;
            int tryNumber = 0;
            do
            {
                lastTry = tryNumber == MaxRetries - 1;
                CutDisabledTree();

                added.Clear();
                anyClosed = false;

                for (int depth = 0; depth < TargetLookAhead; depth++)
                {
                    var addedAux = ExpandLeavesOnce(_tree.Root, out var anyClosedAux);
                    added.UnionWith(addedAux);
                    if (anyClosedAux && !lastTry)
                    {
                        anyClosed = true;
                        break;
                    }
                }

                tryNumber++;
            } while (anyClosed && !lastTry);

            return new NodesDeltaBuilder<TC>()
                .WithRemovedNodes(removed)
                .WithAddedNodes(added)
                .Build();
        }

        public NodesDelta<TC> EnableNode(TC position)
        {
            var node = _tree.GetNodeInPosition(position);
            if (node == null)
                throw new ArgumentException("Position does not exist on path tree");

            if (!GetEnableablePositions().Contains(node.Position))
                return null;


            var created = ExpandLeavesOnce(node, out var anyClosed).ToHashSet();

            var delta = new NodesDeltaBuilder<TC>()
                .WithAddedNodes(created)
                .WithEnabledNodes(new() { node })
                .Build();
            if (anyClosed)
            {
                delta = delta.ApplyDelta(RegenerateDisabledTree());
            }

            node.Data.Enabled = true;

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

            // This makes choosing 1 neighbor twice as likely than choosing 2 neighbors, same for 2 and 3
            var amount = neighs.Count - (int)Math.Log(rng.Next(1, 1 << neighs.Count), 2);
            var neighArray = neighs.ToArray();

            var chosenNeighs = new List<TC>();
            for (var i = 0; i < amount; i++)
            {
                var chosen = rng.Next(i, neighArray.Length);
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
            return Root.Descendants()
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
        public HashSet<NodeWithParent<TC>> addedNodes;
        public HashSet<PathNode<TC, NodeData>> removedNodes;
        public HashSet<PathNode<TC, NodeData>> enabledNodes;

        public NodesDelta(HashSet<NodeWithParent<TC>> addedNodes, HashSet<PathNode<TC, NodeData>> removedNodes,
            HashSet<PathNode<TC, NodeData>> enabledNodes)
        {
            this.addedNodes = addedNodes;
            this.removedNodes = removedNodes;
            this.enabledNodes = enabledNodes;
        }

        public NodesDelta<TC> ApplyDelta(NodesDelta<TC> other)
        {
            var added = addedNodes.ToHashSet();
            var addedSet = added.Select(a => a.node).ToHashSet();
            var removed = other.removedNodes.ToHashSet();

            added.RemoveWhere(a => removed.Contains(a.node));
            removed.RemoveWhere(r => addedSet.Contains(r));
            removed.UnionWith(removedNodes);
            added.UnionWith(other.addedNodes);
            var enabled = enabledNodes.Except(removed).Union(other.enabledNodes).ToHashSet();

            return new NodesDelta<TC>(
                added,
                removed,
                enabled
            );
        }
    }

    public class NodesDeltaBuilder<TC>
    {
        private HashSet<NodeWithParent<TC>> _addedNodes = new HashSet<NodeWithParent<TC>>();
        private HashSet<PathNode<TC, NodeData>> _removedNodes = new HashSet<PathNode<TC, NodeData>>();
        private HashSet<PathNode<TC, NodeData>> _enabledNodes = new HashSet<PathNode<TC, NodeData>>();

        public NodesDeltaBuilder<TC> WithAddedNodes(HashSet<NodeWithParent<TC>> addedNodes)
        {
            _addedNodes = addedNodes;
            return this;
        }

        public NodesDeltaBuilder<TC> WithRemovedNodes(HashSet<PathNode<TC, NodeData>> removedNodes)
        {
            _removedNodes = removedNodes;
            return this;
        }

        public NodesDeltaBuilder<TC> WithEnabledNodes(HashSet<PathNode<TC, NodeData>> enabledNodes)
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