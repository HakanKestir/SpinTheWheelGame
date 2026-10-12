using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace WheelOfFortune.UI.Wheel
{
    [RequireComponent(typeof(Button))]
    public class UISpinButtonAnimator : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler
    {
        [Header("Hover Settings")]
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float hoverDuration = 0.18f;

        [Header("Click / Press Settings")]
        [SerializeField] private float pressScale = 0.92f;
        [SerializeField] private float pressDuration = 0.08f;
        [SerializeField] private float punchScaleAmount = 0.10f;
        [SerializeField] private float punchDuration = 0.22f;

        [Header("Idle Breathing Pulse")]
        [SerializeField] private bool enableIdlePulse = true;
        [SerializeField] private float pulseScale = 1.035f;
        [SerializeField] private float pulseDuration = 0.85f;

        [Header("Animation Target (Separated Transform)")]
        [Tooltip("Child transform that receives the animation scaling, keeping the root button transform static")]
        [SerializeField] private RectTransform animTargetTransform;

        private Button button;
        private Tween currentTween;
        private Tween idlePulseTween;
        private bool isPointerInside;
        private bool isPointerDown;

        public RectTransform AnimTargetTransform => animTargetTransform;

        private void Awake()
        {
            button = GetComponent<Button>();
            EnsureAnimTarget();
        }

        private void OnValidate()
        {
            EnsureAnimTarget();
        }

        private void EnsureAnimTarget()
        {
            if (animTargetTransform == null)
            {
                // Prefer designated child content transform if available
                var content = transform.Find("ui_transform_button_content") as RectTransform;
                if (content != null)
                {
                    animTargetTransform = content;
                }
                else if (transform.childCount > 0)
                {
                    animTargetTransform = transform.GetChild(0) as RectTransform;
                }

                if (animTargetTransform == null)
                {
                    animTargetTransform = GetComponent<RectTransform>();
                }
            }
        }

        private void OnEnable()
        {
            if (animTargetTransform != null) animTargetTransform.localScale = Vector3.one;
            if (button != null && button.interactable)
            {
                StartIdlePulse();
            }
        }

        private void OnDisable()
        {
            KillAllTweens();
            if (animTargetTransform != null) animTargetTransform.localScale = Vector3.one;
            isPointerInside = false;
            isPointerDown = false;
        }

        private void OnDestroy()
        {
            KillAllTweens();
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null) button.interactable = interactable;
            if (interactable)
            {
                StartIdlePulse();
            }
            else
            {
                StopIdlePulse();
                currentTween?.Kill();
                if (animTargetTransform != null)
                {
                    currentTween = animTargetTransform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
                }
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button == null || !button.interactable) return;
            isPointerInside = true;
            StopIdlePulse();

            currentTween?.Kill();
            if (animTargetTransform != null)
            {
                currentTween = animTargetTransform.DOScale(hoverScale, hoverDuration).SetEase(Ease.OutBack);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isPointerInside = false;
            isPointerDown = false;
            if (button == null || !button.interactable) return;

            currentTween?.Kill();
            if (animTargetTransform != null)
            {
                currentTween = animTargetTransform.DOScale(1f, 0.15f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        if (!isPointerInside && button != null && button.interactable)
                        {
                            StartIdlePulse();
                        }
                    });
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (button == null || !button.interactable) return;
            isPointerDown = true;
            StopIdlePulse();

            currentTween?.Kill();
            if (animTargetTransform != null)
            {
                currentTween = animTargetTransform.DOScale(pressScale, pressDuration).SetEase(Ease.OutQuad);
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (button == null || !button.interactable) return;
            isPointerDown = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (button == null || !button.interactable) return;
            if (animTargetTransform == null) return;

            currentTween?.Kill();
            float baseScale = isPointerInside ? hoverScale : 1f;
            animTargetTransform.localScale = Vector3.one * baseScale;
            currentTween = animTargetTransform.DOPunchScale(Vector3.one * punchScaleAmount, punchDuration, 8, 0.5f)
                .OnComplete(() =>
                {
                    if (!isPointerInside && button != null && button.interactable)
                    {
                        StartIdlePulse();
                    }
                });
        }

        public void StartIdlePulse()
        {
            if (!enableIdlePulse || animTargetTransform == null) return;
            if (button != null && !button.interactable) return;
            if (isPointerInside || isPointerDown) return;

            idlePulseTween?.Kill();
            animTargetTransform.localScale = Vector3.one;
            idlePulseTween = animTargetTransform.DOScale(pulseScale, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }

        public void StopIdlePulse()
        {
            idlePulseTween?.Kill();
        }

        private void KillAllTweens()
        {
            currentTween?.Kill();
            idlePulseTween?.Kill();
        }
    }
}
