using System;
using UnityEngine;
using MelonLoader;
using ScheduleOne.PlayerScripts;
using ScheduleOne.NPCs;
using ScheduleOne.Persistence;
using System.Linq;

namespace WindyFramework.Modules.NPCGenerator
{
    public class NPCGeneratorUI
    {
        private static NPCGeneratorUI _instance;
        public static NPCGeneratorUI Instance => _instance ?? (_instance = new NPCGeneratorUI());

        public bool IsOpen { get; private set; } = false;
        private Rect _windowRect = new Rect(20, 20, 300, 200);

        private string _firstName = "John";
        private string _lastName = "Doe";

        public void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F4))
            {
                IsOpen = !IsOpen;
                if (IsOpen)
                {
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                }
                else
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
            }
        }

        public void OnGUI()
        {
            if (!IsOpen) return;

            _windowRect = GUI.Window(2001, _windowRect, DrawWindow, "NPC GENERATOR (F4)");
        }

        private void DrawWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 300, 20));

            GUILayout.BeginVertical();
            GUILayout.Space(10);

            GUILayout.Label("New NPC Details");

            GUILayout.BeginHorizontal();
            GUILayout.Label("First Name:", GUILayout.Width(80));
            _firstName = GUILayout.TextField(_firstName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Last Name:", GUILayout.Width(80));
            _lastName = GUILayout.TextField(_lastName);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (GUILayout.Button("GENERATE NPC"))
            {
                GenerateNPC();
            }

            GUILayout.EndVertical();
        }

        private void GenerateNPC()
        {
            if (Player.Local == null)
            {
                MelonLogger.Warning("[NPCGenerator] Player not found!");
                return;
            }

            var template = GameObject.FindObjectOfType<NPC>();
            if (template == null)
            {
                MelonLogger.Warning("[NPCGenerator] No existing NPC found to clone!");
                return;
            }

            try
            {
                // Calculate position: 3m forward, 0.5m up
                Vector3 spawnPos = Player.Local.transform.position + (Player.Local.transform.forward * 3f) + (Vector3.up * 0.5f);
                Quaternion spawnRot = Quaternion.LookRotation(-Player.Local.transform.forward); // Face player

                // Clone
                GameObject newNPCObj = GameObject.Instantiate(template.gameObject, spawnPos, spawnRot);
                newNPCObj.name = $"NPC_{_firstName}_{_lastName}";

                NPC npcComponent = newNPCObj.GetComponent<NPC>();
                if (npcComponent != null)
                {
                    // Set Details
                    npcComponent.FirstName = _firstName;
                    npcComponent.LastName = _lastName;

                    // Generate new IDs
                    npcComponent.ID = Guid.NewGuid().ToString();
                    npcComponent.SetGUID(Guid.NewGuid());

                    // Reset State
                    if (npcComponent.Health != null)
                    {
                        npcComponent.Health.Revive();
                        npcComponent.Health.RestoreHealth();
                    }

                    // Ensure it is active
                    newNPCObj.SetActive(true);

                    MelonLogger.Msg($"[NPCGenerator] Generated NPC: {_firstName} {_lastName} at {spawnPos}");
                }
                else
                {
                    MelonLogger.Error("[NPCGenerator] Cloned object does not have NPC component!");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[NPCGenerator] Error generating NPC: {ex}");
            }
        }
    }
}
