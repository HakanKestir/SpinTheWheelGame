using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Core;
using WheelOfFortune.UI.Common;
using WheelOfFortune.Utils;

namespace WheelOfFortune.UI.Rewards
{
    public class UIRewardTraySlotItemView : UIAutoBinder
    {
        private const float MIN_COUNT_DURATION = 0.30f;
        private const float MAX_COUNT_DURATION = 1.20f;
        private const float COUNT_DURATION_PER_DECADE = 0.18f;

        [Header("Slot UI Elements (Names ending with _value)")]
        [SerializeField] private Image ui_image_reward_icon;
        [SerializeField] private TextMeshProUGUI ui_text_reward_amount_value;
        [SerializeField] private RectTransform ui_transform_punch_target;

        private string currentItemId;
        private long displayedAmount;
        private Coroutine countUpRoutine;

        public string CurrentItemId => currentItemId;

        protected override void AutoBindReferences()
        {
            if (ui_image_reward_icon == null)
            {
                ui_image_reward_icon = FindImageByName("ui_image_reward_icon");
            }
            if (ui_text_reward_amount_value == null)
            {
                ui_text_reward_amount_value = FindTextByName("ui_text_reward_amount_value");
            }
            if (ui_transform_punch_target == null)
            {
                var t = FindChildComponent<RectTransform>("ui_transform_punch_target");
                if (t != null) ui_transform_punch_target = t;
            }
        }

        protected override void RegisterButtonCallbacks() { }
        protected override void UnregisterButtonCallbacks() { }

        public void SetReward(CollectedRewardEntry entry, bool playAnimation = false)
        {
            if (entry == null || entry.Item == null) return;

            bool itemChanged = currentItemId != entry.Item.Id;
            currentItemId = entry.Item.Id;

            if (ui_image_reward_icon != null)
            {
                ui_image_reward_icon.sprite = entry.Item.Icon;
                ui_image_reward_icon.preserveAspect = true;
            }

            long targetAmount = entry.TotalAmount;
            bool canCount = playAnimation
                && targetAmount > displayedAmount
                && (!itemChanged || displayedAmount == 0);

            if (canCount)
            {
                StartCountUp(targetAmount);
            }
            else
            {
                SetAmountImmediate(targetAmount);
            }

            if (playAnimation && ui_transform_punch_target != null)
            {
                ui_transform_punch_target.DOKill(true);
                ui_transform_punch_target.localScale = Vector3.one;
                ui_transform_punch_target.DOPunchScale(Vector3.one * 0.25f, 0.35f, 5, 0.5f);
            }
        }

        private void StartCountUp(long targetAmount)
        {
            StopCountUp();
            countUpRoutine = StartCoroutine(CountUpRoutine(displayedAmount, targetAmount));
        }

        private IEnumerator CountUpRoutine(long fromAmount, long targetAmount)
        {
            float duration = GetCountDuration(fromAmount, targetAmount);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                // Ease-out cubic: fast start, gentle landing.
                float eased = 1f - Mathf.Pow(1f - progress, 3f);
                long value = (long)System.Math.Round(fromAmount + ((double)targetAmount - fromAmount) * eased);
                SetAmountText(value);
                yield return null;
            }

            SetAmountText(targetAmount);
            countUpRoutine = null;
        }

        /// <summary>
        /// Small gains count in a fraction of a second, while huge gains stay
        /// readable instead of ticking for seconds: 10 -> ~0.5s, 1K -> ~0.85s,
        /// 1M+ -> capped at 1.2s.
        /// </summary>
        private static float GetCountDuration(long fromAmount, long targetAmount)
        {
            double delta = System.Math.Abs((double)targetAmount - fromAmount);
            if (delta < 1d) return 0f;

            float duration = MIN_COUNT_DURATION
                + Mathf.Log10(1f + (float)delta) * COUNT_DURATION_PER_DECADE;
            return Mathf.Clamp(duration, MIN_COUNT_DURATION, MAX_COUNT_DURATION);
        }

        private void SetAmountImmediate(long amount)
        {
            StopCountUp();
            SetAmountText(amount);
        }

        private void SetAmountText(long amount)
        {
            displayedAmount = amount;
            if (ui_text_reward_amount_value != null)
            {
                ui_text_reward_amount_value.text = NumberFormatter.ToShortString(amount);
            }
        }

        private void StopCountUp()
        {
            if (countUpRoutine != null)
            {
                StopCoroutine(countUpRoutine);
                countUpRoutine = null;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            StopCountUp();
        }
    }
}
