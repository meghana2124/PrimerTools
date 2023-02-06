using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

// ReSharper disable once CheckNamespace
// Namespace does not correspond to file location, should be: 'Primer.Timeline'
// We use FakeUnityEngine namespace because if "UnityEngine" is part of the namespace Unity allow us
//  to show this track without submenu
namespace Primer.Timeline.FakeUnityEngine
{
    [TrackClipType(typeof(GenericClip))]
    [TrackBindingType(typeof(Transform))]
    internal class GenericTrack : PrimerTrack
    {
        public float defaultDuration = 1;

        protected override Playable CreatePlayable(PlayableGraph graph, GameObject gameObject, TimelineClip clip)
        {
            if (clip.asset is GenericClip asset)
                asset.resolver ??= graph.GetResolver();

            return base.CreatePlayable(graph, gameObject, clip);
        }

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var trackTarget = graph.GetResolver() is PlayableDirector director
                ? director.GetGenericBinding(this) as Transform
                : null;

            foreach (var clip in GetClips()) {
                if (clip.asset is not GenericClip asset)
                    continue;

                asset.resolver ??= graph.GetResolver();

                if (asset.trackTarget is null && trackTarget != null)
                    asset.trackTarget = trackTarget;

                // HACK: to set the display name of the clip to match the clipName property
                if (string.IsNullOrWhiteSpace(clip.displayName)
                    || GenericBehaviour.IsGeneratedClipName(clip.displayName)) {
                    // the name has been autogenerated so we can safely replace it
                    clip.displayName = asset.template.clipName;
                }

                // HACK: pass the clip values to the GenericBehaviour
                asset.template.start = (float)clip.start;
                asset.template.duration = (float)clip.duration;
            }

            return ScriptPlayable<GenericMixer>.Create(graph, inputCount);
        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            base.OnCreateClip(clip);

            clip.duration = defaultDuration;

            if (clip.asset is GenericClip asset)
                asset.Initialize();

            // clip.postExtrapolationMode = TimelineClip.ClipExtrapolation.Hold;
            //
            // HACK: Property's setter above is internal (why? ask Unity), so we use reflection to set it.
            const BindingFlags PRIVATE_INSTANCE = BindingFlags.NonPublic | BindingFlags.Instance;
            clip.GetType()
                .GetMethod("set_postExtrapolationMode", PRIVATE_INSTANCE)
                ?.Invoke(clip, new object[] { TimelineClip.ClipExtrapolation.Hold });
        }
    }
}
