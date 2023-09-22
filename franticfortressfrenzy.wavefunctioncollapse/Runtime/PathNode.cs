using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace FranticFortressFrenzy.WaveFunctionCollapse
{
    public class PathNode<TC, T> : IEnumerable<PathNode<TC, T>>
    {
        public TC Position { get; }
        public List<PathNode<TC, T>> Children { get; }
        [CanBeNull] public PathNode<TC, T> Parent { get; }
        public T Data { get; private set; }

        public PathNode(TC position, T data, PathNode<TC, T> parent, List<PathNode<TC, T>> children)
        {
            Position = position;
            Children = children;
            Parent = parent;
            Data = data;
        }

        public PathNode(TC position, T data, List<PathNode<TC, T>> children) : this(position, data, null, children)
        {
        }

        public IEnumerator<PathNode<TC, T>> GetEnumerator()
        {
            return Enumerable.Repeat(this, 1).Concat(Children.SelectMany(c => c)).GetEnumerator();
        }

        public PathNode<TC, T> AddChild(TC position, T data)
        {
            var node = new PathNode<TC, T>(position, data, this, new List<PathNode<TC, T>>());
            Children.Add(node);
            return node;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<PathNode<TC, T>> Leaves()
        {
            return this.Where(n => n.Children.Count == 0);
        }
    }
}