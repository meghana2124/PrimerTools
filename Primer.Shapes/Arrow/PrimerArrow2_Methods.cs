using System.Threading.Tasks;
using Primer.Animation;
using UnityEngine;

namespace Primer.Shapes
{
    public partial class PrimerArrow2
    {
        #region Setters
        public PrimerArrow2 Follow(GameObject from, GameObject to) => Follow(from.transform, to.transform);
        public PrimerArrow2 Follow(Component from, Component to) => Follow(from.transform, to.transform);

        public PrimerArrow2 Follow(Transform from, Transform to)
        {
            tailPoint.follow = from;
            headPoint.follow = to;
            return this;
        }

        public void StopFollowing()
        {
            tailPoint.StopTracking();
            headPoint.StopTracking();
        }

        public void SetFromTo(Vector3 from, Vector3 to, bool global)
        {
            globalPositioning = global;
            SetFromTo(from, to);
        }

        public void SetFromTo(Vector3 from, Vector3 to)
        {
            tailPoint.vector = from;
            headPoint.vector = to;
        }

        private void SetLength(float value)
        {
            // If the length is too small, just prevent the change
            if (value < (tailPointer ? realArrowLength : 0) + (headPointer ? realArrowLength : 0))
                return;

            var diff = head - tail;
            headPoint.vector += (value - diff.magnitude) * Vector3.Normalize(diff);
        }
        #endregion


        #region Animations
        public Tween GrowFromStart()
        {
            var requiresAdjustmentTweening = tailPoint.adjustment != headPoint.adjustment;
            var originalHeadPosition = (Vector3Provider)headPoint;
            var originalHeadAdjustment = headPoint.adjustment;

            tailPoint.CopyTo(headPoint);

            var growTween = Animate(headEnd: originalHeadPosition).Observe(
                beforeStart: () => gameObject.SetActive(true),
                afterComplete: () => originalHeadPosition.ApplyTo(headPoint)
            );

            var extensionTween = requiresAdjustmentTweening
                ? Tween.Parallel(growTween, Tween.Value(
                    v => headPoint.adjustment = v, 
                    () => tailPoint.adjustment, 
                    () => originalHeadAdjustment)
                )
                : growTween;

            return Tween.Parallel(
                extensionTween,
                headObject.GetChild(0).MoveTo(Vector3.right * 0.05f, Vector3.zero),
                headObject.GetChild(0).ScaleTo(50, 0)
            );
        }

        public Tween ShrinkToEnd(bool restoreTracking = false)
        {
            var shrinkTween = Animate(tailEnd: headPoint, preventRestoreTracking: !restoreTracking).Observe(
                beforeStart: StopFollowing,
                afterComplete: () => this.SetActive(false)
            );

            if (tailPoint.adjustment == headPoint.adjustment)
                return shrinkTween;

            var originalTailAdjustment = tailPoint.adjustment;

            return Tween.Parallel(
                shrinkTween,
                Tween.Value(
                        v => tailPoint.adjustment = v,
                        () => tailPoint.adjustment,
                        () => headPoint.adjustment
                        )
                    .Observe(afterComplete: () => tailPoint.adjustment = originalTailAdjustment)
            );
        }

        public Tween Animate(
            Vector3Provider headEnd = null,
            Vector3Provider tailEnd = null,
            Vector3Provider headStart = null,
            Vector3Provider tailStart = null,
            bool preventRestoreTracking = false)
        {
            var tailTracking = tailPoint.isTracking ? (Vector3Provider)tailPoint : null;
            var headTracking = headPoint.isTracking ? (Vector3Provider)headPoint : null;
            var tailTween = tailPoint.Tween(tailEnd, tailStart);
            var headTween = headPoint.Tween(headEnd, headStart);

            var tween = new Tween(t => {
                tailPoint.vector = tailTween(t);
                headPoint.vector = headTween(t);
                Recalculate();
            });

            if (preventRestoreTracking || (tailTracking is null && headTracking is null))
                return tween;

            return tween.Observe(
                afterComplete: () => {
                    tailTracking?.ApplyTo(tailPoint);
                    headTracking?.ApplyTo(headPoint);
                }
            );
        }
        #endregion
    }
}
