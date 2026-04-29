using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleScroll : MonoBehaviour
{
    [Header("UI")]
    public Scrollbar scrollbar;           
    public RectTransform content;         

    private float startY;

    [Header("Sınırlar (px)")]
    public float initialShiftY = 40f;     
    public float upRange = 0f;
    public float downRange = 400f;

    private ScrollRect scrollRect;

    void Start()
    {
        if (content.parent.GetComponent<RectMask2D>() == null && content.parent.GetComponent<Mask>() == null)
        {
            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.SetParent(content.parent, false);

            viewportRect.anchorMin = content.anchorMin;
            viewportRect.anchorMax = content.anchorMax;
            viewportRect.pivot = content.pivot;
            viewportRect.anchoredPosition = content.anchoredPosition;
            viewportRect.sizeDelta = content.sizeDelta;

            // Maskenin arkaplanını görünmez yap
            Image img = viewport.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            img.raycastTarget = false; // BAŞKA SEKMELERİ ENGELLEMEMESİ İÇİN KAPALI OLMALI!

            content.SetParent(viewportRect, true);

            // Unity'nin kendi kusursuz ScrollRect sistemini ekliyoruz!
            scrollRect = viewport.AddComponent<ScrollRect>();
            scrollRect.content = content;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Unrestricted; // Sınırları biz kodla belirleyeceğiz
            scrollRect.viewport = viewportRect;

            if (scrollbar != null)
            {
                scrollbar.transform.SetParent(viewportRect, true);
                scrollbar.transform.SetAsFirstSibling();
                
                Image[] sbImages = scrollbar.GetComponentsInChildren<Image>(true);
                foreach (Image imgComponent in sbImages)
                {
                    imgComponent.raycastTarget = false;
                }
                scrollbar.enabled = false; // Scrollbar'ı tamamen devre dışı bırak
            }
        }
        else
        {
            scrollRect = content.parent.gameObject.GetComponent<ScrollRect>();
            if (scrollRect == null)
            {
                scrollRect = content.parent.gameObject.AddComponent<ScrollRect>();
                scrollRect.content = content;
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
            }
        }

        // Listeyi header'dan uzak başlat
        startY = content.anchoredPosition.y + initialShiftY;
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, startY);

        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        }
    }

    private void OnScrollRectValueChanged(Vector2 normalizedPosition)
    {
        // Kendi upRange ve downRange sınırlarımızı zorla
        float targetY = content.anchoredPosition.y;
        float minY = startY - upRange;
        float maxY = startY + downRange;
        
        if (targetY < minY) targetY = minY;
        if (targetY > maxY) targetY = maxY;

        if (content.anchoredPosition.y != targetY)
        {
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetY);
        }
    }
}
