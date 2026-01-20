using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MelonLoader;
using WindyFramework.Core;

namespace WindyFramework.Modules.UI.Features
{
    public class HostManagerApp : MonoBehaviour
    {
        public static HostManagerApp Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            MelonLogger.Msg("[HostManagerApp] Waking up...");
        }

        private void OnEnable()
        {
            RefreshUI();
        }

        public void RefreshUI()
        {
            // TODO: Implement Player List and Settings population
            MelonLogger.Msg("[HostManagerApp] Refreshing UI...");
        }
        
        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
