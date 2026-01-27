using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIHoverFade : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public CanvasGroup target;
    public float fadeIn = 0.15f;
    public float fadeOut = 0.25f;

    Coroutine routine;

    public CanvasGroup group;

    void Start()
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        FadeTo(1f, fadeIn);
    }

    public void OnPointerExit(PointerEventData e)
    {
        FadeTo(0f, fadeOut);
    }

    void FadeTo(float targetAlpha, float time)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(Fade(targetAlpha, time));
    }

    IEnumerator Fade(float targetAlpha, float time)
    {
        float start = target.alpha;
        float t = 0f;

        target.interactable = true;
        target.blocksRaycasts = true;

        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            target.alpha = Mathf.Lerp(start, targetAlpha, t / time);
            yield return null;
        }

        target.alpha = targetAlpha;
    }
}
