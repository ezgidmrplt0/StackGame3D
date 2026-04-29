using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Content paneline eklenecek yardımcı sınıf (sürüklemeleri yakalamak için)
public class ContentDragHandler : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public SimpleScroll simpleScroll;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (simpleScroll != null) simpleScroll.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (simpleScroll != null) simpleScroll.OnDrag(eventData);
    }
}

public class SimpleScroll : MonoBehaviour
{
    [Header("UI")]
    public Scrollbar scrollbar;           // Inspector'dan bağla
    public RectTransform content;         // Kaydırılacak panel

    private float startY;
    private float dragStartY;
    private float contentStartY;

    [Header("Sınırlar (px)")]
    [Tooltip("Başlangıçta listeyi aşağı itmek için. Pozitif değer -> aşağı iner")]
    public float initialShiftY = 40f;     // Header'dan kaç px aşağıda başlasın
    [Tooltip("Yukarı gidebileceği maksimum mesafe (pozitif)")]
    public float upRange = 0f;
    [Tooltip("Aşağı gidebileceği maksimum mesafe (pozitif)")]
    public float downRange = 400f;

    void Start()
    {
        // Eğer maskeleme için bir Viewport yoksa otomatik oluştur
        if (content.parent.GetComponent<RectMask2D>() == null && content.parent.GetComponent<Mask>() == null)
        {
            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.SetParent(content.parent, false);

            // Viewport'un boyutunu content'in ilk haline ayarla
            viewportRect.anchorMin = content.anchorMin;
            viewportRect.anchorMax = content.anchorMax;
            viewportRect.pivot = content.pivot;
            viewportRect.anchoredPosition = content.anchoredPosition;
            viewportRect.sizeDelta = content.sizeDelta;

            // Maskenin arkaplanını görünmez yap
            Image img = viewport.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            img.raycastTarget = true; 

            // Content'i viewport içine taşı
            content.SetParent(viewportRect, true);

            // Scrollbar'ı dışarı (görünmez şekilde çalışması için) taşı
            if (scrollbar != null && scrollbar.transform.parent == content)
            {
                scrollbar.transform.SetParent(viewportRect, true);
                scrollbar.transform.SetAsFirstSibling();
                // Scrollbar tıklamaları engellemesin diye kapat
                Image sbImg = scrollbar.GetComponent<Image>();
                if (sbImg != null) sbImg.raycastTarget = false;
            }

            // Content arkaplanı sürüklemeleri algılasın
            Image contentImg = content.GetComponent<Image>();
            if (contentImg == null)
            {
                contentImg = content.gameObject.AddComponent<Image>();
                contentImg.color = new Color(0,0,0,0);
            }
            contentImg.raycastTarget = true;

            // Doğrudan Content paneline sürükleme algılayıcı ekle
            ContentDragHandler dragHandler = content.gameObject.AddComponent<ContentDragHandler>();
            dragHandler.simpleScroll = this;
        }

        // Listeyi header'dan uzak başlat
        startY = content.anchoredPosition.y + initialShiftY;
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, startY);

        if (scrollbar != null)
            scrollbar.onValueChanged.AddListener(OnScrollChanged);
    }

    public void OnScrollChanged(float value)
    {
        float offset = Mathf.Lerp(-upRange, downRange, value);
        float targetY = startY + offset;
        float minY = startY - upRange;
        float maxY = startY + downRange;
        targetY = Mathf.Clamp(targetY, minY, maxY);
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetY);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragStartY = eventData.position.y;
        contentStartY = content.anchoredPosition.y;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float deltaY = eventData.position.y - dragStartY;
        // Ekranda yukarı kaydırınca liste yukarı gitmeli (y değeri artmalı)
        float newY = contentStartY + deltaY;

        float minY = startY - upRange;
        float maxY = startY + downRange;
        newY = Mathf.Clamp(newY, minY, maxY);

        content.anchoredPosition = new Vector2(content.anchoredPosition.x, newY);

        // Scrollbar varsa değerini güncelle (döngüye girmemesi için onValueChanged tetiklenmeden)
        if (scrollbar != null && (downRange + upRange) > 0)
        {
            float t = (newY - minY) / (maxY - minY);
            scrollbar.SetValueWithoutNotify(t);
        }
    }
}
