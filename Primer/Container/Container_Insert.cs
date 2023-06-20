using System.Collections.Generic;
using UnityEngine;

namespace Primer
{
    public partial class Container
    {
        private readonly List<Transform> usedChildren = new();
        private readonly List<Transform> unusedChildren = new();

        public int childCount => usedChildren.Count;

        public void Insert<TChild>(TChild child, ChildOptions options = null)
            where TChild : Component
        {
            options ??= new ChildOptions();
            var t = child.transform;

            if (options.enable)
                t.SetActive(true);

            if (t.parent != transform)
                t.SetParent(transform, options.worldPositionStays);

            if (options.ignoreSiblingOrder) {
                usedChildren.Add(t);
            } else {
                var siblingIndex = (int?)options.siblingIndex ?? childCount;

                usedChildren.Insert(siblingIndex, t);

                if (t.GetSiblingIndex() != siblingIndex)
                    t.SetSiblingIndex(siblingIndex);
            }

            unusedChildren.Remove(t);

            // AQUI ESTA EL ERROR
            child.GetPrimer().parentContainer = this;
        }
    }
}
