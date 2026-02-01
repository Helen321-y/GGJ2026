using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : MonoBehaviour
{
public static ScreenFade Instance { get; private set; }

    [SerializeField] private Image image;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (image == null)
            image = GetComponent<Image>();

        // 初始透明
        SetAlpha(0f);
    }

    public IEnumerator FadeOut(float duration)
    {
        yield return FadeTo(1f, duration);
    }

    public IEnumerator FadeIn(float duration)
    {
        yield return FadeTo(0f, duration);
    }

    private IEnumerator FadeTo(float target, float duration)
    {
        float start = image.color.a;
        float t = 0f;

        Color c = image.color;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = duration <= 0f ? 1f : t / duration;
            c.a = Mathf.Lerp(start, target, k);
            image.color = c;
            yield return null;
        }

        c.a = target;
        image.color = c;
    }

    private void SetAlpha(float a)
    {
        Color c = image.color;
        c.a = a;
        image.color = c;
    }
}
