using UnityEngine;

namespace Tools
{
    /// <summary>
    /// Extension helpers for quickly applying common RectTransform anchor presets in code.
    /// </summary>
    public static class RectTransformExtensions
    {
        /// <summary>
        /// Unity's Anchor Preset equivalents.
        /// </summary>
        public enum AnchorPreset
        {
            TopLeft,
            TopCenter,
            TopRight,
            MiddleLeft,
            MiddleCenter,
            MiddleRight,
            BottomLeft,
            BottomCenter,
            BottomRight,
            StretchHorizontalTop,
            StretchHorizontalMiddle,
            StretchHorizontalBottom,
            StretchVerticalLeft,
            StretchVerticalCenter,
            StretchVerticalRight,
            StretchAll
        }

        /// <summary>
        /// Sets anchorMin, anchorMax and pivot at once. Optional zeroing keeps offsets predictable.
        /// </summary>
        public static void SetAnchorsAndPivot(this RectTransform rt, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, bool resetAnchoredPosition = true, bool resetSizeDelta = false)
        {
            if (rt == null) return;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            if (resetAnchoredPosition) rt.anchoredPosition = Vector2.zero;
            if (resetSizeDelta) rt.sizeDelta = Vector2.zero;
        }

        /// <summary>
        /// Applies one of Unity's anchor presets via code.
        /// </summary>
        public static void SetAnchorPreset(this RectTransform rt, AnchorPreset preset,
            bool resetAnchoredPosition = true, bool resetSizeDelta = true)
        {
            if (rt == null) return;

            Vector2 anchorMin;
            Vector2 anchorMax;
            Vector2 pivot;

            switch (preset)
            {
                case AnchorPreset.TopLeft:
                    anchorMin = anchorMax = pivot = new Vector2(0f, 1f);
                    break;
                case AnchorPreset.TopCenter:
                    anchorMin = anchorMax = new Vector2(0.5f, 1f);
                    pivot = new Vector2(0.5f, 1f);
                    break;
                case AnchorPreset.TopRight:
                    anchorMin = anchorMax = pivot = new Vector2(1f, 1f);
                    break;
                case AnchorPreset.MiddleLeft:
                    anchorMin = anchorMax = pivot = new Vector2(0f, 0.5f);
                    break;
                case AnchorPreset.MiddleCenter:
                    anchorMin = anchorMax = pivot = new Vector2(0.5f, 0.5f);
                    break;
                case AnchorPreset.MiddleRight:
                    anchorMin = anchorMax = pivot = new Vector2(1f, 0.5f);
                    break;
                case AnchorPreset.BottomLeft:
                    anchorMin = anchorMax = pivot = new Vector2(0f, 0f);
                    break;
                case AnchorPreset.BottomCenter:
                    anchorMin = anchorMax = new Vector2(0.5f, 0f);
                    pivot = new Vector2(0.5f, 0f);
                    break;
                case AnchorPreset.BottomRight:
                    anchorMin = anchorMax = pivot = new Vector2(1f, 0f);
                    break;
                case AnchorPreset.StretchHorizontalTop:
                    anchorMin = new Vector2(0f, 1f);
                    anchorMax = new Vector2(1f, 1f);
                    pivot = new Vector2(0.5f, 1f);
                    break;
                case AnchorPreset.StretchHorizontalMiddle:
                    anchorMin = new Vector2(0f, 0.5f);
                    anchorMax = new Vector2(1f, 0.5f);
                    pivot = new Vector2(0.5f, 0.5f);
                    break;
                case AnchorPreset.StretchHorizontalBottom:
                    anchorMin = new Vector2(0f, 0f);
                    anchorMax = new Vector2(1f, 0f);
                    pivot = new Vector2(0.5f, 0f);
                    break;
                case AnchorPreset.StretchVerticalLeft:
                    anchorMin = new Vector2(0f, 0f);
                    anchorMax = new Vector2(0f, 1f);
                    pivot = new Vector2(0f, 0.5f);
                    break;
                case AnchorPreset.StretchVerticalCenter:
                    anchorMin = new Vector2(0.5f, 0f);
                    anchorMax = new Vector2(0.5f, 1f);
                    pivot = new Vector2(0.5f, 0.5f);
                    break;
                case AnchorPreset.StretchVerticalRight:
                    anchorMin = new Vector2(1f, 0f);
                    anchorMax = new Vector2(1f, 1f);
                    pivot = new Vector2(1f, 0.5f);
                    break;
                case AnchorPreset.StretchAll:
                    anchorMin = Vector2.zero;
                    anchorMax = Vector2.one;
                    pivot = new Vector2(0.5f, 0.5f);
                    break;
                default:
                    anchorMin = rt.anchorMin;
                    anchorMax = rt.anchorMax;
                    pivot = rt.pivot;
                    break;
            }

            rt.SetAnchorsAndPivot(anchorMin, anchorMax, pivot, resetAnchoredPosition, resetSizeDelta);
        }

        /// <summary>
        /// Quickly zero both anchoredPosition and sizeDelta. Handy after changing presets.
        /// </summary>
        public static void ResetOffsets(this RectTransform rt)
        {
            if (rt == null) return;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }
    }
}