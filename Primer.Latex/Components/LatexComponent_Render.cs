using System;
using System.Collections.Generic;
using System.Linq;
using Primer.Animation;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.VectorGraphics;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.Rendering;

namespace Primer.Latex
{
    public partial class LatexComponent : IMeshController, IHierarchyManipulator
    {
        private const string CHARACTERS_CONTAINER_NAME = "Characters";

        private Transform charactersCache;
        private Transform characters => Meta.CachedChildFind(ref charactersCache, transform, CHARACTERS_CONTAINER_NAME);

        [Title("Rendering")]
        [ShowInInspector]
        public Color color {
            get => this.GetColor();
            set => this.SetColor(value);
        }

        [ShowInInspector]
        public Material material {
            get => this.GetMaterial();
            set => this.SetMaterial(value);
        }


        private void Reset() => PatchMaterial();

        private void PatchMaterial()
        {
            // A default preset will automatically get applied when we're reset.
            // If we unconditionally set material here, we'll blow away the value it set.
            var presets = Preset.GetDefaultPresetsForObject(this);

            if (material is null || presets.All(preset => preset.excludedProperties.Contains("material"))) {
                material = RendererExtensions.defaultMaterial;
            }
        }


        private LatexExpression CreateExpressionFromHierarchy()
        {
            return string.IsNullOrWhiteSpace(latex)
                ? null
                : LatexExpression.FromHierarchy(characters, config);
        }

        public IEnumerable<MeshRenderer> GetCharacters(int? startIndex = null, int? endIndex = null)
        {
            var chars = characters.GetChildren();
            var cropped = endIndex.HasValue ? chars.Take(endIndex.Value + 1) : chars;
            return cropped.Skip(startIndex ?? 0).Select(x => x.GetComponent<MeshRenderer>());
        }

        public void SetColors(Color newColor, int? startIndex = null, int? endIndex = null)
        {
            GetCharacters(startIndex, endIndex).SetColor(newColor);
        }

        public Tween TweenColors(Color newColor, int? startIndex = null, int? endIndex = null)
        {
            return GetCharacters(startIndex, endIndex).TweenColor(newColor);
        }

        public void SetCastShadows(bool castShadows)
        {
            var mode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;

            foreach (var child in GetCharacters()) {
                child.shadowCastingMode = mode;
            }
        }


        public void UpdateChildren()
        {
            if (activeDisplay is null)
                SetActiveDisplay(characters);

            var isExpressionInvalid = expression is null || expression.Any(x => x.mesh is null);

            if (isExpressionInvalid || transform == null)
                return;

            var gnome = new Gnome(characters);
            var currentMaterial = material;
            var currentColor = color;

            foreach (var (index, character) in expression.WithIndex()) {
                var charTransform = gnome.Add($"LatexChar {index}").SetDefaults();
                character.RenderTo(charTransform, currentMaterial, currentColor);
            }

            gnome.Purge();

            // Just make sure all shadows are off for now. Could make this an option in the future if needed.
            SetCastShadows(false);
        }

        public void RegenerateChildren()
        {
            transform.RemoveAllChildren();
            UpdateChildren();
        }

        MeshRenderer[] IMeshController.GetMeshRenderers()
        {
            return transform.GetComponentsInChildren<MeshRenderer>();
        }

        [SerializeField, HideInInspector]
        private LatexAlignment _alignment = LatexAlignment.Center;
        [ShowInInspector]
        public LatexAlignment alignment {
            get => _alignment;
            set {
                _alignment = value;
                UpdateAlignment();
            }
        }

        private void UpdateAlignment()
        {
            // For each character,
            // get the position of the vertex in the parent's space as 2D
            var allVertices2D = expression
                .SelectMany(x => x.mesh.vertices.
                Select(y => new Vector2(y.x + x.position.x, y.y + x.position.y))); 
            var bounds = VectorUtils.Bounds(allVertices2D);
            switch (_alignment)
            {
                case LatexAlignment.Left:
                    expression.ForEach(x => x.position -= bounds.xMin * Vector3.right);
                    break;
                case LatexAlignment.Right:
                    expression.ForEach(x => x.position -= bounds.xMax * Vector3.right);
                    break;
                case LatexAlignment.Center:
                    expression.ForEach(x => x.position -= (Vector3)bounds.center);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            UpdateChildren();
        }
    }
    
    public enum LatexAlignment
    {
        Left,
        Center,
        Right
    }
}
