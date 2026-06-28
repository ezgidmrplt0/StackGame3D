using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class TemizlikciPanelVeNPC : MonoBehaviour
{
    [Header("Temizlikci Prefab ve Spawn")]
    public GameObject temizlikciPrefab;
    public Transform spawnPozisyon;

    [Header("UI")]
    public Button satinAlButton;
    public TextMeshProUGUI fiyatText;
    public int fiyat = 1000;

    [Header("Temizlik Ayarlari")]
    public float hareketHizi = 13f;
    public float temizlemeSuresi = 10f;

    private KirlilikYonetici kirlilikYonetici;
    private bool satinAlindi = false;
    private GameObject aktifTemizlikci;

    private void Start()
    {
        kirlilikYonetici = FindObjectOfType<KirlilikYonetici>();

        if (fiyatText != null)
            fiyatText.text = fiyat.ToString();

        if (satinAlButton != null)
            satinAlButton.onClick.AddListener(Satinal);
    }

    public void Satinal()
    {
        if (satinAlindi) return;

        if (temizlikciPrefab == null)
        {
            Debug.LogError("TemizlikciPanelVeNPC: temizlikciPrefab atanmamis!");
            return;
        }

        if (spawnPozisyon == null)
        {
            Debug.LogError("TemizlikciPanelVeNPC: spawnPozisyon atanmamis!");
            return;
        }

        satinAlindi = true;

        aktifTemizlikci = Instantiate(temizlikciPrefab, spawnPozisyon.position, Quaternion.identity);
        TemizlikciNPC npcScript = aktifTemizlikci.AddComponent<TemizlikciNPC>();
        npcScript.hareketHizi = hareketHizi;
        npcScript.temizlemeSuresi = temizlemeSuresi;
        npcScript.kirlilikYonetici = kirlilikYonetici;

        if (satinAlButton != null)
            satinAlButton.interactable = false;
    }
}

public class TemizlikciNPC : MonoBehaviour
{
    [HideInInspector] public float hareketHizi = 13f;
    [HideInInspector] public float temizlemeSuresi = 10f;
    [HideInInspector] public KirlilikYonetici kirlilikYonetici;

    public bool sadeceYEkseni = true;
    public float donusSuresi = 0.15f;

    private void Start()
    {
        StartCoroutine(TemizlikDongusu());
    }

    private IEnumerator TemizlikDongusu()
    {
        while (true)
        {
            if (kirlilikYonetici == null || kirlilikYonetici.aktifKirler.Count == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            GameObject hedefAlan = kirlilikYonetici.aktifKirler[Random.Range(0, kirlilikYonetici.aktifKirler.Count)];
            if (hedefAlan == null)
            {
                kirlilikYonetici.aktifKirler.RemoveAll(x => x == null);
                continue;
            }

            yield return Dondur(hedefAlan.transform.position);

            float mesafe = Vector3.Distance(transform.position, hedefAlan.transform.position);
            float sure = mesafe / Mathf.Max(0.01f, hareketHizi);
            yield return transform.DOMove(hedefAlan.transform.position, sure)
                                  .SetEase(Ease.Linear)
                                  .WaitForCompletion();

            if (hedefAlan == null) continue;

            Renderer rend = hedefAlan.GetComponent<Renderer>();
            if (rend != null)
                yield return rend.material.DOFade(0f, temizlemeSuresi).SetEase(Ease.Linear).WaitForCompletion();

            if (hedefAlan != null)
            {
                kirlilikYonetici.KirliAlanTemizlendi(hedefAlan);
                Destroy(hedefAlan);
            }
        }
    }

    private IEnumerator Dondur(Vector3 hedefPozisyon)
    {
        Vector3 ileri = hedefPozisyon - transform.position;
        if (sadeceYEkseni) ileri.y = 0f;
        if (ileri.sqrMagnitude < 0.0001f) yield break;

        Quaternion hedefRot = Quaternion.LookRotation(ileri.normalized, Vector3.up);
        yield return transform.DORotateQuaternion(hedefRot, Mathf.Max(0f, donusSuresi))
                              .SetEase(Ease.OutSine)
                              .WaitForCompletion();
    }
}
