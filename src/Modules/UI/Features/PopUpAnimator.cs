using UnityEngine;
using UnityEngine.EventSystems;

namespace WindyFramework.Modules.UI.Features
{
    public class PopUpAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public float Duration = 0.5f;
        public float Delay = 0.1f;

        // Hover Settings (Match official style)
        public float HoverScaleMult = 1.1f;
        public float HoverSpeed = 10f;

        private RectTransform rect;
        private CanvasGroup cg;
        private float time;
        private bool entranceDone = false;
        private bool isHovered = false;
        private float currentHoverLerp = 0f;

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
            cg = GetComponent<CanvasGroup>();
        }

        private void OnEnable()
        {
            time = 0;
            entranceDone = false;
            currentHoverLerp = 0f;
            if (rect != null) rect.localScale = Vector3.zero;
            if (cg != null) cg.alpha = 0f;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
        }

        private void Update()
        {
            // 1. Entrance Animation (Scale / Alpha)
            if (!entranceDone)
            {
                time += Time.unscaledDeltaTime;
                if (time < Delay) return;

                float t = (time - Delay) / Duration;

                if (t >= 1f)
                {
                    entranceDone = true;
                    if (cg != null) cg.alpha = 1f;
                }
                else
                {
                    float c4 = (2f * Mathf.PI) / 3f;
                    float ease = (t == 0f) ? 0f : (t == 1f) ? 1f : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;

                    if (rect != null) rect.localScale = Vector3.one * ease;
                    if (cg != null) cg.alpha = t;
                    return; // Don't process hover scale until entrance is done
                }
            }

            // 2. Hover Animation (Scale)
            if (rect != null)
            {
                float targetScale = isHovered ? HoverScaleMult : 1f;
                currentHoverLerp = Mathf.Lerp(currentHoverLerp, targetScale, Time.unscaledDeltaTime * HoverSpeed);
                rect.localScale = Vector3.one * currentHoverLerp;
            }
        }
    }
}
