using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WindyFramework.Core;
using System.Collections.Generic;
using MelonLoader;

namespace WindyFramework.Modules.UI.Features
{
    public class BrowserScreen : MonoBehaviour
    {
        public GameObject HomeScreen;
        public Transform ListContainer;
        public GameObject ButtonTemplate; // We'll clone a setting item as a template

        private void OnEnable()
        {
            if (WindyManager.Instance == null) return;
            WindyManager.Instance.OnLobbiesUpdated += RebuildUI;
            Refresh();
        }

        private void OnDisable()
        {
            if (WindyManager.Instance == null) return;
            WindyManager.Instance.OnLobbiesUpdated -= RebuildUI;
        }

        public void Refresh()
        {
            if (ListContainer == null) return;

            try
            {
                // Trigger network request
                WindyManager.Instance.RefreshLobbyList();

                // Show cached results immediately if any
                RebuildUI();
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[UIModule] Refresh Error: {ex}");
            }
        }

        private void RebuildUI()
        {
            if (ListContainer == null) return;

            try
            {
                // Clear existing
                foreach (Transform child in ListContainer)
                {
                    if (ButtonTemplate != null && child.gameObject != ButtonTemplate)
                        Destroy(child.gameObject);
                    else if (ButtonTemplate == null)
                        Destroy(child.gameObject);
                }

                var lobbies = WindyManager.Instance.FoundLobbies;
                foreach (var lobby in lobbies)
                {
                    CreateEntry(lobby);
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[UIModule] RebuildUI Error: {ex}");
            }
        }

        private void CreateEntry(WindyManager.LobbyData lobby)
        {
            if (ButtonTemplate == null) return;

            try
            {
                GameObject entry = Instantiate(ButtonTemplate, ListContainer);
                entry.SetActive(true);

                Transform container = entry.transform.Find("Container");
                if (container == null) return;

                // 1. Activate Info Panel
                Transform info = container.Find("Info");
                if (info != null) info.gameObject.SetActive(true);

                // 2. Map Data
                // Organisation -> Business Name
                string business = !string.IsNullOrEmpty(lobby.BusinessName) ? lobby.BusinessName : $"{lobby.HostName}'s Biz";
                SetText(container, "Info/Organisation", business);

                // NetWorth -> Net Worth
                string worth = !string.IsNullOrEmpty(lobby.NetWorth) ? lobby.NetWorth : "$0";
                SetText(container, "Info/NetWorth/Text", worth);

                // Created -> Host Name
                SetText(container, "Info/Created/Text", $"Host: {lobby.HostName}");

                // LastPlayed -> Game Mode
                string mode = !string.IsNullOrEmpty(lobby.GameMode) ? lobby.GameMode : "Freemode";
                SetText(container, "Info/LastPlayed/Text", mode);

                // Version -> Version
                SetText(container, "Info/Version", $"v{lobby.Version}");

                // 3. Hide Unwanted Buttons (Import/Export/Delete)
                Transform[] allChildren = container.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    string name = child.name.ToLower();
                    if (name.Contains("import") || name.Contains("export") || name.Contains("delete"))
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                // 4. Setup Button
                Button btn = container.Find("Button")?.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                        WindyManager.Instance.JoinLobbyDirect(lobby);
                    }));
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[UIModule] CreateEntry Error: {ex}");
            }
        }

        private void SetText(Transform root, string path, string val)
        {
            Transform t = root.Find(path);
            if (t != null)
            {
                var tmp = t.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = val;
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
            if (HomeScreen != null) HomeScreen.SetActive(true);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }
    }
}
