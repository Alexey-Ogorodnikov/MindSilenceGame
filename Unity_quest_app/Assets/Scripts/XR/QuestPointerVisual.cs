using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit.Interactors.Visuals;

namespace MindSilence.XR
{
    /// <summary>
    /// Quest OS pointer look from Meta ray-casting specs: white hover beam,
    /// dark-blue select, overlay width 1 mm. Fixes URP magenta LineRenderer.
    /// </summary>
    public static class QuestPointerVisual
    {
        public static readonly Color Hover = Color.white;
        public static readonly Color Selecting = new Color32(0x00, 0x1E, 0x78, 0xFF);
        public const float OverlayWidthMeters = 0.001f;

        static Material _lineMaterial;
        static Gradient _hoverGradient;
        static Gradient _selectingGradient;

        public static Gradient HoverGradient => _hoverGradient ??= CreateSolid(Hover);

        public static Gradient SelectingGradient => _selectingGradient ??= CreateSolid(Selecting);

        public static Gradient CreateSolid(Color color)
        {
            return new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color, 1f),
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(color.a, 0f),
                    new GradientAlphaKey(color.a, 1f),
                },
            };
        }

        public static void Apply(LineRenderer line, XRInteractorLineVisual visual)
        {
            if (line != null)
            {
                line.sharedMaterial = LineMaterial();
                line.shadowCastingMode = ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.textureMode = LineTextureMode.Stretch;
                line.startWidth = OverlayWidthMeters;
                line.endWidth = OverlayWidthMeters;
                line.startColor = Hover;
                line.endColor = Hover;
            }

            if (visual == null)
            {
                return;
            }

            visual.lineWidth = OverlayWidthMeters;
            visual.setLineColorGradient = true;
            visual.validColorGradient = HoverGradient;
            visual.invalidColorGradient = HoverGradient;
            visual.blockedColorGradient = HoverGradient;
        }

        public static void SetSelecting(XRInteractorLineVisual visual, bool selecting)
        {
            if (visual == null)
            {
                return;
            }

            var gradient = selecting ? SelectingGradient : HoverGradient;
            visual.validColorGradient = gradient;
            visual.invalidColorGradient = gradient;
            visual.blockedColorGradient = gradient;
        }

        static Material LineMaterial()
        {
            if (_lineMaterial != null)
            {
                return _lineMaterial;
            }

            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                ?? Shader.Find("Universal Render Pipeline/Unlit")
                ?? Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            _lineMaterial = new Material(shader)
            {
                name = "QuestPointerLine",
                color = Hover,
                hideFlags = HideFlags.HideAndDontSave,
            };
            if (_lineMaterial.HasProperty("_BaseColor"))
            {
                _lineMaterial.SetColor("_BaseColor", Hover);
            }

            if (_lineMaterial.HasProperty("_Color"))
            {
                _lineMaterial.SetColor("_Color", Hover);
            }

            return _lineMaterial;
        }
    }
}
