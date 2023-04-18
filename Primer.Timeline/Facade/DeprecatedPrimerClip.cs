using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Primer.Timeline
{
    /// Remember to use [Serializable] attribute when extending this
    [Obsolete("Use PrimerClip instead")]
    public abstract class DeprecatedPrimerClip<T> : PlayableAsset, ITimelineClipAsset
        where T : PrimerPlayable, new()
    {
        public virtual ClipCaps clipCaps => ClipCaps.None;

        [Space]
        [HideLabel]
        [InlineProperty]
        public T template = new();

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return template is null
                ? Playable.Null
                : ScriptPlayable<T>.Create(graph, template);
        }
    }
}
