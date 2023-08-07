using Primer.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    public partial class Axis
    {
        #region public float thickness;
        [SerializeField, HideInInspector]
        private float _thickness = 1;

        [Title("Rod")]
        [ShowInInspector]
        public float thickness {
            get => _thickness;
            set {
                _thickness = value;
                UpdateChildren();
            }
        }
        #endregion

        private Tween TransitionRod(Gnome gnome)
        {
            var rod = gnome.AddGnome("Rod");
            var position = new Vector3(rodStart, 0f, 0f);
            var scale = new Vector3(rodEnd - rodStart, thickness, thickness);

            DrawBar(rod);

            return Tween.Parallel(
                rod.localPosition == position ? null : rod.MoveTo(position),
                rod.localScale == scale ? null : rod.ScaleTo(scale)
            );
        }

        private static void DrawBar(Gnome rod)
        {
            var cylinder = rod.AddPrimitive(PrimitiveType.Cylinder);
            cylinder.localPosition = new Vector3(0.5f, 0, 0);
            cylinder.localRotation = Quaternion.Euler(0, 0, -90);
            cylinder.localScale = new Vector3(0.0375f, 0.5f, 0.0375f);
        }
    }
}
