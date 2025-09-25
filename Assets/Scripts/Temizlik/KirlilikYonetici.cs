using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class KirlilikYonetici : MonoBehaviour
{
    public GameObject[] kirliAlanlar;
    public float minKirlenmeSuresi = 15f;
    public float maxKirlenmeSuresi = 45f;
    public float olusmaAnimasyonSuresi = 1f;

    // Temizlik Barý deðiþkenleri
    public Slider kirlilikBar;
    public Image barRenkImage;
    public Color temizRenk = Color.green;
    public Color kirliRenk = Color.red;

    private int aktifKirliAlanSayisi = 0;

    private void Start()
    {
        foreach (GameObject kirliAlan in kirliAlanlar)
        {
            SetObjectAlpha(kirliAlan, 0f);
            kirliAlan.SetActive(false);
        }

        if (kirlilikBar != null)
        {
            kirlilikBar.value = 0f;
            // Baþlangýçta barýn doluluk kýsmýný tamamen saydam yap
            barRenkImage.color = new Color(temizRenk.r, temizRenk.g, temizRenk.b, 0f);
        }

        StartCoroutine(KirlenmeDöngüsü());
    }

    private IEnumerator KirlenmeDöngüsü()
    {
        while (true)
        {
            float beklemeSuresi = Random.Range(minKirlenmeSuresi, maxKirlenmeSuresi);
            yield return new WaitForSeconds(beklemeSuresi);

            List<GameObject> pasifAlanlar = new List<GameObject>();
            foreach (GameObject kirliAlan in kirliAlanlar)
            {
                if (!kirliAlan.activeSelf)
                {
                    pasifAlanlar.Add(kirliAlan);
                }
            }

            if (pasifAlanlar.Count > 0)
            {
                int rastgeleIndex = Random.Range(0, pasifAlanlar.Count);
                GameObject yeniKirliAlan = pasifAlanlar[rastgeleIndex];

                yeniKirliAlan.SetActive(true);
                yeniKirliAlan.GetComponent<Renderer>().material.DOFade(1f, olusmaAnimasyonSuresi)
                    .SetEase(Ease.OutQuad);

                aktifKirliAlanSayisi++;
                KirlilikBariniGuncelle();

                Debug.Log("Yeni bir kirli alan belirdi! Aktif kirli alan sayýsý: " + aktifKirliAlanSayisi);
            }
        }
    }

    public void KirliAlanTemizlendi()
    {
        aktifKirliAlanSayisi--;
        KirlilikBariniGuncelle();
        Debug.Log("Kirli alan temizlendi! Aktif kirli alan sayýsý: " + aktifKirliAlanSayisi);
    }

    private void KirlilikBariniGuncelle()
    {
        if (kirlilikBar != null)
        {
            float kirlilikYuzdesi = (float)aktifKirliAlanSayisi / kirliAlanlar.Length;

            // Slider'ýn doluluk deðerini güncelle
            kirlilikBar.DOValue(kirlilikYuzdesi, 0.5f).SetEase(Ease.OutQuad);

            // Eðer kirlilik yoksa saydam, varsa normal renge geçiþ yap
            Color hedefRenk;
            if (aktifKirliAlanSayisi == 0)
            {
                hedefRenk = new Color(temizRenk.r, temizRenk.g, temizRenk.b, 0f);
            }
            else
            {
                hedefRenk = Color.Lerp(temizRenk, kirliRenk, kirlilikYuzdesi);
            }
            barRenkImage.DOColor(hedefRenk, 0.5f);
        }
    }

    private void SetObjectAlpha(GameObject obj, float alpha)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Color color = renderer.material.color;
            color.a = alpha;
            renderer.material.color = color;
        }
    }
}