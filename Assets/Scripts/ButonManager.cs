using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButonManager : MonoBehaviour
{
    // Inspector'dan atayacaðýnýz tüm butonlarýn RectTransform'larý
    public List<RectTransform> allButtons;

    // Butonlar arasýndaki boþluk
    public float buttonSpacing = 51f;
    public GameObject kahveTablo;
    public GameObject kahveAlaný;

    // Bu metot, ExpandManager tarafýndan çaðrýlacak
    public void HideAndShiftButtons(RectTransform buttonToHide)
    {
        // Týklanan butonu pürüzsüz bir þekilde yok et
        buttonToHide.DOScale(Vector3.zero, 0.25f).OnComplete(() =>
        {
            // Animasyon bitince butonu tamamen pasif hale getir
            buttonToHide.gameObject.SetActive(false);
        });

        // "Gizlenen" butonun Y pozisyonunu al
        float hiddenButtonY = buttonToHide.anchoredPosition.y;

        // Gizlenen butonun altýndaki diðer butonlarý yukarý kaydýr
        foreach (RectTransform buttonRect in allButtons)
        {
            // Gizlenen butonun kendisi ve zaten gizlenmiþ olanlar hariç
            if (buttonRect != buttonToHide && buttonRect.gameObject.activeSelf)
            {
                if (buttonRect.anchoredPosition.y < hiddenButtonY)
                {
                    // Butonun yüksekliðini ve boþluðu al
                    float buttonHeight = buttonRect.sizeDelta.y;
                    buttonRect.DOAnchorPosY(buttonRect.anchoredPosition.y + buttonHeight + buttonSpacing, 0.5f).SetEase(Ease.OutSine);
                }
            }
        }
    }
    public void KahveTablo()
    {
        kahveTablo.SetActive(true);
        kahveAlaný.SetActive(true);
    }
}