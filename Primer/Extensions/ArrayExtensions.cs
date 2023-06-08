using System.Collections.Generic;
using System.Linq;

namespace Primer
{
    public static class ArrayExtensions
    {
        public static T[] Append<T>(this T[] self, params T[] other) {
            var result = new T[self.Length + other.Length];
            self.CopyTo(result, 0);
            other.CopyTo(result, self.Length);
            return result;
        }

        public static IEnumerable<(int index, T value)> WithIndex<T>(this IEnumerable<T> items)
        {
            return items.Select((value, i) => (i, value));
        }

        public static IEnumerable<(T, float)> SpaceEvenly<T>(this IEnumerable<T> items, float distanceBetweenChildren)
        {
            var children = items.ToArray();

            for (var i = 0; i < children.Length; i++) {
                var y = GetEvenlySpacedFloatGivenStep(i, children.Length, distanceBetweenChildren);
                yield return (children[i], y);
            }
        }

        private static float GetEvenlySpacedFloatGivenStep(int objectIndex, int totalObjects, float distanceBetweenObjects)
        {
            var totalSpace = distanceBetweenObjects * (totalObjects - 1);
            var min = -totalSpace / 2;
            return min + objectIndex * distanceBetweenObjects;
        }
    }
}
