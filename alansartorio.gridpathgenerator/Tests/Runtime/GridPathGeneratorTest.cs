using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace AlanSartorio.GridPathGenerator
{
    class GridPathGeneratorTest
    {
        [Test]
        public void SimpleTest()
        {
            var test = new[]
            {
                1, 2, 3, 4, 5
            };

            var enumerable = test.GetFirst(out var first);

            Assert.That(first, Is.EqualTo(1));

            Assert.That(enumerable.ToArray(), Is.EqualTo(test));
        }
    }
}
