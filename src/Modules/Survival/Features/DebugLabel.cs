using UnityEngine;
using ScheduleOne.Interaction;
using ScheduleOne.NPCs;

namespace Zordon.ScheduleI.Survival.Features
{
    public class DebugLabel : MonoBehaviour
    {
        private WorldSpaceLabel _label;
        private NPC _npc;
        private LineRenderer _beam;
        private string _text;
        private Color _color;
        private float _spawnTime;
        public static bool ShowBeams = false;
        public static bool ShowLabels = false;

        public void Setup(NPC npc, string text, Color color)
        {
            _npc = npc;
            _text = text;
            _color = color;
            _spawnTime = Time.time;

            // Setup Beam
            var beamGO = new GameObject("DebugBeam");
            beamGO.transform.SetParent(transform);
            _beam = beamGO.AddComponent<LineRenderer>();
            _beam.material = new Material(Shader.Find("Sprites/Default"));
            _beam.startColor = Color.white;
            _beam.endColor = new Color(1, 1, 1, 0); // Fade out at top
            _beam.startWidth = 0.5f;
            _beam.endWidth = 0.5f;
            _beam.positionCount = 2;
            _beam.useWorldSpace = true;
            _beam.enabled = false;
        }

        private Vector3 GetLabelPosition()
        {
            if (_npc == null) return Vector3.zero;
            return _npc.transform.position + Vector3.up * 2.2f;
        }

        private void Update()
        {
            if (_npc == null || _npc.Health.IsDead || !_npc.gameObject.activeInHierarchy)
            {
                Cleanup();
                Destroy(this); // Remove this component
                return;
            }

            // Distance Safety Check (Prevent ghosts)
            if (SurvivalController.Instance.SurvivalEnabled)
            {
                var nearestPlayer = SurvivalController.Instance.GetNearestPlayer(transform.position);
                if (nearestPlayer != null)
                {
                    float dist = Vector3.Distance(transform.position, nearestPlayer.transform.position);
                    if (dist > 200f) // If an enemy is lost in the void, kill the marker
                    {
                        Cleanup();
                        Destroy(this);
                        return;
                    }
                }
            }

            // Sync Label Visibility
            if (ShowLabels)
            {
                if (_label == null)
                {
                    _label = new WorldSpaceLabel(_text, GetLabelPosition());
                    _label.color = _color;
                }
                else
                {
                    _label.position = GetLabelPosition();
                }
            }
            else
            {
                if (_label != null)
                {
                    _label.Destroy();
                    _label = null;
                }
            }

            // Update Beam
            if (_beam != null)
            {
                _beam.enabled = ShowBeams;
                if (ShowBeams)
                {
                    _beam.SetPosition(0, _npc.transform.position);
                    _beam.SetPosition(1, _npc.transform.position + Vector3.up * 50f);
                }
            }
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (_label != null)
            {
                _label.Destroy();
                _label = null;
            }
            if (_beam != null)
            {
                Destroy(_beam.gameObject);
                _beam = null;
            }
        }
    }
}