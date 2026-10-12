using System;
using DG.Tweening;
using UnityEngine;
using WheelOfFortune.Data;

namespace WheelOfFortune.Core
{
    public class WheelSpinController : MonoBehaviour, IWheelSpinController
    {
        [Header("Transforms")]
        [Tooltip("Child transform that rotates (keeping root transform clean)")]
        [SerializeField] private RectTransform wheelRotatorTransform;
        [Tooltip("Needle/indicator transform that tilts/ticks")]
        [SerializeField] private RectTransform indicatorTransform;

        [Header("Spin Settings")]
        [SerializeField] private float spinDuration = 4.0f;
        [SerializeField] private int minFullRotations = 5;
        [SerializeField] private Ease spinEase = Ease.OutCubic;

        [Header("Indicator Needle Settings")]
        [SerializeField] private float indicatorTickAngle = 18f;
        [SerializeField] private float indicatorTickDuration = 0.12f;

        private WheelConfigSO currentConfig;
        private int currentZoneMultiplier = 1;
        private bool isSpinning;
        private Tween currentSpinTween;
        private Tween currentIndicatorTween;
        private float lastSlicePassedAngle;
        private int lastSliceTickIndex = -1;

        public const float SliceAngle = 360f / WheelConfigSO.SliceCount; // 45 degrees

        public bool IsSpinning => isSpinning;
        public WheelConfigSO CurrentConfig => currentConfig;

        public event Action<int> OnSlicePassed;
        public event Action<int> OnSpinCompleted;

        public void Initialize(WheelConfigSO config, int zoneMultiplier)
        {
            currentConfig = config;
            currentZoneMultiplier = zoneMultiplier;
            isSpinning = false;

            if (wheelRotatorTransform != null)
            {
                wheelRotatorTransform.localEulerAngles = Vector3.zero;
            }
            if (indicatorTransform != null)
            {
                indicatorTransform.localEulerAngles = Vector3.zero;
            }
        }

        public void SpinToSlice(int targetIndex, Action<int> onCompleted = null)
        {
            if (isSpinning) return;
            if (wheelRotatorTransform == null)
            {
                Debug.LogError("[WheelSpinController] wheelRotatorTransform is not assigned!");
                return;
            }

            isSpinning = true;
            targetIndex = Mathf.Clamp(targetIndex, 0, WheelConfigSO.SliceCount - 1);

            // Kill any active tweens
            currentSpinTween?.Kill();
            currentIndicatorTween?.Kill();

            // Calculate start angle and target angle
            float currentAngle = wheelRotatorTransform.localEulerAngles.z;
            float currentAngleNorm = (currentAngle % 360f + 360f) % 360f;

            // Target slice angle: slice i was placed at i * SliceAngle clockwise from 12:00.
            // To bring slice i under the top indicator (12:00), the wheel rotator must rest at +targetSliceAngle.
            float targetSliceAngle = targetIndex * SliceAngle;
            
            // To spin clockwise (decreasing Z angle) from currentAngle to targetSliceAngle:
            float clockwiseDelta = (currentAngleNorm - targetSliceAngle) % 360f;
            if (clockwiseDelta <= 0f) clockwiseDelta += 360f;

            float totalRotationDegrees = (minFullRotations * 360f) + clockwiseDelta;
            float endAngleZ = currentAngle - totalRotationDegrees;

            lastSlicePassedAngle = currentAngle;
            lastSliceTickIndex = -1;

            float currentAnimAngle = currentAngle;

            currentSpinTween = DOTween.To(
                () => currentAnimAngle,
                x =>
                {
                    currentAnimAngle = x;
                    if (wheelRotatorTransform != null)
                    {
                        wheelRotatorTransform.localEulerAngles = new Vector3(0, 0, currentAnimAngle);
                    }
                    CheckIndicatorTick(currentAnimAngle);
                },
                endAngleZ,
                spinDuration
            )
            .SetEase(spinEase)
            .OnComplete(() =>
            {
                isSpinning = false;
                
                // Snap cleanly to normalized target angle
                float finalAngle = (targetSliceAngle % 360f + 360f) % 360f;
                if (wheelRotatorTransform != null)
                {
                    wheelRotatorTransform.localEulerAngles = new Vector3(0, 0, finalAngle);
                }

                if (indicatorTransform != null)
                {
                    indicatorTransform.DOLocalRotate(Vector3.zero, 0.1f);
                }

                onCompleted?.Invoke(targetIndex);
                OnSpinCompleted?.Invoke(targetIndex);
            });
        }

        private void CheckIndicatorTick(float currentZAngle)
        {
            // Indicator sits at top (0 deg). Needle ticks whenever a slice boundary crosses 0.
            // Slices are spaced by 45 degrees.
            float normalizedAngle = (currentZAngle % 360f + 360f) % 360f;
            int currentSliceIndex = Mathf.FloorToInt((normalizedAngle + (SliceAngle * 0.5f)) / SliceAngle) % WheelConfigSO.SliceCount;

            if (currentSliceIndex != lastSliceTickIndex)
            {
                lastSliceTickIndex = currentSliceIndex;
                OnSlicePassed?.Invoke(currentSliceIndex);
                PlayIndicatorTick();
            }
        }

        private void PlayIndicatorTick()
        {
            if (indicatorTransform == null) return;

            currentIndicatorTween?.Kill(true);

            // Punch needle rotation slightly in direction of spin, then spring back
            indicatorTransform.localEulerAngles = new Vector3(0, 0, indicatorTickAngle);
            currentIndicatorTween = indicatorTransform
                .DOLocalRotate(Vector3.zero, indicatorTickDuration)
                .SetEase(Ease.OutBack);
        }

        public void StopImmediate()
        {
            currentSpinTween?.Kill();
            currentIndicatorTween?.Kill();
            isSpinning = false;
            if (indicatorTransform != null)
            {
                indicatorTransform.localEulerAngles = Vector3.zero;
            }
        }

        private void OnDestroy()
        {
            currentSpinTween?.Kill();
            currentIndicatorTween?.Kill();
        }
    }
}
