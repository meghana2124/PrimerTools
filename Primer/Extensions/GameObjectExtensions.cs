using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Primer
{
    public static class GameObjectExtensions
    {
        /// <summary>Will be true if game object is preset.</summary>
        /// <remarks>
        ///     This condition was found through exploration... There is no documented way to determine
        ///     whether we're currently editing a preset. There's likely to be other cases where this is true
        ///     that we'll want to figure out how to exclude. But we'll handle those as needed.
        /// </remarks>
        public static bool IsPreset(this GameObject gameObject)
        {
            return gameObject.scene.handle == 0 || gameObject.scene.path == "";
        }

        public static async void Dispose(this GameObject gameObject, bool urgent = false)
        {
            if (!gameObject)
                return;

            // TODO: invert this boolean, the boolean should be called `delayed` and only run this line if true
            if (!urgent)
                await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);

#if UNITY_EDITOR
            if (Application.isEditor && !Application.isPlaying)
                Object.DestroyImmediate(gameObject);
            else
#endif
                Object.Destroy(gameObject);
        }

        public static void DisposeAll(this IEnumerable<Transform> list)
        {
            var array = list.ToArray();

            for (var i = array.Length - 1; i >= 0; i--) {
                if (array[i] != null)
                    Dispose(array[i].gameObject);
            }
        }

        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            var found = gameObject.GetComponent<T>();
            return found == null ? gameObject.AddComponent<T>() : found;
        }

        public static PrimerBehaviour GetPrimer(this GameObject gameObject)
        {
            return GetOrAddComponent<PrimerBehaviour>(gameObject);
        }
    }
}
