using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WheelOfFortune.UI.Common
{
    /// <summary>
    /// Base class for UI views that automates reference finding in OnValidate
    /// and ensures runtime code-only button listeners (zero Inspector OnClick references).
    /// </summary>
    public abstract class UIAutoBinder : MonoBehaviour
    {
        protected virtual void Awake()
        {
            AutoBindReferences();
            RegisterButtonCallbacks();
        }

        protected virtual void OnDestroy()
        {
            UnregisterButtonCallbacks();
        }

        /// <summary>
        /// Derived classes subscribe their button click listeners here via code.
        /// </summary>
        protected abstract void RegisterButtonCallbacks();

        /// <summary>
        /// Derived classes remove their button click listeners here via code.
        /// </summary>
        protected abstract void UnregisterButtonCallbacks();

        public void ForceValidate()
        {
            AutoBindReferences();
            OptimizeRaycastAndMaskable();
        }

        protected virtual void OnValidate()
        {
            ForceValidate();
        }

        /// <summary>
        /// Automatically finds child buttons, texts, and images in editor time.
        /// </summary>
        protected virtual void AutoBindReferences() { }

        /// <summary>
        /// Optimizes raycastTarget and maskable properties across UI elements.
        /// Elements under a Mask or RectMask2D must have maskable = true to be clipped properly.
        /// Scroll areas, viewports, and handles must retain raycastTarget = true.
        /// </summary>
        protected virtual void OptimizeRaycastAndMaskable()
        {
            var images = GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                bool isUnderMask = img.GetComponentInParent<RectMask2D>() != null || img.GetComponentInParent<Mask>() != null;
                img.maskable = isUnderMask;

                bool isScrollElement = img.GetComponent<ScrollRect>() != null ||
                                       img.name.IndexOf("viewport", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                       img.name.IndexOf("scroll", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                       img.name.IndexOf("handle", StringComparison.OrdinalIgnoreCase) >= 0;

                bool isBackdrop = img.name.IndexOf("backdrop", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  img.name.IndexOf("modal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                  img.name.IndexOf("overlay", StringComparison.OrdinalIgnoreCase) >= 0;

                bool isInteractive = isScrollElement || isBackdrop || img.GetComponent<Selectable>() != null;
                img.raycastTarget = isInteractive;
            }

            var texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var txt in texts)
            {
                bool isUnderMask = txt.GetComponentInParent<RectMask2D>() != null || txt.GetComponentInParent<Mask>() != null;
                txt.maskable = isUnderMask;
                txt.raycastTarget = false;
            }
        }

        protected T FindChildComponent<T>(string childName) where T : Component
        {
            var transforms = GetComponentsInChildren<Transform>(true);
            foreach (var t in transforms)
            {
                if (t.name.Equals(childName, StringComparison.OrdinalIgnoreCase))
                {
                    return t.GetComponent<T>();
                }
            }
            return null;
        }

        protected Button FindButtonByName(string buttonName)
        {
            return FindChildComponent<Button>(buttonName);
        }

        protected TextMeshProUGUI FindTextByName(string textName)
        {
            return FindChildComponent<TextMeshProUGUI>(textName);
        }

        protected Image FindImageByName(string imageName)
        {
            return FindChildComponent<Image>(imageName);
        }
    }
}
