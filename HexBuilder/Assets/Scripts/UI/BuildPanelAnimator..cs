using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace HexBuilder.UI
{
    
    public class BuildPanelAnimator : MonoBehaviour
    {
        [Header("Refs")]
        public RectTransform panel;       
        public CanvasGroup canvasGroup;   

        [Header("Anim")]
        [Tooltip("Sekundy na anim·ciu otvorenia/zatvorenia.")]
        public float duration = 0.25f;
        [Tooltip("K¯ivka anim·cie (0..1).")]
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [Tooltip("Extra posun pod spodok (px), nech je panel ˙plne mimo obrazovky.")]
        public float extraHideOffset = 20f;
        [Tooltip("Skryù po ötarte?")]
        public bool startHidden = true;

        Vector2 shownPos;   
        Vector2 hiddenPos; 
        Coroutine anim;

        void Reset()
        {
            panel = GetComponent<RectTransform>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        void Awake()
        {
            if (!panel) panel = GetComponent<RectTransform>();
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();

           
            shownPos = panel.anchoredPosition;

           
            float hideY = shownPos.y - (panel.rect.height + extraHideOffset);
            hiddenPos = new Vector2(shownPos.x, hideY);

            if (startHidden) InstantHide();
            else InstantShow();
        }

        public void Show()
        {
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(Animate(shownPos, 1f, true));
        }

        public void Hide()
        {
            if (anim != null) StopCoroutine(anim);
            anim = StartCoroutine(Animate(hiddenPos, 0f, false));
        }

        void InstantShow()
        {
            panel.anchoredPosition = shownPos;
            if (canvasGroup)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        void InstantHide()
        {
            panel.anchoredPosition = hiddenPos;
            if (canvasGroup)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }

        IEnumerator Animate(Vector2 targetPos, float targetAlpha, bool enableAtEnd)
        {
            
            if (canvasGroup)
            {
                if (enableAtEnd) canvasGroup.blocksRaycasts = true;
                else { canvasGroup.interactable = false; canvasGroup.blocksRaycasts = false; }
            }

            Vector2 startPos = panel.anchoredPosition;
            float startAlpha = canvasGroup ? canvasGroup.alpha : 1f;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;         
                float k = curve.Evaluate(Mathf.Clamp01(t));

                panel.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, k);
                if (canvasGroup)
                    canvasGroup.alpha = Mathf.LerpUnclamped(startAlpha, targetAlpha, k);

                yield return null;
            }

            panel.anchoredPosition = targetPos;
            if (canvasGroup)
            {
                canvasGroup.alpha = targetAlpha;
                canvasGroup.interactable = enableAtEnd;
                canvasGroup.blocksRaycasts = enableAtEnd;
            }
            anim = null;
        }
    }
}
