using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WindyFramework.Modules.UI.Features;
using MelonLoader;

namespace WindyFramework.Modules.UI.Patches
{
    public static class MenuInjector
    {
        public static MenuBank BankInstance { get; private set; }

        public static void Inject()
        {
            try
            {
                GameObject mainMenu = GameObject.Find("MainMenu");
                if (mainMenu == null) return;

                Transform home = mainMenu.transform.Find("Home");
                Transform bank = mainMenu.transform.Find("Home/Bank");

                if (home == null || bank == null) return;

                // --- 1. SETUP UI BANK CONTROLLER ---
                if (BankInstance == null)
                {
                    GameObject bankObj = new GameObject("MenuBank_Controller");
                    BankInstance = bankObj.AddComponent<MenuBank>();
                    BankInstance.Initialize(home, new Vector3(0f, 60f, 0f));
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
                Transform listContainer = browserPage.transform.Find("Container");
                GameObject listTemplate = null;

                if (listContainer != null)
                {
                    Transform slotSource = listContainer.Find("Slot");
                    if (slotSource != null)
                    {
                        listTemplate = slotSource.gameObject;
                        listTemplate.SetActive(false);
                    }

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

                // --- 3. ADD BUTTONS TO BANK ---
                Transform sourceBtn = bank.Find("Settings");
                if (sourceBtn == null) return;

                // Add Server Browser Button
                BankInstance.AddButton(sourceBtn.gameObject, "Server Browser", () => {
                    MelonLogger.Msg("[WindyUI] Opening Browser...");
                    if (controller.HomeScreen != null) controller.HomeScreen.SetActive(false);        
                    browserPage.SetActive(true);
                });

                // Add Survival Button
                BankInstance.AddButton(sourceBtn.gameObject, "Survival", () => {
                    Zordon.ScheduleI.Survival.Features.SurvivalLaunch.StartSurvival();
                });

                MelonLogger.Msg("[WindyUI] Menu Bank Injected.");
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[WindyUI] UI Injection Failed: {ex.Message}");
            }
        }
    }
}