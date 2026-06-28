using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class TemizlemeSistemi : MonoBehaviour
{
    public float temizlemeSuresi = 1.5f;
    public float algilaMesafesi = 1.2f;

    private Dictionary<GameObject, float> temizlemeProgress = new Dictionary<GameObject, float>();
    private HashSet<GameObject> temizlenenAlanlar = new HashSet<GameObject>();

    private KirlilikYonetici kirlilikYonetici;
    private Transform oyuncuTransform;

    private void Start()
    {
        kirlilikYonetici = FindObjectOfType<KirlilikYonetici>();
        OyuncuVeKamera oyuncu = GetComponentInParent<OyuncuVeKamera>();
        oyuncuTransform = oyuncu != null ? oyuncu.transform : transform;
    }

    private void Update()
    {
        if (kirlilikYonetici == null || kirlilikYonetici.aktifKirler.Count == 0) return;

        // Aktif kirleri kopyala (döngü içinde liste değişebilir)
        List<GameObject> mevcutKirler = new List<GameObject>(kirlilikYonetici.aktifKirler);

        foreach (GameObject kir in mevcutKirler)
        {
            if (kir == null) continue;
            if (temizlenenAlanlar.Contains(kir)) continue;

            Vector3 fark = oyuncuTransform.position - kir.transform.position;
            float mesafe = new Vector2(fark.x, fark.z).magnitude;
            if (mesafe > algilaMesafesi) continue;

            if (!temizlemeProgress.ContainsKey(kir))
                temizlemeProgress[kir] = 0f;

            temizlemeProgress[kir] += Time.deltaTime;

            float yuzde = Mathf.Clamp01(temizlemeProgress[kir] / temizlemeSuresi);
            SetObjectAlpha(kir, 1f - yuzde);

            if (temizlemeProgress[kir] >= temizlemeSuresi)
            {
                temizlenenAlanlar.Add(kir);
                temizlemeProgress.Remove(kir);

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
