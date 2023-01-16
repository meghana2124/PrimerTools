using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace Primer.Axis
{
    [HideLabel]
    [Serializable]
    [InlineProperty]
    [DisableContextMenu]
    [HideReferenceObjectPicker]
    [Title("Label")]
    internal class AxisLabel
    {
        public const float X_OFFSET = 0.4f;

        private PrimerText2 labelObject;

        public string text = "Label";
        public Vector3 offset = Vector3.zero;
        public AxisLabelPosition position = AxisLabelPosition.End;

        public void Update(ChildrenDeclaration modifier, AxisDomain domain, float labelDistance)
        {
            modifier.Next(ref labelObject, "Label");

            var pos = position switch {
                AxisLabelPosition.Along => new Vector3(domain.length / 2, 0f, 0f),
                AxisLabelPosition.End => new Vector3(domain.rodEnd + X_OFFSET, 0f, 0f),
                _ => Vector3.zero,
            };

            labelObject.text = text;
            labelObject.alignment = TextAlignmentOptions.Midline;
            labelObject.transform.localPosition = pos + offset;
        }
    }
}
