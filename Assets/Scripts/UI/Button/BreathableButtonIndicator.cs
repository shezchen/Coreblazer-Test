using DG.Tweening;
using Sirenix.OdinInspector;
using Tools;
using UnityEngine;

namespace Architecture
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BreathableButtonIndicator : MonoBehaviour
    {
        [Title("Breath Settings")]
        [SerializeField]
        private bool canBreathe = true;
        
        [SerializeField, Range(0.1f, 5f), Tooltip("Duration for one breath cycle (max to min)")]
        private float duration = 1f;

        [SerializeField, Range(0f, 1f)]
        private float minAlpha = 0.4f;

        [SerializeField, Range(0f, 1f)]
        private float maxAlpha = 1f;

        [SerializeField]
        private Ease easeType = Ease.InOutSine;

        [SerializeField]
        private bool independentUpdate = true;

        private CanvasGroup _canvasGroup;
        private Tween _breathTween;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            if (canBreathe)
            {
                StartBreathing();
            }
            
        }

        private void OnDisable()
        {
            if (canBreathe)
            {
                StopBreathing();
            }
        }

        private void StartBreathing()
        {
            if (_canvasGroup == null) return;

            // Kill any existing tween to be safe
            StopBreathing();

            // Use the extension method from DOTweenTool
            _breathTween = _canvasGroup.Breath(
                minAlpha, 
                maxAlpha, 
                duration, 
                -1, // Infinite loops
                easeType, 
                independentUpdate
            );
        }

        private void StopBreathing()
        {
            if (_breathTween != null && _breathTween.IsActive())
            {
                _breathTween.Kill();
                _breathTween = null;
            }
            
            // Reset alpha to max when disabled/stopped
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = maxAlpha;
            }
        }

        [Button("Test Start"), DisableInEditorMode]
        private void TestStart() => StartBreathing();

        [Button("Test Stop"), DisableInEditorMode]
        private void TestStop() => StopBreathing();
    }
}