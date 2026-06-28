using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class KirlilikYonetici : MonoBehaviour
{
    [Header("Kir Prefab")]
    public GameObject kirPrefab;

    [Header("Zemin")]
    [Tooltip("'Zemin' parent objesi — childlarının collider'ları spawn alanı olarak kullanılır.")]
    public Transform zeminParent;
    [Tooltip("Zeminin üzerinden ne kadar yüksekte spawn olsun.")]
    public float yukseklikOfseti = 0.05f;

    [Header("Spawn Ayarları")]
    public float minKirlenmeSuresi = 5f;
    public float maxKirlenmeSuresi = 12f;
    public int maxKirSayisi = 8;
    public float olusmaAnimasyonSuresi = 1f;

    [Header("UI")]
    public Slider kirlilikBar;
    public Image barRenkImage;
    public Color temizRenk = Color.green;
    public Color kirliRenk = Color.red;

    public List<GameObject> aktifKirler = new List<GameObject>();

    private Collider[] zeminParcalari;

    private void Start()
    {
        if (zeminParent != null)
            zeminParcalari = zeminParent.GetComponentsInChildren<Collider>();

        if (kirlilikBar != null)
        {
            kirlilikBar.value = 0f;
            if (barRenkImage != null)
                barRenkImage.color = new Color(temizRenk.r, temizRenk.g, temizRenk.b, 0f);
        }

        StartCoroutine(KirlenmeDongusu());
    }

    private IEnumerator KirlenmeDongusu()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minKirlenmeSuresi, maxKirlenmeSuresi));

            if (kirPrefab == null || zeminParcalari == null || zeminParcalari.Length == 0) continue;
            if (aktifKirler.Count >= maxKirSayisi) continue;

            Vector3 spawnPos = RastgelePozisyon();
            GameObject yeniKir = Instantiate(kirPrefab, spawnPos, Quaternion.identity);

            Renderer rend = yeniKir.GetComponent<Renderer>();
            if (rend != null)
            {
                SetAlpha(rend, 0f);
                rend.material.DOFade(1f, olusmaAnimasyonSuresi).SetEase(Ease.OutQuad);
            }

            aktifKirler.Add(yeniKir);
            KirlilikBariniGuncelle();
        }
    }

    private Vector3 RastgelePozisyon()
    {
        Collider secilen = zeminParcalari[Random.Range(0, zeminParcalari.Length)];
        Bounds b = secilen.bounds;

        float x = Random.Range(b.min.x, b.max.x);
        float z = Random.Range(b.min.z, b.max.z);
        float y = b.max.y + yukseklikOfseti;

        return new Vector3(x, y, z);
    }

    public void KirliAlanTemizlendi(GameObject kirliAlan)
    {
        aktifKirler.Remove(kirliAlan);
        KirlilikBariniGuncelle();
    }

    private void KirlilikBariniGuncelle()
    {
        if (kirlilikBar == null) return;

        float oran = maxKirSayisi > 0 ? (float)aktifKirler.Count / maxKirSayisi : 0f;
        kirlilikBar.DOValue(oran, 0.5f).SetEase(Ease.OutQuad);

        if (barRenkImage != null)
        {
            Color hedefRenk = aktifKirler.Count == 0
                ? new Color(temizRenk.r, temizRenk.g, temizRenk.b, 0f)
                : Color.Lerp(temizRenk, kirliRenk, oran);
            barRenkImage.DOColor(hedefRenk, 0.5f);
        }
    }

    private void SetAlpha(Renderer rend, float alpha)
    {
        Color c = rend.material.color;
        c.a = alpha;
        rend.material.color = c;
    }

    private void OnDrawGizmos()
    {
        if (zeminParent == null) return;

        Collider[] parcalar = zeminParent.GetComponentsInChildren<Collider>();
        foreach (Collider col in parcalar)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.2f);
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f);
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
