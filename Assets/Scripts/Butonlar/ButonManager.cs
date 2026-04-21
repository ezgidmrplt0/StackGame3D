using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ButonManager : MonoBehaviour
{
    // Inspector'dan atayacağınız tüm butonların RectTransform'ları
    public List<RectTransform> allButtons;

    // Butonlar arasındaki boşluk
    public float buttonSpacing = 51f;
    public GameObject kahveTablo;
    public GameObject kahveAlanı;

    // Bu metot, ExpandManager tarafından çağrılacak
    public void HideAndShiftButtons(RectTransform buttonToHide)
    {
        // Tıklanan butonu pürüzsüz bir şekilde yok et
        buttonToHide.DOScale(Vector3.zero, 0.25f).OnComplete(() =>
        {
            // Animasyon bitince butonu tamamen pasif hale getir
            buttonToHide.gameObject.SetActive(false);
        });

        // "Gizlenen" butonun Y pozisyonunu al
        float hiddenButtonY = buttonToHide.anchoredPosition.y;

        // Gizlenen butonun altındaki diğer butonları yukarı kaydır
        foreach (RectTransform buttonRect in allButtons)
        {
            // Gizlenen butonun kendisi ve zaten gizlenmiş olanlar hariç
            if (buttonRect != buttonToHide && buttonRect.gameObject.activeSelf)
            {
                if (buttonRect.anchoredPosition.y < hiddenButtonY)
                {
                    // Butonun yüksekliğini ve boşluğu al
                    float buttonHeight = buttonRect.sizeDelta.y;
                    buttonRect.DOAnchorPosY(buttonRect.anchoredPosition.y + buttonHeight + buttonSpacing, 0.5f).SetEase(Ease.OutSine);
                }
            }
        }
    }
    public void KahveTablo()
    {
        kahveTablo.SetActive(true);
        kahveAlanı.SetActive(true);
    }
}