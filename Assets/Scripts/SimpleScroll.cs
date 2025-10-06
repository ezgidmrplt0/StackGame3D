using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SimpleScroll : MonoBehaviour
{
    public Scrollbar scrollbar;          // Scrollbar'ı Inspector'dan sürükle
    public RectTransform content;        // Kaydırmak istediğin panel

    private float startY;

    [Header("Scroll Aralıkları")]
    public float upRange = 0f;           // Yukarıya doğru izin verilen mesafe
    public float downRange = 100f;       // Aşağıya doğru izin verilen mesafe

    void Start()
    {
        startY = content.anchoredPosition.y;
        scrollbar.onValueChanged.AddListener(OnScrollChanged);
    }

    public void OnScrollChanged(float value)
    {
        // value = 0 en üst, 1 en alt
        // Yukarı için negatif gitmesin diye 0-1 aralığını map ediyoruz
        float offset = Mathf.Lerp(-upRange, downRange, value);
        content.anchoredPosition = new Vector2(
            content.anchoredPosition.x,
            startY + offset
        );
    }
}
