using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace FranticFortressFrenzy.WaveFunctionCollapse
{
    public class Tree<TC, T>
    {
        public PathNode<TC, T> Root { get; private set; }
        private readonly Dictionary<TC, PathNode<TC, T>> _nodes;

        public Tree(TC rootPosition, T rootData)
        {
            Root = new PathNode<TC, T>(rootPosition, rootData, new List<PathNode<TC, T>>());
            _nodes = new Dictionary<TC, PathNode<TC, T>> { { Root.Position, Root } };
        }
        
        [CanBeNull]
        public PathNode<TC, T> GetNodeInPosition(TC position)
        {
            return _nodes.TryGetValue(position, out var value) ? value : null;
        }

        public void RegisterNode(PathNode<TC, T> node)
        {
            _nodes.Add(node.Position, node);
        }

        public IEnumerable<PathNode<TC, T>> RemoveNode(PathNode<TC, T> node, PathNode<TC, T> parent)
        {
            foreach (var child in node.Descendants())
            {
                _nodes.Remove(child.Position);
                yield return child;
            }
            
            parent.RemoveChild(node);
        }
    }
}