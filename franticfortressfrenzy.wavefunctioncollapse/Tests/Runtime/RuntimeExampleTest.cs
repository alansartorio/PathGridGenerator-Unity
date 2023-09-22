using System;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FranticFortressFrenzy.WaveFunctionCollapse
{
    class Tests
    {
        [Test]
        public void SimpleTest()
        {
            var a = new GridPathGenerator<Vector2Int>(1, 1, new Vector2IntNeighborGetter(), Vector2Int.zero);
            a.Initialize();
            a.EnableNode(Vector2Int.zero);

            var expandAnyLeaf = new Action(() =>
            {
                var leaves = a.Root.Leaves().ToArray();
                a.EnableNode(leaves[Random.Range(0, leaves.Length)].Position);
            });

            for (int i = 0; i < 10; i++)
            {
                expandAnyLeaf();
            }

            var map = new char[2 * 10 + 1, 2 * 10 + 1];
            for (var y = 0; y < map.GetLength(0); y++)
            {
                for (var x = 0; x < map.GetLength(1); x++)
                {
                    map[y, x] = ' ';
                }
            }

            var nodePos = new Func<Vector2Int, Vector2Int>(n => (n + Vector2Int.one * 5) * 2);
            foreach (var node in a.Root)
            {
                var pos = nodePos(node.Position);
                if (node.Parent == null)
                {
                    map[pos.y, pos.x] = 'o';
                    continue;
                }

                map[pos.y, pos.x] = '#';
                var parentPos = nodePos(node.Parent.Position);
                var connection = (pos + parentPos) / 2;
                map[connection.y, connection.x] = '*';
            }

            StringBuilder mapString = new();
            for (var y = 0; y < map.GetLength(0); y++)
            {
                mapString.Append(':');
                for (var x = 0; x < map.GetLength(1); x++)
                {
                    mapString.Append(map[y, x]);
                }

                mapString.Append('\n');
            }

            Debug.Log(mapString);
        }
    }
}