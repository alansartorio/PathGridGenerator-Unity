using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace FranticFortressFrenzy.WaveFunctionCollapse
{
    public class PathNode<TC, T>
    {
        public TC Position { get; }
        public List<PathNode<TC, T>> Children { get; }
        [CanBeNull] public PathNode<TC, T> Parent { get; private set; }
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

        public IEnumerable<PathNode<TC, T>> Descendants()
        {
            return Enumerable
                .Repeat(this, 1)
                .Concat(Children.SelectMany(c => c.Descendants()));
        }

        public PathNode<TC, T> AddChild(TC position, T data)
        {
            var node = new PathNode<TC, T>(position, data, this, new List<PathNode<TC, T>>());
            Children.Add(node);
            return node;
        }

        public void RemoveChild(PathNode<TC, T> child)
        {
            Children.Remove(child);
            child.Parent = null;
        }

        public void SetParent(PathNode<TC, T> parent)
        {
            Parent = parent;
        }

        public IEnumerable<PathNode<TC, T>> Leaves()
        {
            return Descendants().Where(n => n.Children.Count == 0);
        }
    }
}