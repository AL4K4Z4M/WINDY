using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WindyFramework.Modules.UI.Features;
using MelonLoader;

namespace WindyFramework.Modules.UI.Patches
{
    public static class MenuInjector
    {
        public static void Inject()
        {
            try
            {
                GameObject mainMenu = GameObject.Find("MainMenu");
                if (mainMenu == null) return;

                Transform home = mainMenu.transform.Find("Home");
                Transform bank = mainMenu.transform.Find("Home/Bank");

                if (home == null || bank == null) return;

                // --- 1. INJECT ENTRY BUTTON (BOTTOM CENTER) ---
                // Clone 'Settings' from Bank to get the correct button style
                Transform sourceBtn = bank.Find("Settings");
                if (sourceBtn == null) return;

                // Parent to 'Home' directly to escape the Bank's VerticalLayoutGroup
                GameObject browserBtn = Object.Instantiate(sourceBtn.gameObject, home);
                browserBtn.name = "MOD_ServerBrowser";

                // MANUAL POSITIONING: Bottom Center
                RectTransform rect = browserBtn.GetComponent<RectTransform>();
                if (rect != null)
                {
                    // Anchors: Bottom Center
                    rect.anchorMin = new Vector2(0.5f, 0f);
                    rect.anchorMax = new Vector2(0.5f, 0f);
                    rect.pivot = new Vector2(0.5f, 0f);

                    // Position: 0 X (Center), 60 Y (Padding from bottom)
                    rect.anchoredPosition3D = new Vector3(0f, 60f, 0f);
                    rect.localRotation = Quaternion.identity;
                    rect.localScale = Vector3.one;

                    // Force Size (Settings button might be wide/stretched)
                    rect.sizeDelta = new Vector2(300f, 50f);
                }

                // ANIMATION OVERRIDE:
                // 1. Kill native Animator (if present)
                Animator nativeAnim = browserBtn.GetComponent<Animator>();
                if (nativeAnim != null) Object.Destroy(nativeAnim);

                // 2. Kill ButtonScaler (Replaced by our custom Hover Up anim)
                Component scaler = browserBtn.GetComponent("ButtonScaler");
                if (scaler != null) Object.Destroy(scaler);

                // 3. Add our custom "Pop Up" animator
                browserBtn.AddComponent<PopUpAnimator>();

                // Interaction Fix
                CanvasGroup cg = browserBtn.GetComponent<CanvasGroup>();
                if (cg == null) cg = browserBtn.AddComponent<CanvasGroup>();
                cg.interactable = true;
                cg.blocksRaycasts = true;
                cg.ignoreParentGroups = true;

                browserBtn.SetActive(true);

                // Fix Text
                Transform textContainer = browserBtn.transform.Find("TextContainer/Text");
                if (textContainer != null) {
                    TextMeshProUGUI label = textContainer.GetComponent<TextMeshProUGUI>();
                    if (label != null) {
                        label.text = "Server Browser";
                        label.alignment = TextAlignmentOptions.Center; // Center align text
                    }
                }

                // --- 2. CREATE BROWSER PAGE ---
                // Clone 'Continue' screen to get the Save Slot list style
                Transform continueSource = mainMenu.transform.Find("Continue");
                if (continueSource == null) return;

                GameObject browserPage = Object.Instantiate(continueSource.gameObject, mainMenu.transform);
                browserPage.name = "MOD_BrowserPage";
                browserPage.SetActive(false);

                // Strip Native Logic
                Component[] nativeScripts = browserPage.GetComponents<Component>();
                foreach (var comp in nativeScripts) {
                    if (!(comp is RectTransform) && !(comp is CanvasRenderer)) {
                        Object.Destroy(comp);
                    }
                }

                // Setup Title
                Transform title = browserPage.transform.Find("Title");
                if (title != null) {
                    var tmp = title.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null) tmp.text = "Server Browser";
                }

                // Setup List Container
                // Continue screen structure: Continue -> Container -> Slot, Slot (1)...
                Transform listContainer = browserPage.transform.Find("Container");
                GameObject listTemplate = null;

                if (listContainer != null)
                {
                    // Use the first slot as our template
                    Transform slotSource = listContainer.Find("Slot");
                    if (slotSource != null)
                    {
                        listTemplate = slotSource.gameObject;
                        listTemplate.SetActive(false);
                    }

                    // Clear other slots
                    foreach (Transform child in listContainer)
                    {
                        if (child.gameObject != listTemplate) Object.Destroy(child.gameObject);
                    }
                }

                // Add Controller
                BrowserScreen controller = browserPage.AddComponent<BrowserScreen>();
                controller.HomeScreen = mainMenu.transform.Find("Home")?.gameObject;
                controller.ListContainer = listContainer;
                controller.ButtonTemplate = listTemplate;

                // --- 3. WIRE BUTTON ---
                Button btn = browserBtn.GetComponent<Button>();
                if (btn != null)
                {
                    btn.interactable = true;
                    // NUCLEAR OPTION: Completely replace the event container
                    btn.onClick = new Button.ButtonClickedEvent();

                    btn.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                        MelonLogger.Msg("[WindyUI] Opening Browser...");
                        if (controller.HomeScreen != null) controller.HomeScreen.SetActive(false);        
                        browserPage.SetActive(true);
                    }));
                }

                MelonLogger.Msg("[WindyUI] Native Server Browser injected to Bottom Center (Clean Event).");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WindyUI] UI Injection Failed: {ex.Message}");
            }
        }
    }
}
