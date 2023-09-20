using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FranticFortressFrenzy.WaveFunctionCollapse
{
    public class GridPathGenerator<TC> where TC : IEquatable<TC>
    {
        public PathNode<TC> Root { get; }
        public HashSet<TC> occupied = new();
        private INeighborGetter<TC> _neighborGetter;
        private Dictionary<TC, PathNode<TC>> _nodes;

        public GridPathGenerator(INeighborGetter<TC> neighborGetter, TC rootPosition)
        {
            _neighborGetter = neighborGetter;
            Root = new PathNode<TC>(rootPosition, new List<PathNode<TC>>());
            _nodes = new Dictionary<TC, PathNode<TC>>();
            _nodes.Add(Root.Position, Root);
        }

        public void Step()
        {
        }

        [CanBeNull]
        public PathNode<TC> GetNodeInPosition(TC position)
        {
            return _nodes[position];
        }

        private void RegisterNode(PathNode<TC> node)
        {
            _nodes.Add(node.Position, node);
        }

        public void Expand(TC position)
        {
            var node = GetNodeInPosition(position);
            if (node == null)
                throw new ArgumentException("Position does not exist on path tree");

            var neighs = new HashSet<TC>(_neighborGetter.GetNeighbors(node.Position));

            if (node.Parent != null)
                neighs.Remove(node.Parent.Position);

            neighs.RemoveWhere(n => _nodes.ContainsKey(n));

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
                var addedNode = node.AddChild(chosenNeigh);
                RegisterNode(addedNode);
            }
        }
    }

    public class PathNode<TC> : IEnumerable<PathNode<TC>>
    {
        public TC Position { get; }
        public List<PathNode<TC>> Children { get; }
        [CanBeNull] public PathNode<TC> Parent { get; }

        public PathNode(TC position, PathNode<TC> parent, List<PathNode<TC>> children)
        {
            Position = position;
            Children = children;
            Parent = parent;
        }

        public PathNode(TC position, List<PathNode<TC>> children) : this(position, null, children)
        {
        }

        public IEnumerator<PathNode<TC>> GetEnumerator()
        {
            return Enumerable.Repeat(this, 1).Concat(Children.SelectMany(c => c)).GetEnumerator();
        }

        public PathNode<TC> AddChild(TC position)
        {
            var node = new PathNode<TC>(position, this, new List<PathNode<TC>>());
            Children.Add(node);
            return node;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<PathNode<TC>> Leaves()
        {
            return this.Where(n => n.Children.Count == 0);
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