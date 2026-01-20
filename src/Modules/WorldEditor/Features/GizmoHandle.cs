using UnityEngine;
using System;

namespace Zordon.ScheduleI.WorldEditor.Features
{
    public class GizmoHandle : MonoBehaviour
    {
        public enum Axis { X, Y, Z }
        public enum HandleType { Translate, Rotate, Scale }
        
        public Axis axis;
        public HandleType type;
        public Color normalColor;
        public Color hoverColor = Color.yellow;
        
        private MeshRenderer _renderer;
        private bool _isHovered;
        private bool _isDragging;
        public bool IsDragging => _isDragging;
        public bool IsHovered => _isHovered;
        private Vector3 _lastMousePos;
        
        public event Action<Axis, float> OnHandleDragged;

        private void Awake()
        {
            _renderer = GetComponentInChildren<MeshRenderer>();
            if (_renderer != null) normalColor = _renderer.material.color;
            
            // Ensure handle has a collider for raycasting
            if (GetComponent<Collider>() == null)
            {
                var col = gameObject.AddComponent<BoxCollider>();
                col.isTrigger = true;
            }
        }

        public void SetHighlight(bool highlight)
        {
            if (_renderer == null) return;
            _renderer.material.color = highlight ? hoverColor : normalColor;
        }

        private void OnMouseEnter()
        {
            _isHovered = true;
            SetHighlight(true);
        }

        private void OnMouseExit()
        {
            _isHovered = false;
            if (!_isDragging) SetHighlight(false);
        }

        private void Update()
        {
            // 1. Raycast for Hover (since standard Unity OnMouseEnter fails when mouse is locked/unlocked frequently)
            UpdateHover();

            if (_isHovered && Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _lastMousePos = Input.mousePosition;
            }

            if (_isDragging)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    _isDragging = false;
                    SetHighlight(_isHovered);
                    return;
                }

                HandleDrag();
            }
        }

        private void UpdateHover()
        {
            if (Camera.main == null) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool hit = false;
            
            // Check all colliders in children (arrows heads)
            foreach (var col in GetComponentsInChildren<Collider>())
            {
                if (col.Raycast(ray, out RaycastHit hitInfo, 100f))
                {
                    hit = true;
                    break;
                }
            }

            if (hit != _isHovered)
            {
                _isHovered = hit;
                SetHighlight(_isHovered || _isDragging);
            }
        }

        private void HandleDrag()
        {
            Vector3 currentMousePos = Input.mousePosition;
            Vector3 diff = currentMousePos - _lastMousePos;
            _lastMousePos = currentMousePos;

            if (Camera.main == null) return;

            Vector3 axisDir = GetAxisVector();
            float moveDelta = 0;

            if (type == HandleType.Translate)
            {
                Vector3 pBase = Camera.main.WorldToScreenPoint(transform.position);
                Vector3 pTip = Camera.main.WorldToScreenPoint(transform.position + axisDir);
                Vector2 screenDir = new Vector2(pTip.x - pBase.x, pTip.y - pBase.y).normalized;
                moveDelta = Vector2.Dot(new Vector2(diff.x, diff.y), screenDir);
                
                float dist = Vector3.Distance(Camera.main.transform.position, transform.position);
                moveDelta *= (dist * 0.002f); 
            }
            else if (type == HandleType.Rotate)
            {
                // Rotation logic: Find the tangent of the circle in screen space at the mouse position
                Vector3 screenCenter = Camera.main.WorldToScreenPoint(transform.parent.position); // Gizmo root
                Vector2 toMouse = new Vector2(currentMousePos.x - screenCenter.x, currentMousePos.y - screenCenter.y).normalized;
                
                // Perpendicular vector to create a 'tangent' screen direction
                Vector2 tangent = new Vector2(-toMouse.y, toMouse.x);
                
                // How much did the mouse move along that tangent?
                moveDelta = Vector2.Dot(new Vector2(diff.x, diff.y), tangent);
                
                // Scale so rotation speed is reasonable
                moveDelta *= 0.01f; 
            }
            
            if (Mathf.Abs(moveDelta) > 0.0001f)
            {
                OnHandleDragged?.Invoke(axis, moveDelta);
            }
        }

        private Vector3 GetAxisVector()
        {
            switch (axis)
            {
                case Axis.X: return transform.right;
                case Axis.Y: return transform.up;
                case Axis.Z: return transform.forward;
                default: return Vector3.up;
            }
        }
    }
}
