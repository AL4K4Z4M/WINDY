using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using System.Collections;
using WindyFramework.Core;

namespace WindyFramework.Modules.UI.Features
{
    public static class PhoneInjector
    {
        private static string _homeIconsPath = "Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/HomeScreen/AppIcons";
        private static string _appsCanvasPath = "Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/AppsCanvas";
        
        private static GameObject _appPanel;
        private static bool _injecting = false;

        public static void TryInject()
        {
            if (_injecting) return;
            MelonCoroutines.Start(WaitForPhone());
        }

        private static IEnumerator WaitForPhone()
        {
            _injecting = true;
            int attempts = 0;
            while (attempts < 60)
            {
                if (GameObject.Find(_homeIconsPath) != null && GameObject.Find(_appsCanvasPath) != null)
                {
                    InjectApp();
                    _injecting = false;
                    yield break;
                }
                attempts++;
                yield return new WaitForSeconds(1f);
            }
            _injecting = false;
            MelonLogger.Error("[PhoneInjector] Failed to find Phone UI.");
        }

        private static void InjectApp()
        {
            try
            {
                // 1. Create Icon
                GameObject iconsContainer = GameObject.Find(_homeIconsPath);
                if (iconsContainer == null) return;

                // CHECK: Deduplication
                if (iconsContainer.transform.Find("AppIcon_HostManager") != null)
                {
                    MelonLogger.Msg("[PhoneInjector] Host Manager App already injected. Skipping.");
                    return;
                }

                // Clone the last icon
                if (iconsContainer.transform.childCount == 0) return;
                GameObject templateIcon = iconsContainer.transform.GetChild(iconsContainer.transform.childCount - 1).gameObject;
                
                GameObject newIcon = Object.Instantiate(templateIcon, iconsContainer.transform);
                newIcon.name = "AppIcon_HostManager";
                
                // Update Label
                Transform label = newIcon.transform.Find("Label");
                if (label != null) label.GetComponent<Text>().text = "Host Mgr";

                // 2. Create App Panel
                GameObject appsCanvas = GameObject.Find(_appsCanvasPath);
                Transform templateApp = appsCanvas.transform.Find("ProductManagerApp");
                if (templateApp == null)
                {
                    MelonLogger.Error("[PhoneInjector] ProductManagerApp template not found!");
                    return;
                }

                // Prevent Awake/OnEnable from running during instantiation
                bool wasActive = templateApp.gameObject.activeSelf;
                templateApp.gameObject.SetActive(false);

                _appPanel = Object.Instantiate(templateApp.gameObject, appsCanvas.transform);
                
                // Restore template state
                templateApp.gameObject.SetActive(wasActive);

                _appPanel.name = "HostManagerApp_Panel";
                // Ensure it stays inactive
                _appPanel.SetActive(false);

                // Strip Native Logic to prevent interference
                Component[] nativeScripts = _appPanel.GetComponents<Component>();
                foreach (var comp in nativeScripts) {
                    if (!(comp is RectTransform) && !(comp is CanvasRenderer) && !(comp is Image) && !(comp is CanvasGroup)) {
                        Object.Destroy(comp);
                    }
                }

                // Destroy all children (UI elements of the copied app)
                foreach (Transform child in _appPanel.transform) {
                    Object.Destroy(child.gameObject);
                }

                // Force Visibility Properties
                CanvasGroup cg = _appPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = _appPanel.AddComponent<CanvasGroup>();
                cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
                cg.ignoreParentGroups = false;

                // Reset Scale/Position by copying Template
                RectTransform rect = _appPanel.GetComponent<RectTransform>();
                RectTransform templateRect = templateApp.GetComponent<RectTransform>();
                if (rect != null && templateRect != null)
                {
                    rect.localScale = templateRect.localScale;
                    rect.localRotation = Quaternion.identity; // Force identity to prevent rotation glitches
                    rect.localPosition = templateRect.localPosition;
                    rect.anchorMin = templateRect.anchorMin;
                    rect.anchorMax = templateRect.anchorMax;
                    rect.pivot = templateRect.pivot;
                    rect.sizeDelta = templateRect.sizeDelta;
                }

                RectTransform parentRect = appsCanvas.GetComponent<RectTransform>();
                MelonLogger.Msg($"[PhoneInjector] Parent ({appsCanvas.name}) Size: {parentRect?.sizeDelta}");

                // Attach our script
                _appPanel.AddComponent<HostManagerApp>();

                // 3. Wire Button
                Button btn = newIcon.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick = new Button.ButtonClickedEvent(); // Clear existing
                    btn.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                        MelonLogger.Msg("[PhoneInjector] Icon clicked!");
                        if (_appPanel != null)
                        {
                            _appPanel.SetActive(true);
                            _appPanel.transform.SetAsLastSibling();
                            
                            var cg = _appPanel.GetComponent<CanvasGroup>();
                            if (cg != null) cg.alpha = 1f;

                            // Force all children active
                            foreach(Transform child in _appPanel.transform) child.gameObject.SetActive(true);

                            RectTransform r = _appPanel.GetComponent<RectTransform>();
                            MelonLogger.Msg($"[PhoneInjector] Panel activated. Pos: {r.position}, Size: {r.sizeDelta}, Parent: {_appPanel.transform.parent.name}, Alpha: {cg?.alpha}");
                        }
                        else
                        {
                            MelonLogger.Error("[PhoneInjector] Panel is null!");
                        }
                    }));
                }

                MelonLogger.Msg("[PhoneInjector] Host Manager App injected!");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[PhoneInjector] Injection failed: {ex}");
            }
        }
    }
}
