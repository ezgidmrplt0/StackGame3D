using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleScroll : MonoBehaviour
{
    [Header("UI")]
    public Scrollbar scrollbar;           // Inspector'dan bağla
    public RectTransform content;         // Kaydırılacak panel

    private float startY;

    [Header("Sınırlar (px)")]
    [Tooltip("Başlangıçta listeyi aşağı itmek için. Pozitif değer -> aşağı iner")]
    public float initialShiftY = 40f;     // Header'dan kaç px aşağıda başlasın
    [Tooltip("Yukarı gidebileceği maksimum mesafe (pozitif)")]
    public float upRange = 0f;
    [Tooltip("Aşağı gidebileceği maksimum mesafe (pozitif)")]
    public float downRange = 400f;

    void Start()
    {
        // Listeyi header'dan uzak başlat
        startY = content.anchoredPosition.y + initialShiftY;
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, startY);

        if (scrollbar != null)
            scrollbar.onValueChanged.AddListener(OnScrollChanged);
    }

    public void OnScrollChanged(float value)
    {
        // value: 0 -> en üst, 1 -> en alt
        float offset = Mathf.Lerp(-upRange, downRange, value);
        float targetY = startY + offset;

        // Yukarı ve aşağı kesin sınır
        float minY = startY - upRange;     // en yukarı
        float maxY = startY + downRange;   // en aşağı
        targetY = Mathf.Clamp(targetY, minY, maxY);

        content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetY);
    }
}
