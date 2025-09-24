using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class TemizlemeSistemi : MonoBehaviour
{
    public float temizlemeSuresi = 1.5f;

    private Dictionary<GameObject, float> temizlemeProgress = new Dictionary<GameObject, float>();

    // Oyuncunun temas ettiði kirli alanlarý takip etmek için liste
    private List<GameObject> temasEdilenKirliAlanlar = new List<GameObject>();

    private KirlilikYonetici kirlilikYonetici;
    private OyuncuVeKamera oyuncu;

    private void Start()
    {
        kirlilikYonetici = FindObjectOfType<KirlilikYonetici>();
        oyuncu = GetComponent<OyuncuVeKamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("KirliAlan"))
        {
            // Yeni bir kirli alana girdiðimizde listeye ekle
            if (!temasEdilenKirliAlanlar.Contains(other.gameObject))
            {
                temasEdilenKirliAlanlar.Add(other.gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("KirliAlan"))
        {
            // Oyuncu en az bir kirli alanýn içindeyse yavaþla
            if (oyuncu != null)
            {
                oyuncu.SetSpeedMultiplier(0.5f);
            }

            if (!temizlemeProgress.ContainsKey(other.gameObject))
            {
                temizlemeProgress.Add(other.gameObject, 0f);
            }

            temizlemeProgress[other.gameObject] += Time.deltaTime;
            temizlemeProgress[other.gameObject] = Mathf.Clamp(temizlemeProgress[other.gameObject], 0f, temizlemeSuresi);

            float temizlemeYuzdesi = temizlemeProgress[other.gameObject] / temizlemeSuresi;
            SetObjectAlpha(other.gameObject, 1f - temizlemeYuzdesi);

            if (temizlemeProgress[other.gameObject] >= temizlemeSuresi)
            {
                GameObject temizlenenAlan = other.gameObject;
                temizlenenAlan.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
                {
                    temizlenenAlan.SetActive(false);
                    temizlenenAlan.transform.localScale = Vector3.one;
                    SetObjectAlpha(temizlenenAlan, 1f);
                });

                temizlemeProgress.Remove(temizlenenAlan);

                if (kirlilikYonetici != null)
                {
                    kirlilikYonetici.KirliAlanTemizlendi();
                }

                // Temizlenen alaný listeden kaldýr
                temasEdilenKirliAlanlar.Remove(temizlenenAlan);

                // Eðer artýk hiçbir kirli alanla temas etmiyorsak hýzý normale döndür
                if (temasEdilenKirliAlanlar.Count == 0 && oyuncu != null)
                {
                    oyuncu.SetSpeedMultiplier(1f);
                }

                Debug.Log("Kirli alan tamamen temizlendi!");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("KirliAlan"))
        {
            // Temas ettiðimiz alanlar listesinden bu alaný kaldýr
            temasEdilenKirliAlanlar.Remove(other.gameObject);

            // Eðer artýk hiçbir kirli alanla temas etmiyorsak hýzý normale döndür
            if (temasEdilenKirliAlanlar.Count == 0 && oyuncu != null)
            {
                oyuncu.SetSpeedMultiplier(1f);
            }
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