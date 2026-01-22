using UnityEngine;
using MelonLoader;
using System.Collections;
using System.Collections.Generic;
using EPOOutline;
using ScheduleOne;
using ScheduleOne.PlayerScripts;
using ScheduleOne.Tools;
using Zordon.ScheduleI.WorldEditor.Features;

namespace Zordon.ScheduleI.WorldEditor.Features
{
    public class EditorController : MonoBehaviour
    {
        private static EditorController _instance;
        public static EditorController Instance
        {
            get
            {
                if (_instance == null || !_instance.gameObject)
                {
                    var go = new GameObject("WorldEditorController");
                    Object.DontDestroyOnLoad(go);
                    _instance = go.AddComponent<EditorController>();
                }
                return _instance;
            }
        }

        public bool IsEditorActive { get; private set; } = false;
        private bool _isMenuFocused = true;

        private GameObject _selectedObject;
        private Outlinable _outline;
        private LayerMask _selectionMask;

        public enum SelectionMode { Smart, Precision, Root }
        public SelectionMode CurrentSelectionMode { get; private set; } = SelectionMode.Smart;

        // Physics Handling
        private bool _wasKinematic;
        private Rigidbody _selectedRb;

        // UI Layout
        private Rect _inspectorRect = new Rect(20, 20, 300, 650);
        private float[] _snapDistances = { 0f, 0.05f, 0.1f, 0.25f, 0.5f, 1f, 2f };
        private int _snapDistIndex = 0;
        private float[] _snapAngles = { 0f, 5f, 15f, 30f, 45f, 90f };
        private int _snapAngleIndex = 0;
        public bool IsLocalSpace = false;
        private bool _groundSnap = true;
        private bool _occlusionFixEnabled = true;
        private bool _autoRoot = true;

        private bool _showPalette = false;
        private bool _showMapMenu = false;
        private string _paletteSearch = "";
        private string _mapSaveName = "MyMap";
        private string _currentMapName = "";
        
        private class SpawnableItem
        {
            public string Name;
            public string ID; 
            public string Category;
            public enum ItemType { Buildable, Vehicle, Tree, Template, Generic }
            public ItemType Type;
            public ScheduleOne.ItemFramework.BuildableItemDefinition BuildableDef;
            public ScheduleOne.Vehicles.LandVehicle VehiclePrefab;
            public GameObject TreePrefab;
            public GameObject GenericPrefab;
            public Zordon.ScheduleI.WorldEditor.Models.ObjectTemplate Template;
        }
        
        private List<SpawnableItem> _spawnables = new List<SpawnableItem>();
        private List<SpawnableItem> _filteredSpawnables = new List<SpawnableItem>();
        private string _lastSearch = "";
        private int _lastCategory = -1;

        private string[] _paletteCategories = { "All", "Structural", "Urban", "Nature", "Furniture", "Industrial", "Drug Lab", "Vehicles", "Misc Props", "Templates" };
        private int _selectedCategoryIndex = 0;
        private Vector2 _paletteScroll = Vector2.zero;
        private bool _showGizmos = true;

        // Clipboard
        private string _clipboardSourceID;
        private Vector3 _clipboardScale = Vector3.one;
        private GameObject _clipboardTemplate;

        private struct TransformState
        {
            public Vector3 Position;
            public Vector3 Rotation;
            public Vector3 Scale;
        }
        private List<TransformState> _undoStack = new List<TransformState>();
        private bool _isDragging = false;

        private List<GameObject> _globallyHiddenProxies = new List<GameObject>();
        private List<LODGroup> _disabledLODGroups = new List<LODGroup>();

        private void Start()
        {
            _selectionMask = LayerMask.GetMask("Default", "Terrain", "Vehicle", "NPC", "StoredItem", "Trash");
            LoadSpawnables();
        }

        private void LoadSpawnables()
        {
            _spawnables.Clear();
            HashSet<string> addedNames = new HashSet<string>();

            // 1. Load Registry buildables
            if (Registry.Instance != null)
            {
                var allItems = Registry.Instance.GetAllItems();
                if (allItems != null)
                {
                    foreach (var item in allItems)
                    {
                        if (item is ScheduleOne.ItemFramework.BuildableItemDefinition buildable)
                        {
                            if (addedNames.Add(buildable.ID))
                                _spawnables.Add(new SpawnableItem { Name = buildable.Name, ID = buildable.ID, Category = "Structural", Type = SpawnableItem.ItemType.Buildable, BuildableDef = buildable });
                        }
                    }
                }
            }

            // 2. Load Vehicles
            if (ScheduleOne.Vehicles.VehicleManager.Instance != null && ScheduleOne.Vehicles.VehicleManager.Instance.VehiclePrefabs != null)
            {
                foreach (var veh in ScheduleOne.Vehicles.VehicleManager.Instance.VehiclePrefabs)
                {
                    if (veh == null || !addedNames.Add($"VEH_{veh.VehicleCode}")) continue;
                    _spawnables.Add(new SpawnableItem { Name = $"Vehicle: {veh.VehicleCode}", ID = $"VEH_{veh.VehicleCode}", Category = "Vehicles", Type = SpawnableItem.ItemType.Vehicle, VehiclePrefab = veh });
                }
            }

            // 3. Load Trees
            if (Terrain.activeTerrain != null && Terrain.activeTerrain.terrainData != null)
            {
                var prototypes = Terrain.activeTerrain.terrainData.treePrototypes;
                if (prototypes != null)
                {
                    for (int i = 0; i < prototypes.Length; i++)
                    {
                        var prefab = prototypes[i].prefab;
                        if (prefab != null && addedNames.Add($"TREE_{prefab.name}")) 
                            _spawnables.Add(new SpawnableItem { Name = $"Tree: {prefab.name}", ID = $"TREE_{prefab.name}", Category = "Nature", Type = SpawnableItem.ItemType.Tree, TreePrefab = prefab });
                    }
                }
            }

            // 4. Load Generic World Assets from Memory (The categorization logic)
            GameObject[] allPrefabs = Resources.FindObjectsOfTypeAll<GameObject>();
            
            var catMap = new Dictionary<string, string[]> {
                { "Structural", new[] { "wall", "fence", "floor", "roof", "truss", "beam", "column", "foundation", "stairs", "railing", "brick", "concrete" } },
                { "Urban", new[] { "bus stop", "trash", "bench", "atm", "billboard", "bollard", "light", "street", "mailbox", "vending", "bin", "dumpster", "parking", "sign", "trafficcone", "pay phone", "intercom", "vms" } },
                { "Nature", new[] { "bush", "hedge", "tree", "rock", "dirt", "plant", "cactus", "shroom", "mushroom", "grass", "leaf", "shrub", "flower", "coca", "weed" } },
                { "Furniture", new[] { "table", "chair", "sofa", "shelf", "cabinet", "bed", "desk", "drawer", "rack", "lamp", "ottoman", "stool", "wardrobe", "whiteboard", "painting" } },
                { "Industrial", new[] { "container", "crate", "barrel", "pallet", "tank", "valve", "pipe", "crane", "forklift", "mixer", "skip bin", "generator" } },
                { "Drug Lab", new[] { "beaker", "flask", "burner", "station", "mixing", "chemistry", "oven", "mortar", "pestle", "syringe", "vial", "acid", "phosphorus", "iodine", "meth", "cocaine", "press", "grow", "pot", "tray" } },
                { "Misc Props", new[] { "ashtray", "bottle", "can", "donut", "battery", "clipboard", "hammer", "machete", "baton", "revolver", "shotgun", "taser", "lighter", "cigarette", "bag", "note" } }
            };

            foreach (var go in allPrefabs)
            {
                if (go == null || !string.IsNullOrEmpty(go.scene.name)) continue;
                if (addedNames.Contains(go.name)) continue;

                // Basic sanity check: Does it have a Transform and is it not a UI/Internal object?
                // We'll still allow them, but categorization helps.
                string lowerName = go.name.ToLower();
                string foundCategory = "Misc Props"; // Default category

                foreach (var kvp in catMap)
                {
                    if (System.Array.Exists(kvp.Value, kw => lowerName.Contains(kw)))
                    {
                        foundCategory = kvp.Key;
                        break;
                    }
                }

                if (addedNames.Add(go.name))
                {
                    _spawnables.Add(new SpawnableItem { Name = go.name, ID = go.name, Category = foundCategory, Type = SpawnableItem.ItemType.Generic, GenericPrefab = go });
                }
            }

            // 5. Templates
            if (TemplateManager.Instance != null && TemplateManager.Instance.Templates != null)
            {
                foreach (var tmpl in TemplateManager.Instance.Templates)
                {
                    _spawnables.Add(new SpawnableItem { Name = $"* {tmpl.Name}", ID = tmpl.SourceItemID, Category = "Templates", Type = SpawnableItem.ItemType.Template, Template = tmpl });
                }
            }
            MelonLogger.Msg($"World Editor: Loaded {_spawnables.Count} spawnable items across {_paletteCategories.Length} categories.");
        }

        public void ToggleEditor()
        {
            IsEditorActive = !IsEditorActive;
            MelonLogger.Msg($"World Editor: {(IsEditorActive ? "ENABLED" : "DISABLED")}");
            if (IsEditorActive)
            {
                try { if (_spawnables.Count == 0) LoadSpawnables(); }
                catch (System.Exception ex) { MelonLogger.Error($"Error loading spawnables: {ex.Message}"); }
                _isMenuFocused = true;
                
                // Enable Freecam
                if (PlayerCamera.Instance != null)
                {
                    PlayerCamera.Instance.SetFreeCam(true);
                    PlayerCamera.Instance.OpenInterface(true, true);
                }

                // NUCLEAR OPTION: Cleanup optimization proxies globally
                SetGlobalOptimizationState(false);
            }
            else
            {
                // Restore World
                SetGlobalOptimizationState(true);
                ClearSelection();
                _showPalette = false;
                
                // Disable Freecam and return to player
                if (PlayerCamera.Instance != null)
                {
                    PlayerCamera.Instance.SetFreeCam(false);
                    PlayerCamera.Instance.CloseInterface();
                }
            }
        }

        private void SetGlobalOptimizationState(bool normalGameMode)
        {
            if (normalGameMode)
            {
                MelonLogger.Msg("World Editor: Restoring global optimization proxies...");
                foreach (var go in _globallyHiddenProxies) if (go) go.SetActive(true);
                _globallyHiddenProxies.Clear();

                foreach (var lod in _disabledLODGroups)
                {
                    if (lod)
                    {
                        lod.enabled = true;
                        lod.ForceLOD(-1);
                        // Also re-enable all LOD gameobjects so the system can choose
                        foreach (var l in lod.GetLODs())
                        {
                            foreach (var r in l.renderers) if (r) r.gameObject.SetActive(true);
                        }
                    }
                }
                _disabledLODGroups.Clear();

                // Re-enable custom optimization components
                var scripts = Object.FindObjectsOfType<MonoBehaviour>();
                foreach (var s in scripts)
                {
                    if (s == null) continue;
                    string name = s.GetType().Name;
                    if (name == "MeshCombiner" || name == "MeshMerger" || name == "UnluckDistanceDisabler") s.enabled = true;
                }
            }
            else
            {
                MelonLogger.Msg("World Editor: Performing global optimization cleanup...");
                _globallyHiddenProxies.Clear();
                _disabledLODGroups.Clear();

                // 1. Aggressively hide combined meshes and proxies GLOBALLY
                GameObject[] allGos = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in allGos)
                {
                    if (go == null || !string.IsNullOrEmpty(go.scene.name)) continue; 
                    
                    if (IsOptimizationProxy(go))
                    {
                        if (go.activeSelf)
                        {
                            go.SetActive(false);
                            _globallyHiddenProxies.Add(go);
                        }
                    }
                }

                // 2. Disable LODGroups and force High Detail
                LODGroup[] allLods = Object.FindObjectsOfType<LODGroup>();
                foreach (var lod in allLods)
                {
                    if (lod == null) continue;
                    
                    var lods = lod.GetLODs();
                    if (lods.Length > 0)
                    {
                        // Enable LOD0 renderers
                        foreach (var r in lods[0].renderers) if (r) r.gameObject.SetActive(true);
                        
                        // Disable other LOD renderers
                        for (int i = 1; i < lods.Length; i++)
                        {
                            foreach (var r in lods[i].renderers) 
                            {
                                if (r && r.gameObject)
                                {
                                    bool inLOD0 = false;
                                    foreach (var r0 in lods[0].renderers) if (r0 == r) inLOD0 = true;
                                    if (!inLOD0) r.gameObject.SetActive(false);
                                }
                            }
                        }
                    }

                    lod.enabled = false; 
                    _disabledLODGroups.Add(lod);
                }

                // 3. Disable custom optimization scripts
                var scripts = Object.FindObjectsOfType<MonoBehaviour>();
                foreach (var s in scripts)
                {
                    if (s == null) continue;
                    string name = s.GetType().Name;
                    if (name == "MeshCombiner" || name == "MeshMerger" || name == "UnluckDistanceDisabler") s.enabled = false;
                }
                
                MelonLogger.Msg($"World Editor: Global cleanup complete. Purged {_globallyHiddenProxies.Count} objects.");
            }
        }

        private void RefreshAllWorldVisuals()
        {
            SetGlobalOptimizationState(true);
            SetGlobalOptimizationState(false);
            MelonLogger.Msg("World Visuals Refreshed (Global Cleanup Re-Run).");
        }

        private bool IsOptimizationProxy(GameObject obj)
        {
            if (obj == null) return false;
            string name = obj.name.ToLower();
            
            // Explicitly allowed high-detail markers
            if (name.Contains("lod0") || name.Contains("lod_0") || name.Contains("_high") || name.EndsWith("_h")) return false;
            
            // EXACT NAME PATTERNS FROM USER (Combined meshes that cover buildings)
            if (name.Contains("combined mesh") || name.Contains("combined_mesh") || name.Contains("combinedmesh")) return true;
            if (name.Contains("district") && name.Contains("combined")) return true;
            if (name.Contains("neighborhood") && name.Contains("combined")) return true;
            if (name.Contains("region_") && name.Contains("combined")) return true;

            // Identify optimization indicators
            if (System.Text.RegularExpressions.Regex.IsMatch(name, @"lod[1-9]")) return true;

            if (name.Contains("proxy") || name.Contains("hlod") || name.Contains("billboard") || name.Contains("_far") || 
                name.Contains("_dist") || name.Contains("_distance") || name.Contains("_simple") || 
                name.Contains("_baked") || name.Contains("stencil") || name.Contains("occluder") || 
                name.Contains("_low") || name.Contains("_mid") || name.EndsWith("_l") ||
                name.Contains("imposter") || name.Contains("lowpoly") || name.Contains("chunkmesh") || name.Contains("combined"))
            {
                // Safety check: ensure it's not actually a building name we want
                if (name.Contains("apartment") || name.Contains("center") || name.Contains("community")) return false;
                if (!name.Contains("lod0")) return true;
            }
            return false;
        }

        private bool IsLowDetailLOD(GameObject obj)
        {
            if (obj == null) return false;
            string name = obj.name.ToLower();
            if (System.Text.RegularExpressions.Regex.IsMatch(name, @"lod[1-9]")) return true;
            if (name.Contains("proxy") || name.Contains("combined") || name.Contains("lowpoly") || 
                name.Contains("_low") || name.Contains("_mid") || name.Contains("billboard")) return true;
            return false;
        }

        private void Update()
        {
            // PERSISTENT FIX: Constantly force occlusion culling OFF if fix is enabled
            if (_occlusionFixEnabled)
            {
                foreach (var cam in Camera.allCameras)
                {
                    if (cam != null && cam.useOcclusionCulling) cam.useOcclusionCulling = false;
                }
            }

            if (!IsEditorActive) return;

            // Handle cursor state based on RMB
            if (PlayerCamera.Instance != null && PlayerCamera.Instance.FreeCamEnabled)
            {
                bool holdingRightClick = Input.GetMouseButton(1);
                if (holdingRightClick) { Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked; }
                else if (_isMenuFocused) { Cursor.visible = true; Cursor.lockState = CursorLockMode.None; }
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                _isMenuFocused = !_isMenuFocused;
                if (PlayerCamera.Instance != null)
                {
                    if (_isMenuFocused) PlayerCamera.Instance.OpenInterface(true, true);
                    else PlayerCamera.Instance.CloseInterface(0.2f, false); // Don't re-enable look here, our Update handles it
                }
            }

            if (Input.GetKeyDown(KeyCode.F4))
            {
                CurrentSelectionMode = (SelectionMode)(((int)CurrentSelectionMode + 1) % 3);
                MelonLogger.Msg($"Selection Mode: {CurrentSelectionMode}");
            }

            if (Input.GetKeyDown(KeyCode.F3) && _selectedObject != null) SelectObject(GetSafeRoot(_selectedObject));

            if (_selectedObject != null && !_selectedObject) ClearSelection();

            if (Input.GetKeyDown(KeyCode.P)) _showPalette = !_showPalette;
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.M)) _showMapMenu = !_showMapMenu;

            if (_isMenuFocused)
            {
                HandleHotkeys();
                HandleSelection();
                HandleTransformation();
                HandleUndo();
            }
        }

        private void HandleHotkeys()
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.C) && _selectedObject != null) Copy(_selectedObject);
                if (Input.GetKeyDown(KeyCode.V)) Paste();
                if (Input.GetKeyDown(KeyCode.Z)) Undo();
            }
        }

        private void Copy(GameObject obj)
        {
            var patch = WorldPatchManager.Instance.GetPatchForObject(obj);
            _clipboardSourceID = patch?.SourceItemID;
            _clipboardScale = obj.transform.localScale;
            _clipboardTemplate = obj;
            MelonLogger.Msg($"Copied: {obj.name}");
        }

        private void Paste()
        {
            if (string.IsNullOrEmpty(_clipboardSourceID) && _clipboardTemplate == null) return;
            GameObject newObj = null;
            string sourceID = _clipboardSourceID;
            if (!string.IsNullOrEmpty(_clipboardSourceID))
            {
                var def = Registry.GetItem<ScheduleOne.ItemFramework.BuildableItemDefinition>(_clipboardSourceID);
                if (def != null && def.BuiltItem != null)
                {
                    newObj = Instantiate(def.BuiltItem.transform.gameObject);
                    newObj.name = def.Name + "_" + System.Guid.NewGuid().ToString().Substring(0, 8);
                }
            }
            else if (_clipboardTemplate != null)
            {
                newObj = Instantiate(_clipboardTemplate);
                newObj.name = _clipboardTemplate.name + "_Clone_" + System.Guid.NewGuid().ToString().Substring(0, 8);
            }
            if (newObj != null)
            {
                // Calculate horizontal forward position
                Vector3 camPos = Camera.main.transform.position;
                Vector3 camForward = Camera.main.transform.forward;
                camForward.y = 0;
                camForward.Normalize();
                
                Vector3 spawnPos = camPos + (camForward * 3f);
                
                // Find ground height
                if (Physics.Raycast(spawnPos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f, _selectionMask))
                {
                    spawnPos.y = hit.point.y + 1f;
                }
                else
                {
                    spawnPos.y = camPos.y;
                }

                newObj.transform.position = spawnPos;
                newObj.transform.localScale = _clipboardScale;
                GameObject container = GameObject.Find("WorldEditor_SpawnedObjects") ?? new GameObject("WorldEditor_SpawnedObjects");
                newObj.transform.SetParent(container.transform);
                
                // FORCE VISIBLE ONLY FOR SPAWNED/PASTED ITEMS
                ForceVisibleRecursively(newObj, true);
                
                SelectObject(newObj);
                WorldPatchManager.Instance.RegisterOrUpdatePatch(newObj, sourceID);
                MelonLogger.Msg($"Pasted: {newObj.name}");
            }
        }

        private void HandleUndo() { }

        private void Undo()
        {
            if (_selectedObject == null || _undoStack.Count == 0) return;
            var state = _undoStack[_undoStack.Count - 1];
            _undoStack.RemoveAt(_undoStack.Count - 1);
            _selectedObject.transform.position = state.Position;
            _selectedObject.transform.eulerAngles = state.Rotation;
            _selectedObject.transform.localScale = state.Scale;
            WorldPatchManager.Instance.RegisterOrUpdatePatch(_selectedObject);
            MelonLogger.Msg("Undo performed.");
        }

        private void RecordState()
        {
            if (_selectedObject == null) return;
            _undoStack.Add(new TransformState { Position = _selectedObject.transform.position, Rotation = _selectedObject.transform.eulerAngles, Scale = _selectedObject.transform.localScale });
            if (_undoStack.Count > 50) _undoStack.RemoveAt(0);
        }

        private void HandleSelection()
        {
            if (_showMapMenu) return; // Map menu is modal
            Vector2 mousePos = Input.mousePosition;
            float guiMouseY = Screen.height - mousePos.y;

            // Check if mouse is over Inspector
            if (_inspectorRect.Contains(new Vector2(mousePos.x, guiMouseY))) return;

            // Check if mouse is over Palette
            if (_showPalette)
            {
                float paletteWidth = 320;
                Rect paletteRect = new Rect(Screen.width - paletteWidth - 20, 20, paletteWidth, Screen.height - 40);
                if (paletteRect.Contains(new Vector2(mousePos.x, guiMouseY))) return;
            }

            // Check if mouse is over a Gizmo handle
            if (GizmoController.Instance != null && GizmoController.Instance.IsAnyHandleDragging) return;

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                GameObject target = null;

                // 1. Try RaycastAll to "Click Through" optimization proxies
                RaycastHit[] hits = Physics.RaycastAll(ray, 150f, _selectionMask);
                if (hits.Length > 0)
                {
                    // Sort by distance
                    System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                    
                    foreach (var hit in hits)
                    {
                        GameObject hitObj = hit.collider.gameObject;
                        // Skip objects that are known proxies/LODs to find the "real" geometry
                        if (IsOptimizationProxy(hitObj)) continue;
                        
                        target = hitObj;
                        break;
                    }

                    // Fallback: If we only hit proxies, take the closest one anyway
                    if (target == null && hits.Length > 0) target = hits[0].collider.gameObject;
                }
                
                // 2. If standard raycast failed or hit nothing, try a SphereCast for small items
                if (target == null)
                {
                    int layerTerrain = LayerMask.NameToLayer("Terrain");
                    LayerMask maskNoTerrain = _selectionMask & ~(1 << layerTerrain);
                    if (Physics.SphereCast(ray, 0.25f, out RaycastHit hit, 100f, maskNoTerrain)) 
                    {
                        if (!IsOptimizationProxy(hit.collider.gameObject)) target = hit.collider.gameObject;
                    }
                }
                
                if (target != null)
                {
                    // AUTO-ROOT: Automatically climb to building root if enabled
                    if (_autoRoot) target = GetSafeRoot(target);
                    SelectObject(target);
                }
            }
            if (Input.GetKeyDown(KeyCode.Escape)) ClearSelection();
        }

        private GameObject GetSafeRoot(GameObject obj)
        {
            if (obj == null) return null;
            Transform current = obj.transform;
            
            // If we click map or terrain, don't root climb
            string lowerName = obj.name.ToLower();
            if (obj.name == "Map" || lowerName.Contains("terrain") || lowerName.Contains("road") || lowerName.Contains("sidewalk")) return obj;

            int safety = 0;
            Transform bestRoot = current;
            while (current.parent != null && safety < 100)
            {
                safety++;
                string pName = current.parent.name;
                string pLower = pName.ToLower();

                if (pName == "Map" || pName == "Systems" || pName == "@Managers" || pName == "WorldEditor_SpawnedObjects") break;
                
                // Prioritize building identifiers
                if (pLower.Contains("building") || pLower.Contains("house") || pLower.Contains("store") || pLower.Contains("parlour") || pLower.Contains("parlor") || pLower.Contains("structure") || pLower.Contains("station") || pLower.Contains("office") || pLower.Contains("center") || pLower.Contains("shop") || pLower.Contains("lab") || pLower.Contains("factory"))
                {
                    bestRoot = current.parent;
                }

                // Neighborhood or Region is the absolute hard-limit
                if (pLower.StartsWith("region_") || pLower.Contains("neighborhood") || pLower.Contains("district") || pLower.Contains("props_group")) break;
                
                current = current.parent;
            }

            // Safety: If the best root we found is an optimization proxy, try to dive back to the original object
            if (IsOptimizationProxy(bestRoot.gameObject) && bestRoot != obj.transform) return obj;

            return bestRoot.gameObject;
        }

        private void SelectObject(GameObject obj)
        {
            if (obj == null) return;
            if (CurrentSelectionMode == SelectionMode.Root) obj = GetSafeRoot(obj);
            else if (CurrentSelectionMode == SelectionMode.Smart)
            {
                Transform current = obj.transform;
                int safety = 0;
                while (current.parent != null && safety < 50)
                {
                    safety++;
                    string pName = current.parent.name;
                    if (pName == "Map" || pName == "@Managers" || pName == "WorldEditor_SpawnedObjects") break;
                    string name = current.name.ToLower();
                    if (name.Contains("collider") || name.Contains("base") || name.Contains("billboard") || name.Contains("mesh") || name == "default" || name.StartsWith("cube") || name.StartsWith("cylinder") || name.Contains("lod")) current = current.parent;
                    else break; 
                }
                obj = current.gameObject;
            }
            
            // Restore visuals of previously selected object
            if (_selectedObject != null) RestoreVisualsRecursively(_selectedObject);

            ClearSelection();
            _selectedObject = obj;
            _undoStack.Clear(); 
            _selectedRb = _selectedObject.GetComponent<Rigidbody>();
            if (_selectedRb != null) { _wasKinematic = _selectedRb.isKinematic; _selectedRb.isKinematic = true; }
            
            UnlockStaticRecursively(_selectedObject);
            // WE NO LONGER CALL ForceVisibleRecursively HERE TO AVOID GLITCHING WORLD OBJECTS

            if (_showGizmos) GizmoController.Instance.SetTarget(_selectedObject);
            
            _outline = _selectedObject.GetComponent<Outlinable>() ?? _selectedObject.AddComponent<Outlinable>();
            _outline.OutlineParameters.Color = Color.cyan;
            _outline.OutlineParameters.BlurShift = 0f;
            _outline.OutlineParameters.DilateShift = 0.5f;
            var renderers = _selectedObject.GetComponentsInChildren<Renderer>();
            if (renderers.Length < 100) 
            { 
                foreach (var r in renderers) 
                { 
                    if (r is MeshRenderer || r is SkinnedMeshRenderer)
                        _outline.TryAddTarget(new OutlineTarget(r)); 
                } 
            }
            _outline.enabled = true;
            MelonLogger.Msg($"Selected: {obj.name}");
        }

        private void UnlockStaticRecursively(GameObject obj)
        {
            if (obj == null) return;
            obj.isStatic = false;
            foreach (Transform child in obj.transform) UnlockStaticRecursively(child.gameObject);
        }

        private void ClearSelection()
        {
            if (_selectedObject != null)
            {
                RestoreVisualsRecursively(_selectedObject);
                if (_selectedRb != null) { _selectedRb.isKinematic = _wasKinematic; _selectedRb = null; }
            }
            if (_outline != null) _outline.enabled = false;
            _selectedObject = null;
            _outline = null;
            _undoStack.Clear();
            GizmoController.Instance.SetTarget(null);
        }

        private void HandleTransformation()
        {
            if (_selectedObject == null || _showMapMenu) return;
            if (GUIUtility.keyboardControl != 0 || (GizmoController.Instance != null && GizmoController.Instance.IsAnyHandleDragging)) return;

            float moveSpeed = Input.GetKey(KeyCode.LeftShift) ? 5f : 1f;
            float rotateSpeed = 90f;
            bool changed = false;
            
            if (AnyTransformKeyHeld() && !_isDragging) { RecordState(); _isDragging = true; }
            else if (!AnyTransformKeyHeld() && _isDragging) _isDragging = false;
            
            Vector3 moveDir = Vector3.zero;
            // FIXED MOVEMENT DIRECTION
            if (Input.GetKey(KeyCode.UpArrow)) moveDir += Vector3.forward;
            if (Input.GetKey(KeyCode.DownArrow)) moveDir += Vector3.back;
            if (Input.GetKey(KeyCode.LeftArrow)) moveDir += Vector3.left;
            if (Input.GetKey(KeyCode.RightArrow)) moveDir += Vector3.right;
            if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp)) moveDir += Vector3.up;
            if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown)) moveDir += Vector3.down;
            
            if (moveDir != Vector3.zero)
            {
                // World vs Local space for arrow keys
                if (IsLocalSpace)
                {
                    Vector3 localMove = Vector3.zero;
                    if (Input.GetKey(KeyCode.UpArrow)) localMove += _selectedObject.transform.forward;
                    if (Input.GetKey(KeyCode.DownArrow)) localMove += -_selectedObject.transform.forward;
                    if (Input.GetKey(KeyCode.LeftArrow)) localMove += -_selectedObject.transform.right;
                    if (Input.GetKey(KeyCode.RightArrow)) localMove += _selectedObject.transform.right;
                    if (Input.GetKey(KeyCode.R)) localMove += _selectedObject.transform.up;
                    if (Input.GetKey(KeyCode.F)) localMove += -_selectedObject.transform.up;
                    _selectedObject.transform.position += localMove * moveSpeed * Time.deltaTime;
                }
                else
                {
                    _selectedObject.transform.position += moveDir * moveSpeed * Time.deltaTime;
                }

                float snap = _snapDistances[_snapDistIndex];
                if (snap > 0) { Vector3 p = _selectedObject.transform.position; p.x = Mathf.Round(p.x / snap) * snap; p.y = Mathf.Round(p.y / snap) * snap; p.z = Mathf.Round(p.z / snap) * snap; _selectedObject.transform.position = p; }
                
                if (_groundSnap && (moveDir.x != 0 || moveDir.z != 0) && moveDir.y == 0)
                {
                    if (Physics.Raycast(_selectedObject.transform.position + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, _selectionMask))
                    {
                        Vector3 p = _selectedObject.transform.position;
                        p.y = hit.point.y;
                        _selectedObject.transform.position = p;
                    }
                }
                changed = true;
            }
            float rotInput = 0;
            if (Input.GetKey(KeyCode.Q)) rotInput = 1f;
            if (Input.GetKey(KeyCode.E)) rotInput = -1f;
            if (rotInput != 0)
            {
                float rotAmount = rotInput * rotateSpeed * Time.deltaTime;
                _selectedObject.transform.Rotate(Vector3.up, rotAmount, IsLocalSpace ? Space.Self : Space.World);
                float snapAngle = _snapAngles[_snapAngleIndex];
                if (snapAngle > 0) { Vector3 rot = _selectedObject.transform.eulerAngles; rot.y = Mathf.Round(rot.y / snapAngle) * snapAngle; _selectedObject.transform.eulerAngles = rot; }
                changed = true;
            }
            if (changed) WorldPatchManager.Instance.RegisterOrUpdatePatch(_selectedObject);
        }

        private bool AnyTransformKeyHeld() => Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.PageUp) || Input.GetKey(KeyCode.PageDown) || Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.F);

        private void SpawnObject(SpawnableItem item)
        {
            if (Camera.main == null) return;
            Vector3 camPos = Camera.main.transform.position;
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0; camForward.Normalize();
            Vector3 spawnPos = camPos + (camForward * 3f);
            if (Physics.Raycast(spawnPos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f, _selectionMask)) spawnPos.y = hit.point.y;
            else spawnPos.y = camPos.y;

            GameObject newObj = null;
            Vector3 targetScale = Vector3.one;
            string sourceID = item.ID;
            
            if (item.Type == SpawnableItem.ItemType.Template)
            {
                targetScale = item.Template.Scale; sourceID = item.Template.SourceItemID;
                if (sourceID.StartsWith("VEH_")) { string code = sourceID.Substring(4); var veh = ScheduleOne.Vehicles.VehicleManager.Instance?.GetVehiclePrefab(code); if (veh != null) newObj = Instantiate(veh.gameObject, spawnPos, Quaternion.identity); }
                else if (sourceID.StartsWith("TREE_")) { string treeName = sourceID.Substring(5); if (Terrain.activeTerrain != null) { foreach (var proto in Terrain.activeTerrain.terrainData.treePrototypes) { if (proto.prefab.name == treeName) { newObj = Instantiate(proto.prefab, spawnPos, Quaternion.identity); break; } } } }
                else { var def = Registry.GetItem<ScheduleOne.ItemFramework.BuildableItemDefinition>(sourceID); if (def != null && def.BuiltItem != null) newObj = Instantiate(def.BuiltItem.transform.gameObject, spawnPos, Quaternion.identity); }
            }
            else
            {
                if (item.Type == SpawnableItem.ItemType.Buildable && item.BuildableDef?.BuiltItem != null) newObj = Instantiate(item.BuildableDef.BuiltItem.transform.gameObject, spawnPos, Quaternion.identity);
                else if (item.Type == SpawnableItem.ItemType.Vehicle && item.VehiclePrefab != null) newObj = Instantiate(item.VehiclePrefab.gameObject, spawnPos, Quaternion.identity);
                else if (item.Type == SpawnableItem.ItemType.Tree && item.TreePrefab != null) newObj = Instantiate(item.TreePrefab, spawnPos, Quaternion.identity);
                else if (item.Type == SpawnableItem.ItemType.Generic && item.GenericPrefab != null) newObj = Instantiate(item.GenericPrefab, spawnPos, Quaternion.identity);
            }
            if (newObj != null) 
            { 
                newObj.name = item.Name + "_" + System.Guid.NewGuid().ToString().Substring(0, 8); 
                newObj.transform.localScale = targetScale; 
                ForceVisibleRecursively(newObj, true);
                GameObject container = GameObject.Find("WorldEditor_SpawnedObjects") ?? new GameObject("WorldEditor_SpawnedObjects"); 
                newObj.transform.SetParent(container.transform); 
                SelectObject(newObj); 
                WorldPatchManager.Instance.RegisterOrUpdatePatch(newObj, sourceID); 
                MelonLogger.Msg($"Spawned: {newObj.name}"); 
            }
        }

        private void ForceVisibleRecursively(GameObject obj, bool isRoot = false)
        {
            if (obj == null || obj.transform.position.y < -4000) return;
            
            // 1. STRICT PROXY BLOCK: Never enable optimization proxies
            if (IsOptimizationProxy(obj))
            {
                obj.SetActive(false);
                return;
            }

            string name = obj.name.ToLower();
            // Hide specific "noise" objects that ruin building visuals
            if (name.Contains("footprint") || name.Contains("grid") || name.Contains("indicator") || name.Contains("prompt") || name.Contains("arrow") || name.Contains("projector") || name.Contains("collider")) 
            { 
                // We keep colliders for selection but hide their visuals if they have any
                var r = obj.GetComponent<Renderer>();
                if (r) r.enabled = false;
                return; 
            }
            
            NeutralizeObject(obj);

            // 2. SMART ACTIVATION: 
            // If it's the root, we must show it. 
            // If it's a child, we only enable it if it's already active OR it's a high-detail mesh.
            if (isRoot) obj.SetActive(true);
            else if (IsLowDetailLOD(obj)) obj.SetActive(false); // Extra check for variant children

            LODGroup lod = obj.GetComponent<LODGroup>();
            if (lod != null) 
            { 
                lod.enabled = true;
                lod.ForceLOD(0); // Force High Detail during edit
            }
            
            var renderers = obj.GetComponents<Renderer>();
            foreach (var r in renderers) r.enabled = true;
            
            // Recurse to children
            foreach (Transform child in obj.transform) ForceVisibleRecursively(child.gameObject, false);
        }

        private void RestoreVisualsRecursively(GameObject obj)
        {
            if (obj == null || obj.transform.position.y < -4000) return;
            
            // Re-enable GameObjects that might have been disabled as LODs/Proxies
            // (Hand control back to the game's LOD system)
            obj.SetActive(true);

            LODGroup lod = obj.GetComponent<LODGroup>();
            if (lod != null)
            {
                lod.enabled = true;
                lod.ForceLOD(-1); // RESET: Hand control back to the game engine
            }

            foreach (Transform child in obj.transform) RestoreVisualsRecursively(child.gameObject);
        }

        private void DeepDelete(GameObject obj)
        {
            if (obj == null) return;
            RecordState();
            obj.SetActive(false);
            obj.transform.SetParent(null); 
            var renderers = obj.GetComponentsInChildren<Renderer>(true);
            foreach (var r in renderers) r.enabled = false;
            var lods = obj.GetComponentsInChildren<LODGroup>(true);
            foreach (var l in lods) l.enabled = false;
            obj.transform.position = new Vector3(UnityEngine.Random.Range(-1000, 1000), -5000, UnityEngine.Random.Range(-1000, 1000)); 
            obj.transform.position = new Vector3(UnityEngine.Random.Range(-1000, 1000), -5000, UnityEngine.Random.Range(-1000, 1000)); 
            obj.transform.localScale = Vector3.zero;
            DisableRecursively(obj);
            WorldPatchManager.Instance.RegisterOrUpdatePatch(obj);
            if (_selectedObject == obj) ClearSelection();
            MelonLogger.Msg($"Deep Deleted & Unparented: {obj.name}");
        }

        private void NeutralizeObject(GameObject obj)
        {
            var behaviours = obj.GetComponents<MonoBehaviour>();
            foreach (var b in behaviours)
            {
                if (b == null) continue;
                string ns = b.GetType().Namespace;
                if (!string.IsNullOrEmpty(ns) && (ns.StartsWith("ScheduleOne") || ns.StartsWith("FishNet"))) b.enabled = false;
            }
        }

        private void ReplaceWithClone(GameObject original)
        {
            if (original == null) return;
            string cleanName = original.name.Replace("(Clone)", "").Trim();
            cleanName = System.Text.RegularExpressions.Regex.Replace(cleanName, @"\s*\(\d+\)$", "");
            SpawnableItem match = _spawnables.Find(s => s.Name.ToLower().Contains(cleanName.ToLower()));
            if (match == null)
            {
                MelonLogger.Msg($"Attempting direct clone of {original.name}...");
                GameObject clone = Instantiate(original, original.transform.position, original.transform.rotation);
                clone.name = original.name + "_Editable";
                UnlockStaticRecursively(clone);
                DisableRecursively(original);
                original.transform.position = new Vector3(0, -9999, 0); 
                SelectObject(clone);
                WorldPatchManager.Instance.RegisterOrUpdatePatch(original);
                WorldPatchManager.Instance.RegisterOrUpdatePatch(clone);
                return;
            }
            SpawnObject(match);
            if (_selectedObject != null)
            {
                _selectedObject.transform.position = original.transform.position;
                _selectedObject.transform.rotation = original.transform.rotation;
                _selectedObject.transform.localScale = original.transform.localScale;
                DisableRecursively(original);
                original.transform.position = new Vector3(0, -9999, 0);
                WorldPatchManager.Instance.RegisterOrUpdatePatch(original);
                WorldPatchManager.Instance.RegisterOrUpdatePatch(_selectedObject);
            }
        }

        private void DisableRecursively(GameObject obj)
        {
            if (obj == null) return;
            obj.SetActive(false);
            foreach (Transform child in obj.transform) DisableRecursively(child.gameObject);
        }

        private void OnGUI()
        {
            if (!IsEditorActive) return;
            GUI.color = _isMenuFocused ? Color.cyan : Color.gray;
            if (!_isMenuFocused) GUI.Box(new Rect(Screen.width / 2 - 150, 20, 300, 30), "<b>GAME MODE</b> (Press F2 for Menu)");
            else { DrawInspector(); DrawPalette(); DrawMapMenu(); }
        }

        private void DrawInspector()
        {
            GUILayout.BeginArea(_inspectorRect, GUI.skin.box);
            GUILayout.Label("<b>WORLD EDITOR</b>", new GUIStyle { alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.cyan } });
            GUILayout.Label($"<size=11>Mode: {CurrentSelectionMode} (F4 to Cycle)</size>", new GUIStyle { alignment = TextAnchor.MiddleCenter });
            GUILayout.Space(5);
            if (GUILayout.Button($"Maps (Ctrl+M): {(_showMapMenu ? "ON" : "OFF")}")) _showMapMenu = !_showMapMenu;
            if (GUILayout.Button($"Object Palette (P): {(_showPalette ? "ON" : "OFF")}")) _showPalette = !_showPalette;
            
            bool gizmosExist = GizmoController.Instance != null && GizmoController.Instance.gameObject;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Gizmos: {(_showGizmos ? "ON" : "OFF")}")) 
            { 
                _showGizmos = !_showGizmos; 
                if (gizmosExist) GizmoController.Instance.SetTarget(_showGizmos ? _selectedObject : null); 
            }
            if (_showGizmos && gizmosExist)
            {
                if (GUILayout.Button("Move")) GizmoController.Instance.SetMode(GizmoController.GizmoMode.Translate);
                if (GUILayout.Button("Rotate")) GizmoController.Instance.SetMode(GizmoController.GizmoMode.Rotate);
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            GUILayout.Label("<b>Settings</b>");
            string snapDistLabel = _snapDistances[_snapDistIndex] == 0 ? "OFF" : _snapDistances[_snapDistIndex].ToString("F2") + "m";
            if (GUILayout.Button($"Grid Snap: {snapDistLabel}")) _snapDistIndex = (_snapDistIndex + 1) % _snapDistances.Length;
            string snapAngleLabel = _snapAngles[_snapAngleIndex] == 0 ? "OFF" : _snapAngles[_snapAngleIndex].ToString() + "°";
            if (GUILayout.Button($"Angle Snap: {snapAngleLabel}")) _snapAngleIndex = (_snapAngleIndex + 1) % _snapAngles.Length;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button($"Ground Snap: {(_groundSnap ? "ON" : "OFF")}")) _groundSnap = !_groundSnap;
            if (GUILayout.Button($"Auto-Root: {(_autoRoot ? "ON" : "OFF")}")) _autoRoot = !_autoRoot;
            GUILayout.EndHorizontal();
            if (GUILayout.Button($"Occlusion Fix: {(_occlusionFixEnabled ? "ON" : "OFF")}")) _occlusionFixEnabled = !_occlusionFixEnabled;
            if (GUILayout.Button($"Space: {(IsLocalSpace ? "LOCAL" : "GLOBAL")}")) IsLocalSpace = !IsLocalSpace;
            if (GUILayout.Button("Undo (Ctrl+Z)")) Undo();
            if (GUILayout.Button("Refresh World Visuals (LOD Fix)")) RefreshAllWorldVisuals();
            
            if (_selectedObject != null && _selectedObject.transform != null)
            {
                GUILayout.Space(10);
                GUILayout.Label("<color=yellow><b>Object Inspector</b></color>");
                GUILayout.Label($"Name: {_selectedObject.name}");
                GUILayout.Label($"Children: {_selectedObject.transform.childCount}");
                string componentsStr = "";
                try { var allComponents = _selectedObject.GetComponents<Component>(); if (allComponents != null) foreach(var c in allComponents) if (c != null) componentsStr += c.GetType().Name + " "; } catch { componentsStr = "Error reading components"; }
                GUILayout.Label($"<size=10>{componentsStr}</size>");
                GUILayout.Space(5);
                GUILayout.Label("<b>Hierarchy</b>");
                GUILayout.BeginHorizontal();
                if (_selectedObject.transform.parent != null && !(_selectedObject.transform.parent.name == "Map" || _selectedObject.transform.parent.name.StartsWith("@") || _selectedObject.transform.parent.name == "WorldEditor_SpawnedObjects")) if (GUILayout.Button("Select Parent")) SelectObject(_selectedObject.transform.parent.gameObject);
                if (GUILayout.Button("Select Root (F3)")) SelectObject(GetSafeRoot(_selectedObject));
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                if (GUILayout.Button("Replace with Clone (Fix Static)")) ReplaceWithClone(_selectedObject);
                if (GUILayout.Button("Duplicate (Ctrl+C, Ctrl+V)")) { Copy(_selectedObject); Paste(); }
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<color=red>Delete Object</color>")) DeepDelete(_selectedObject);
                if (GUILayout.Button("<color=orange>Destroy Whole Building</color>")) DeepDelete(GetSafeRoot(_selectedObject));
                GUILayout.EndHorizontal();
                if (_selectedObject != null)
                {
                    GUILayout.Label("Position:"); GUILayout.BeginHorizontal(); DrawCoord("X", _selectedObject.transform.position.x); DrawCoord("Y", _selectedObject.transform.position.y); DrawCoord("Z", _selectedObject.transform.position.z); GUILayout.EndHorizontal();
                    GUILayout.Label("Rotation:"); GUILayout.BeginHorizontal(); DrawCoord("Y", _selectedObject.transform.eulerAngles.y); GUILayout.EndHorizontal();
                    if (GUILayout.Button("Reset Rotation")) { RecordState(); _selectedObject.transform.eulerAngles = Vector3.zero; WorldPatchManager.Instance.RegisterOrUpdatePatch(_selectedObject); }
                }
            }
            else { GUILayout.Space(20); GUILayout.Label("<i>Click an object to select</i>", new GUIStyle { alignment = TextAnchor.MiddleCenter }); }
            GUILayout.FlexibleSpace();
            GUILayout.Label("<size=10><b>CAMERA:</b> WASD + Space/LCtrl | Right-Click: Look</size>");
            GUILayout.Label("<size=10><b>OBJECT:</b> Arrows: Move | R/F: Height | Q/E: Rotate</size>");
            GUILayout.EndArea();
        }

        private void DrawPalette()
        {
            if (!_showPalette) return;
            GUI.color = Color.white;
            float width = 320;
            float height = Screen.height - 40;
            GUILayout.BeginArea(new Rect(Screen.width - width - 20, 20, width, height), GUI.skin.box);
            GUILayout.Label("<b>OBJECT PALETTE</b>", new GUIStyle { alignment = TextAnchor.MiddleCenter });
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<", GUILayout.Width(30))) { _selectedCategoryIndex = (_selectedCategoryIndex - 1 + _paletteCategories.Length) % _paletteCategories.Length; }
            GUILayout.Label($"<b>{_paletteCategories[_selectedCategoryIndex]}</b>", new GUIStyle { alignment = TextAnchor.MiddleCenter });
            if (GUILayout.Button(">", GUILayout.Width(30))) { _selectedCategoryIndex = (_selectedCategoryIndex + 1) % _paletteCategories.Length; }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(); GUILayout.Label("Search:", GUILayout.Width(60)); _paletteSearch = GUILayout.TextField(_paletteSearch); GUILayout.EndHorizontal();
            bool searchChanged = _paletteSearch != _lastSearch;
            bool catChanged = _selectedCategoryIndex != _lastCategory;
            if (searchChanged || catChanged)
            {
                _lastSearch = _paletteSearch; _lastCategory = _selectedCategoryIndex; _paletteScroll = Vector2.zero;
                string selectedCat = _paletteCategories[_selectedCategoryIndex]; _filteredSpawnables.Clear();
                foreach (var item in _spawnables) { if (selectedCat != "All" && item.Category != selectedCat) continue; if (!string.IsNullOrEmpty(_paletteSearch) && !item.Name.ToLower().Contains(_paletteSearch.ToLower())) continue; _filteredSpawnables.Add(item); }
            }
            GUILayout.Label($"Count: {_filteredSpawnables.Count}");
            float itemHeight = 25; float viewHeight = height - 150; 
            _paletteScroll = GUILayout.BeginScrollView(_paletteScroll, GUILayout.Height(viewHeight));
            int totalCount = _filteredSpawnables.Count;
            float totalContentHeight = totalCount * itemHeight;
            GUILayout.Space(totalContentHeight);
            int firstVisibleIdx = Mathf.FloorToInt(_paletteScroll.y / itemHeight);
            int lastVisibleIdx = firstVisibleIdx + Mathf.CeilToInt(viewHeight / itemHeight) + 1;
            firstVisibleIdx = Mathf.Clamp(firstVisibleIdx, 0, totalCount);
            lastVisibleIdx = Mathf.Clamp(lastVisibleIdx, 0, totalCount);
            for (int i = firstVisibleIdx; i < lastVisibleIdx; i++) { var item = _filteredSpawnables[i]; Rect buttonRect = new Rect(5, i * itemHeight, width - 30, itemHeight - 2); if (GUI.Button(buttonRect, item.Name)) SpawnObject(item); }
            GUILayout.EndScrollView();
            if (GUILayout.Button("Close Palette")) _showPalette = false;
            GUILayout.EndArea();
        }

        private void DrawMapMenu()
        {
            if (!_showMapMenu) return;
            GUI.color = Color.white;
            GUILayout.BeginArea(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 250, 400, 500), GUI.skin.box);
            GUILayout.Label("<b>MAP MANAGER</b>", new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 16 });
            GUILayout.Space(10);
            GUILayout.Label($"Current Map: <b>{(_currentMapName == "" ? "OFFICIAL (Unmodified)" : _currentMapName)}</b>");
            if (GUILayout.Button("Load Official Map (Unload Custom)")) { WorldPatchManager.Instance.UnloadMap(); _currentMapName = ""; }
            GUILayout.Space(10); GUILayout.Label("<b>Save Map</b>");
            GUILayout.BeginHorizontal(); _mapSaveName = GUILayout.TextField(_mapSaveName); if (GUILayout.Button("Save")) if (!string.IsNullOrEmpty(_mapSaveName)) { WorldPatchManager.Instance.SaveMap(_mapSaveName); _currentMapName = _mapSaveName; } GUILayout.EndHorizontal();
            GUILayout.Space(10); GUILayout.Label("<b>Load Map</b>");
            var maps = WorldPatchManager.Instance.GetAvailableMaps();
            if (maps.Length == 0) GUILayout.Label("No saved maps found.");
            else foreach (var map in maps) if (GUILayout.Button(map)) { WorldPatchManager.Instance.LoadMap(map); _currentMapName = map; _mapSaveName = map; }
            GUILayout.FlexibleSpace(); if (GUILayout.Button("Close")) _showMapMenu = false;
            GUILayout.EndArea();
        }

        private void DrawCoord(string label, float val)
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(label, new GUIStyle { alignment = TextAnchor.MiddleCenter, fontSize = 10 });
            GUILayout.Label(val.ToString("F2"), new GUIStyle { alignment = TextAnchor.MiddleCenter });
            GUILayout.EndVertical();
        }
    }
}