using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class TemizlemeSistemi : MonoBehaviour
{
    public float temizlemeSuresi = 1.5f;

    private Dictionary<GameObject, float> temizlemeProgress = new Dictionary<GameObject, float>();
    private GameObject mevcutTemizlenenAlan;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("KirliAlan"))
        {
            mevcutTemizlenenAlan = other.gameObject;

            // Oyuncu yavaþlasýn
            OyuncuVeKamera oyuncu = GetComponent<OyuncuVeKamera>();
            if (oyuncu != null)
            {
                oyuncu.SetSpeedMultiplier(0.5f); // %50 hýz
            }

            // Eðer bu alaný daha önce temizlemeye baþlamadýysak, progress'ini sýfýr olarak ekle
            if (!temizlemeProgress.ContainsKey(mevcutTemizlenenAlan))
            {
                temizlemeProgress.Add(mevcutTemizlenenAlan, 0f);
            }

            // Geçen zamana göre progress'i artýr
            temizlemeProgress[mevcutTemizlenenAlan] += Time.deltaTime;

            // Progress'i maksimum temizleme süresiyle sýnýrlý tut
            temizlemeProgress[mevcutTemizlenenAlan] = Mathf.Clamp(temizlemeProgress[mevcutTemizlenenAlan], 0f, temizlemeSuresi);

            // Temizleme yüzdesini hesapla
            float temizlemeYuzdesi = temizlemeProgress[mevcutTemizlenenAlan] / temizlemeSuresi;

            // Objenin alfa deðerini ayarla
            SetObjectAlpha(mevcutTemizlenenAlan, 1f - temizlemeYuzdesi);

            // Eðer temizleme tamamlandýysa
            if (temizlemeProgress[mevcutTemizlenenAlan] >= temizlemeSuresi)
            {
                mevcutTemizlenenAlan.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
                {
                    mevcutTemizlenenAlan.SetActive(false);
                    mevcutTemizlenenAlan.transform.localScale = Vector3.one;
                    SetObjectAlpha(mevcutTemizlenenAlan, 1f);
                });

                temizlemeProgress.Remove(mevcutTemizlenenAlan);
                mevcutTemizlenenAlan = null;
                Debug.Log("Kirli alan tamamen temizlendi!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == mevcutTemizlenenAlan)
        {
            mevcutTemizlenenAlan = null;

            // Oyuncu hýzýný geri normale döndür
            OyuncuVeKamera oyuncu = GetComponent<OyuncuVeKamera>();
            if (oyuncu != null)
            {
                oyuncu.SetSpeedMultiplier(1f);
            }
        }
    }

    // Alfa deðerini ayarlayan fonksiyon
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
