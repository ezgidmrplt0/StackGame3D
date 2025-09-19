using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class KirlilikYonetici : MonoBehaviour
{
    public GameObject[] kirliAlanlar;
    public float minKirlenmeSuresi = 15f;
    public float maxKirlenmeSuresi = 45f;
    public float olusmaAnimasyonSuresi = 1f; // Oluþum animasyonunun süresi

    private void Start()
    {
        // Baþlangýçta tüm kirli alanlarý gizle ve transparanlýklarýný sýfýrla
        foreach (GameObject kirliAlan in kirliAlanlar)
        {
            SetObjectAlpha(kirliAlan, 0f); // Tamamen transparan yap
            kirliAlan.SetActive(false);    // Pasif hale getir
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

                // Objenin aktif olmasýný saðla
                yeniKirliAlan.SetActive(true);
                // Alfa deðerini sýfýrdan bire animasyonla getirerek görünür yap
                yeniKirliAlan.GetComponent<Renderer>().material.DOFade(1f, olusmaAnimasyonSuresi)
                    .SetEase(Ease.OutQuad); // Oluþum animasyonu için yumuþak geçiþ

                Debug.Log("Yeni bir kirli alan belirdi!");
            }
        }
    }

    // Objenin alfa deðerini ayarlayan yardýmcý fonksiyon
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