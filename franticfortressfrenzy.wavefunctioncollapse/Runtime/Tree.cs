using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting.Antlr3.Runtime.Tree;

namespace FranticFortressFrenzy.WaveFunctionCollapse
{
    public class Tree<TC, T>
    {
        public PathNode<TC, T> Root { get; private set; }
        private Dictionary<TC, PathNode<TC, T>> _nodes;

        public Tree(TC rootPosition, T rootData)
        {
            Root = new PathNode<TC, T>(rootPosition, rootData, new List<PathNode<TC, T>>());
            _nodes = new Dictionary<TC, PathNode<TC, T>>();
            _nodes.Add(Root.Position, Root);
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
    }
}