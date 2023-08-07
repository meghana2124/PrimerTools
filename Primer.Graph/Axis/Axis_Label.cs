using Primer.Animation;
using Primer.Latex;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Primer.Graph
{
    public partial class Axis
    {
        public const float X_OFFSET = 0.4f;

        #region public bool showLabel;
        [SerializeField, HideInInspector]
        private bool _showLabel = true;

        [Title("Label")]
        [ShowInInspector]
        public bool showLabel {
            get => _showLabel;
            set {
                _showLabel = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public string label;
        [SerializeField, HideInInspector]
        private string _label = "Label";

        [ShowInInspector]
        [EnableIf(nameof(showLabel))]
        public string label {
            get => _label;
            set {
                _label = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public Vector3 labelOffset;
        [SerializeField, HideInInspector]
        private Vector3 _labelOffset = Vector3.zero;

        [ShowInInspector]
        [EnableIf(nameof(showLabel))]
        public Vector3 labelOffset {
            get => _labelOffset;
            set {
                _labelOffset = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public Quaternion labelRotation;
        [SerializeField, HideInInspector]
        private Quaternion _labelRotation = Quaternion.identity;

        [ShowInInspector]
        [EnableIf(nameof(showLabel))]
        public Quaternion labelRotation {
            get => _labelRotation;
            set {
                _labelRotation = value;
                UpdateChildren();
            }
        }
        #endregion

        #region public AxisLabelPosition labelPosition;
        [SerializeField, HideInInspector]
        private AxisLabelPosition _labelPosition = AxisLabelPosition.End;

        [ShowInInspector]
        [EnableIf(nameof(showLabel))]
        public AxisLabelPosition labelPosition {
            get => _labelPosition;
            set {
                _labelPosition = value;
                UpdateChildren();
            }
        }
        #endregion

        private Tween TransitionLabel(Gnome gnome)
        {
            var labelTransform = gnome.AddLatex(label, "Label").transform;

            var pos = labelOffset + (labelPosition switch {
                AxisLabelPosition.Along => new Vector3(length / 2, 0f, 0f),
                AxisLabelPosition.End => new Vector3(rodEnd + X_OFFSET, 0f, 0f),
                _ => Vector3.zero,
            });

            return Tween.Parallel(
                pos == labelTransform.localPosition ? null : labelTransform.MoveTo(pos),
                labelRotation == labelTransform.localRotation ? null : labelTransform.RotateTo(labelRotation)
            );
        }
    }
}
