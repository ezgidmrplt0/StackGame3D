using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class TemizlemeSistemi : MonoBehaviour
{
    // Temizleme iþleminin ne kadar süreceði
    public float temizlemeSuresi = 1.5f;

    // Her kirli alanýn temizlenme progressini saklayacaðýmýz sözlük
    private Dictionary<GameObject, float> temizlemeProgress = new Dictionary<GameObject, float>();

    // Temizleme iþleminin baþladýðý alanýn referansý
    private GameObject mevcutTemizlenenAlan;

    private void OnTriggerStay(Collider other)
    {
        // Temas edilen objenin "KirliAlan" tag'ine sahip olduðunu kontrol et
        if (other.CompareTag("KirliAlan"))
        {
            mevcutTemizlenenAlan = other.gameObject;

            // Eðer bu alaný daha önce temizlemeye baþlamadýysak, progress'ini sýfýr olarak ekle
            if (!temizlemeProgress.ContainsKey(mevcutTemizlenenAlan))
            {
                temizlemeProgress.Add(mevcutTemizlenenAlan, 0f);
            }

            // Geçen zamana göre progress'i artýr
            temizlemeProgress[mevcutTemizlenenAlan] += Time.deltaTime;

            // Progress'i maksimum temizleme süresiyle sýnýrlý tut
            temizlemeProgress[mevcutTemizlenenAlan] = Mathf.Clamp(temizlemeProgress[mevcutTemizlenenAlan], 0f, temizlemeSuresi);

            // Temizleme yüzdesini hesapla (0.0 ile 1.0 arasýnda)
            float temizlemeYuzdesi = temizlemeProgress[mevcutTemizlenenAlan] / temizlemeSuresi;

            // Objenin alfa deðerini, temizleme yüzdesine göre ayarla
            SetObjectAlpha(mevcutTemizlenenAlan, 1f - temizlemeYuzdesi);

            // Eðer temizleme tamamlandýysa (progress süreyi geçtiyse)
            if (temizlemeProgress[mevcutTemizlenenAlan] >= temizlemeSuresi)
            {
                // Objenin pasif hale gelme animasyonunu baþlat
                mevcutTemizlenenAlan.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
                {
                    mevcutTemizlenenAlan.SetActive(false);
                    // Objenin boyutunu ve alfasýný bir sonraki kullaným için hazýrla
                    mevcutTemizlenenAlan.transform.localScale = Vector3.one;
                    SetObjectAlpha(mevcutTemizlenenAlan, 1f);
                });

                // Temizleme iþlemini tamamla ve sözlükten kaldýr
                temizlemeProgress.Remove(mevcutTemizlenenAlan);
                mevcutTemizlenenAlan = null;
                Debug.Log("Kirli alan tamamen temizlendi!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Eðer temas, temizlenmekte olan alandan çýktýysa
        if (other.gameObject == mevcutTemizlenenAlan)
        {
            mevcutTemizlenenAlan = null;
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