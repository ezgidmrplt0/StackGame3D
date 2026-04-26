using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class TemizlemeSistemi : MonoBehaviour
{
    public float temizlemeSuresi = 1.5f;

    private Dictionary<GameObject, float> temizlemeProgress = new Dictionary<GameObject, float>();
    private HashSet<GameObject> temizlenenAlanlar = new HashSet<GameObject>(); // animasyon süresince tekrar tetiklenmesin
    private List<GameObject> temasEdilenKirliAlanlar = new List<GameObject>();

    private KirlilikYonetici kirlilikYonetici;
    private OyuncuVeKamera oyuncu;
    private bool isSlowed = false;

    private void Start()
    {
        kirlilikYonetici = FindObjectOfType<KirlilikYonetici>();
        oyuncu = GetComponent<OyuncuVeKamera>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("KirliAlan"))
        {
            if (!temasEdilenKirliAlanlar.Contains(other.gameObject))
            {
                temasEdilenKirliAlanlar.Add(other.gameObject);
            }

            if (!isSlowed && oyuncu != null)
            {
                isSlowed = true;
                oyuncu.SetSpeedMultiplier(0.5f);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("KirliAlan")) return;
        if (temizlenenAlanlar.Contains(other.gameObject)) return; // zaten temizleniyor, atla

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
            temizlenenAlanlar.Add(temizlenenAlan); // tekrar tetiklenmeyi engelle
            temizlemeProgress.Remove(temizlenenAlan);
            temasEdilenKirliAlanlar.Remove(temizlenenAlan);

            temizlenenAlan.transform.DOScale(Vector3.zero, 0.5f).OnComplete(() =>
            {
                temizlenenAlan.SetActive(false);
                temizlenenAlan.transform.localScale = Vector3.one;
                SetObjectAlpha(temizlenenAlan, 1f);
                temizlenenAlanlar.Remove(temizlenenAlan);
            });

            if (kirlilikYonetici != null)
            {
                kirlilikYonetici.KirliAlanTemizlendi();
            }

            if (temasEdilenKirliAlanlar.Count == 0)
            {
                RestoreSpeed();
            }

            Debug.Log("Kirli alan tamamen temizlendi!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("KirliAlan"))
        {
            temasEdilenKirliAlanlar.Remove(other.gameObject);

            if (temasEdilenKirliAlanlar.Count == 0)
            {
                RestoreSpeed();
            }
        }
    }

    private void RestoreSpeed()
    {
        if (isSlowed && oyuncu != null)
        {
            isSlowed = false;
            oyuncu.SetSpeedMultiplier(1f);
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