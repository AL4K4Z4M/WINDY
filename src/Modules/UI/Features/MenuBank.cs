using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using TMPro;

namespace WindyFramework.Modules.UI.Features
{
    public class MenuBank : MonoBehaviour
    {
        private GameObject _container;
        private HorizontalLayoutGroup _layout;

        public void Initialize(Transform parent, Vector3 position)
        {
            try
            {
                // Create Container
                _container = new GameObject("MOD_MenuBank");
                _container.transform.SetParent(parent, false);

                // Rect
                RectTransform rect = _container.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition3D = position;
                
                // Layout
                _layout = _container.AddComponent<HorizontalLayoutGroup>();
                _layout.childControlWidth = true; // Force layout control
                _layout.childControlHeight = true; // Force layout control
                _layout.childAlignment = TextAnchor.MiddleCenter;
                _layout.spacing = 20f;
                _layout.childForceExpandWidth = false;
                _layout.childForceExpandHeight = false;

                // Size
                ContentSizeFitter fitter = _container.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[MenuBank] Init Failed: {ex}");
            }
        }

        public GameObject AddButton(GameObject template, string label, UnityEngine.Events.UnityAction onClick)
        {
            if (_container == null) return null;

            try
            {
                GameObject btn = Instantiate(template, _container.transform);
                btn.name = $"BTN_{label.Replace(" ", "")}";
                
                // Reset RectTransform for LayoutGroup compatibility
                RectTransform rect = btn.GetComponent<RectTransform>();
                if (rect != null)
                {
                    // Reset Anchors to center-middle so LayoutGroup can move them easily
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition3D = Vector3.zero;
                    rect.localRotation = Quaternion.identity;
                    rect.localScale = Vector3.one;
                }

                btn.SetActive(true);

                // Fix Layout Element if needed
                LayoutElement le = btn.GetComponent<LayoutElement>();
                if (le == null) le = btn.AddComponent<LayoutElement>();
                le.minWidth = 300f; // Default button width
                le.minHeight = 50f;
                le.preferredWidth = 300f;
                le.preferredHeight = 50f;
                le.flexibleWidth = 0;
                le.flexibleHeight = 0;

                // Set Text
                var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.text = label;
                    tmp.alignment = TextAlignmentOptions.Center;
                }

                // Clean up native animators
                Animator nativeAnim = btn.GetComponent<Animator>();
                if (nativeAnim != null) Destroy(nativeAnim);

                Component scaler = btn.GetComponent("ButtonScaler");
                if (scaler != null) Destroy(scaler);

                // Add Click
                Button b = btn.GetComponent<Button>();
                if (b != null)
                {
                    b.onClick = new Button.ButtonClickedEvent();
                    b.onClick.AddListener(onClick);
                }
                
                // Add PopUpAnimator if missing
                if (btn.GetComponent<PopUpAnimator>() == null)
                    btn.AddComponent<PopUpAnimator>();

                // Force Layout Rebuild
                LayoutRebuilder.ForceRebuildLayoutImmediate(_container.GetComponent<RectTransform>());
                MelonLogger.Msg($"[MenuBank] Added button: {label}");

                return btn;
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[MenuBank] AddButton Failed: {ex}");
                return null;
            }
        }
    }
}