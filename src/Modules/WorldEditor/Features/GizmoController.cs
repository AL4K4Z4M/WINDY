using UnityEngine;
using System.Collections.Generic;

namespace Zordon.ScheduleI.WorldEditor.Features
{
    public class GizmoController : MonoBehaviour
    {
        private static GizmoController _instance;
        public static GizmoController Instance
        {
            get
            {
                if (_instance == null || !_instance.gameObject)
                {
                    var go = new GameObject("WorldEditorGizmoController");
                    Object.DontDestroyOnLoad(go);
                    _instance = go.AddComponent<GizmoController>();
                }
                return _instance;
            }
        }

        public enum GizmoMode { Translate, Rotate, Scale }
        public GizmoMode CurrentMode = GizmoMode.Translate;

        private GameObject _target;
        private GameObject _gizmoRoot;
        private GameObject _translateRoot;
        private GameObject _rotateRoot;
        private List<GizmoHandle> _handles = new List<GizmoHandle>();

        public bool IsAnyHandleDragging
        {
            get
            {
                foreach (var h in _handles) if (h != null && (h.IsDragging || h.IsHovered)) return true;
                return false;
            }
        }

        private void Start()
        {
            if (_gizmoRoot == null) CreateGizmos();
            _gizmoRoot.SetActive(false);
        }

        private void CreateGizmos()
        {
            if (_gizmoRoot != null) return;
            _gizmoRoot = new GameObject("WorldEditor_GizmoRoot");
            _gizmoRoot.transform.SetParent(transform);

            _translateRoot = new GameObject("TranslateHandles");
            _translateRoot.transform.SetParent(_gizmoRoot.transform);
            
            _rotateRoot = new GameObject("RotateHandles");
            _rotateRoot.transform.SetParent(_gizmoRoot.transform);

            // 1. Translation Arrows
            _handles.Add(CreateArrow(GizmoHandle.Axis.X, Color.red, Quaternion.Euler(0, 0, -90), _translateRoot.transform));
            _handles.Add(CreateArrow(GizmoHandle.Axis.Y, Color.green, Quaternion.identity, _translateRoot.transform));
            _handles.Add(CreateArrow(GizmoHandle.Axis.Z, Color.blue, Quaternion.Euler(90, 0, 0), _translateRoot.transform));

            // 2. Rotation Rings (The 'Atom' look)
            _handles.Add(CreateRing(GizmoHandle.Axis.X, Color.red, Quaternion.Euler(0, 0, 90), _rotateRoot.transform));
            _handles.Add(CreateRing(GizmoHandle.Axis.Y, Color.green, Quaternion.identity, _rotateRoot.transform));
            _handles.Add(CreateRing(GizmoHandle.Axis.Z, Color.blue, Quaternion.Euler(90, 0, 0), _rotateRoot.transform));
            
            RefreshMode();
        }

        private GizmoHandle CreateArrow(GizmoHandle.Axis axis, Color color, Quaternion rotation, Transform parent)
        {
            GameObject handleObj = new GameObject($"Arrow_{axis}");
            handleObj.transform.SetParent(parent);
            handleObj.transform.localRotation = rotation;

            // Shaft
            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.transform.SetParent(handleObj.transform);
            cyl.transform.localPosition = new Vector3(0, 0.4f, 0);
            cyl.transform.localScale = new Vector3(0.03f, 0.4f, 0.03f);
            AssignGizmoMaterial(cyl, color);
            Destroy(cyl.GetComponent<Collider>());

            // Tip
            GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.transform.SetParent(handleObj.transform);
            tip.transform.localPosition = new Vector3(0, 0.9f, 0);
            tip.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            AssignGizmoMaterial(tip, color);

            var handle = handleObj.AddComponent<GizmoHandle>();
            handle.axis = axis;
            handle.type = GizmoHandle.HandleType.Translate;
            handle.normalColor = color;
            handle.OnHandleDragged += (a, d) => HandleTranslate(a, d);
            return handle;
        }

        private GizmoHandle CreateRing(GizmoHandle.Axis axis, Color color, Quaternion rotation, Transform parent)
        {
            GameObject handleObj = new GameObject($"Ring_{axis}");
            handleObj.transform.SetParent(parent);
            handleObj.transform.localRotation = rotation;

            // Create a ring out of small segments (no custom mesh generation to keep it simple/portable)
            float radius = 0.8f;
            int segments = 24;
            for (int i = 0; i < segments; i++)
            {
                float angle = (i * 2 * Mathf.PI) / segments;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                
                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.transform.SetParent(handleObj.transform);
                seg.transform.localPosition = pos;
                seg.transform.localRotation = Quaternion.Euler(0, -i * (360f / segments), 0);
                seg.transform.localScale = new Vector3(0.15f, 0.02f, 0.02f);
                AssignGizmoMaterial(seg, color);
                
                // Only keep one collider for the whole ring to prevent weirdness, 
                // or just let them all be triggers.
                var col = seg.GetComponent<BoxCollider>();
                if (col != null) col.isTrigger = true;
            }

            var handle = handleObj.AddComponent<GizmoHandle>();
            handle.axis = axis;
            handle.type = GizmoHandle.HandleType.Rotate;
            handle.normalColor = color;
            handle.OnHandleDragged += (a, d) => HandleRotate(a, d);
            return handle;
        }

        private void RefreshMode()
        {
            if (_translateRoot != null) _translateRoot.SetActive(CurrentMode == GizmoMode.Translate);
            if (_rotateRoot != null) _rotateRoot.SetActive(CurrentMode == GizmoMode.Rotate);
        }

        public void SetMode(GizmoMode mode)
        {
            CurrentMode = mode;
            RefreshMode();
        }

        private void AssignGizmoMaterial(GameObject obj, Color color)
        {
            var rend = obj.GetComponent<MeshRenderer>();
            if (rend == null) return;
            // Use unlit shader for maximum visibility
            var shader = Shader.Find("UI/Default") ?? Shader.Find("Legacy Shaders/Transparent/Diffuse");
            rend.material = new Material(shader);
            rend.material.color = color;
            rend.material.renderQueue = 4000; // Render on top
        }

        public void SetTarget(GameObject target)
        {
            _target = target;
            if (_gizmoRoot == null) CreateGizmos();
            if (_gizmoRoot != null) _gizmoRoot.SetActive(_target != null);
            if (_target != null) UpdatePosition();
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                if (_gizmoRoot != null && _gizmoRoot.activeSelf) _gizmoRoot.SetActive(false);
                return;
            }

            UpdatePosition();
            UpdateScale();
        }

        private void UpdatePosition()
        {
            if (_target == null) return;
            _gizmoRoot.transform.position = _target.transform.position;
            
            // Respect Local vs Global space from EditorController
            bool isLocal = EditorController.Instance != null && EditorController.Instance.IsLocalSpace;
            _gizmoRoot.transform.rotation = isLocal ? _target.transform.rotation : Quaternion.identity;
        }

        private void UpdateScale()
        {
            if (Camera.main == null) return;
            float dist = Vector3.Distance(Camera.main.transform.position, _gizmoRoot.transform.position);
            _gizmoRoot.transform.localScale = Vector3.one * (dist * 0.15f);
        }

        private void HandleTranslate(GizmoHandle.Axis axis, float delta)
        {
            if (_target == null) return;
            Vector3 dir = Vector3.zero;
            bool isLocal = EditorController.Instance != null && EditorController.Instance.IsLocalSpace;
            
            switch (axis)
            {
                case GizmoHandle.Axis.X: dir = isLocal ? _target.transform.right : Vector3.right; break;
                case GizmoHandle.Axis.Y: dir = isLocal ? _target.transform.up : Vector3.up; break;
                case GizmoHandle.Axis.Z: dir = isLocal ? _target.transform.forward : Vector3.forward; break;
            }

            _target.transform.position += dir * delta;
            WorldPatchManager.Instance.RegisterOrUpdatePatch(_target);
        }

        private void HandleRotate(GizmoHandle.Axis axis, float delta)
        {
            if (_target == null) return;
            Vector3 axisVec = Vector3.zero;
            bool isLocal = EditorController.Instance != null && EditorController.Instance.IsLocalSpace;

            switch (axis)
            {
                case GizmoHandle.Axis.X: axisVec = isLocal ? _target.transform.right : Vector3.right; break;
                case GizmoHandle.Axis.Y: axisVec = isLocal ? _target.transform.up : Vector3.up; break;
                case GizmoHandle.Axis.Z: axisVec = isLocal ? _target.transform.forward : Vector3.forward; break;
            }

            // Convert linear mouse delta to degrees (arbitrary scaling for feel)
            float degrees = delta * 150f; 
            _target.transform.Rotate(axisVec, degrees, Space.World);
            WorldPatchManager.Instance.RegisterOrUpdatePatch(_target);
        }
    }
}