using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Primer.Latex
{
    internal class LatexTransitionState
    {
        private static void EnsureGroupsHaveSameLength<T, U>(IEnumerable<T> left, IEnumerable<U> right)
            => EnsureGroupsHaveSameLength(left.Count(), right.Count());

        private static void EnsureGroupsHaveSameLength(int left, int right)
        {
            const string ERROR_MESSAGE = "Can't transition from states with different amount of groups";
            Assert.AreEqual(left, right, ERROR_MESSAGE);
        }

        private readonly GroupState[] groups;
        private readonly TransformSnapshot snapshot;
        private readonly GameObject source;

        public Transform transform => source.transform;

        public LatexTransitionState(LatexRenderer renderer, IEnumerable<LatexExpression> groups)
        {
            source = renderer.gameObject;
            snapshot = source.GetOrAddComponent<TransformSnapshot>();
            this.groups = groups.Select((_, i) => new GroupState(renderer.transform, i)).ToArray();
        }

        public LatexTransitionState(LatexRenderer renderer, IEnumerable<TransitionType> transitions)
        {
            source = renderer.gameObject;
            snapshot = source.GetOrAddComponent<TransformSnapshot>();
            groups = transitions.Select((x, i) => new GroupState(renderer.transform, i, x)).ToArray();
        }

        public void Restore()
        {
            snapshot.Restore();
        }

        public IEnumerable<GroupState> GroupsToRemoveTransitioningTo(LatexTransitionState other)
        {
            EnsureGroupsHaveSameLength(groups, other.groups);

            for (var i = 0; i < groups.Length; i++) {
                var group = groups[i];
                var otherGroup = other.groups[i];

                if (group.isReplaced || otherGroup.isReplaced || group.isRemoved && !other.groups[i].isRemoved)
                    yield return group;
            }
        }

        public IEnumerable<(GroupState, GroupState)> GetCommonGroups(LatexTransitionState other)
        {
            EnsureGroupsHaveSameLength(groups, other.groups);

            for (var i = 0; i < groups.Length; i++) {
                if (groups[i].isLerped && other.groups[i].isLerped)
                    yield return (groups[i], other.groups[i]);
            }
        }
    }
}
