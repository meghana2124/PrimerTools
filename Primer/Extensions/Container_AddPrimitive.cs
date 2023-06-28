using System;
using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public static class Container_AddPrimitiveExtensions
    {
        private static readonly Dictionary<PrimitiveType, GameObject> primitives = new();

        public static Transform AddPrimitive(this Container self, PrimitiveType type, string name = null, ChildOptions options = null)
        {
            var primitive = GetPrimitive(type);
            return self.Add(primitive.transform, name ?? Enum.GetName(typeof(PrimitiveType), type), options);
        }

        public static T AddPrimitive<T>(this Container self, PrimitiveType type, string name = null, ChildOptions options = null)
            where T : Component
        {
            var primitive = GetPrimitive(type);
            var transform = self.Add(primitive.transform, name ?? Enum.GetName(typeof(PrimitiveType), type), options);
            return transform.GetOrAddComponent<T>();
        }

        private static GameObject GetPrimitive(PrimitiveType type)
        {
            if (primitives.TryGetValue(type, out var primitive))
                return primitive;

            primitive = GameObject.CreatePrimitive(type);
            primitives.Add(type, primitive);
            return primitive;
        }
    }
}
