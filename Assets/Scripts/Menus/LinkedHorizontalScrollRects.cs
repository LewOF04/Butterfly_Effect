using UnityEngine;
using UnityEngine.UI;

public class LinkedHorizontalScrollRects : MonoBehaviour
{
    public ScrollRect scrollA;
    public ScrollRect scrollB;
    public Scrollbar scrollbar;

    private bool syncing;

    private void Awake()
    {
        scrollbar.onValueChanged.AddListener(OnScrollbarChanged);
        scrollA.onValueChanged.AddListener(OnScrollAChanged);
        scrollB.onValueChanged.AddListener(OnScrollBChanged);
    }

    private void OnDestroy()
    {
        scrollbar.onValueChanged.RemoveListener(OnScrollbarChanged);
        scrollA.onValueChanged.RemoveListener(OnScrollAChanged);
        scrollB.onValueChanged.RemoveListener(OnScrollBChanged);
    }

    private void OnScrollbarChanged(float value)
    {
        if (syncing) return;

        syncing = true;
        scrollA.horizontalNormalizedPosition = value;
        scrollB.horizontalNormalizedPosition = value;
        syncing = false;
    }

    private void OnScrollAChanged(Vector2 _)
    {
        if (syncing) return;

        syncing = true;
        float value = scrollA.horizontalNormalizedPosition;
        scrollB.horizontalNormalizedPosition = value;
        scrollbar.value = value;
        syncing = false;
    }

    private void OnScrollBChanged(Vector2 _)
    {
        if (syncing) return;

        syncing = true;
        float value = scrollB.horizontalNormalizedPosition;
        scrollA.horizontalNormalizedPosition = value;
        scrollbar.value = value;
        syncing = false;
    }
}