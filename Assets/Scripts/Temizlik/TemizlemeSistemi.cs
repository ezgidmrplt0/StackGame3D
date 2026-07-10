using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class TemizlemeSistemi : MonoBehaviour
{
    public float temizlemeSuresi = 2.0f;
    public float algilaMesafesi = 3.5f;

    private Dictionary<GameObject, float> temizlemeProgress = new Dictionary<GameObject, float>();
    private HashSet<GameObject> temizlenenAlanlar = new HashSet<GameObject>();
    private HashSet<GameObject> triggerTemasindakiKirler = new HashSet<GameObject>();

    private KirlilikYonetici kirlilikYonetici;
    private Transform oyuncuTransform;

    private void Start()
    {
        temizlemeSuresi = 2.0f;
        algilaMesafesi = 3.5f;
        EnsureReferences();
    }

    private void EnsureReferences()
    {
        if (kirlilikYonetici == null)
        {
            kirlilikYonetici = FindObjectOfType<KirlilikYonetici>();
        }
        if (oyuncuTransform == null)
        {
            OyuncuVeKamera oyuncu = GetComponentInParent<OyuncuVeKamera>();
            oyuncuTransform = oyuncu != null ? oyuncu.transform : transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        EnsureReferences();
        if (kirlilikYonetici != null && kirlilikYonetici.aktifKirler.Contains(other.gameObject))
        {
            triggerTemasindakiKirler.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        triggerTemasindakiKirler.Remove(other.gameObject);
    }

    private void Update()
    {
        EnsureReferences();
        if (kirlilikYonetici == null || kirlilikYonetici.aktifKirler.Count == 0) return;

        // Clean up destroyed references from our trigger set
        triggerTemasindakiKirler.RemoveWhere(item => item == null);

        // Aktif kirleri kopyala (döngü içinde liste değişebilir)
        List<GameObject> mevcutKirler = new List<GameObject>(kirlilikYonetici.aktifKirler);

        foreach (GameObject kir in mevcutKirler)
        {
            if (kir == null) continue;
            if (temizlenenAlanlar.Contains(kir)) continue;

            // Check if player is near this dirt either via trigger contact OR distance check
            bool temasVar = triggerTemasindakiKirler.Contains(kir);
            if (!temasVar)
            {
                Vector3 fark = oyuncuTransform.position - kir.transform.position;
                float mesafe = new Vector2(fark.x, fark.z).magnitude;
                if (mesafe <= algilaMesafesi)
                {
                    temasVar = true;
                }
            }

            if (!temasVar)
            {
                // If player walked away, we reset the progress for this dirt
                if (temizlemeProgress.ContainsKey(kir))
                {
                    temizlemeProgress.Remove(kir);
                    SetObjectAlpha(kir, 1f);
                }
                continue;
            }

            if (!temizlemeProgress.ContainsKey(kir))
                temizlemeProgress[kir] = 0f;

            temizlemeProgress[kir] += Time.deltaTime;

            float yuzde = Mathf.Clamp01(temizlemeProgress[kir] / temizlemeSuresi);
            SetObjectAlpha(kir, 1f - yuzde);

            if (temizlemeProgress[kir] >= temizlemeSuresi)
            {
                temizlenenAlanlar.Add(kir);
                temizlemeProgress.Remove(kir);
                triggerTemasindakiKirler.Remove(kir);

                kirlilikYonetici.KirliAlanTemizlendi(kir);

                kir.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
                {
                    temizlenenAlanlar.Remove(kir);
                    Destroy(kir);
                });
            }
        }
    }

    private void SetObjectAlpha(GameObject obj, float alpha)
    {
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend != null && rend.material != null)
        {
            Color c = rend.material.color;
            c.a = alpha;
            rend.material.color = c;
        }
    }
}
