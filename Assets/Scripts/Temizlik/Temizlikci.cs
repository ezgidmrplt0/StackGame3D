using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro; // <-- TextMeshPro için

public class TemizlikciPanelVeNPC : MonoBehaviour
{
    [Header("Temizlikçi Prefab ve Spawn")]
    public GameObject temizlikciPrefab;  // Temizlikçi prefabýný sürükle
    public Transform spawnPozisyon;       // Prefabýn sahnede doðacaðý nokta

    [Header("UI Baðlantýlarý")]
    public Button satinAlButton;          // Satýn alma butonu
    public TextMeshProUGUI fiyatText;    // Fiyat göstermek için TextMeshPro
    public int fiyat = 1000;              // Temizlikçi ücreti

    [Header("Temizlik Ayarlarý")]
    public float hareketHizi = 13f;        // Temizlikçi hareket hýzý
    public float temizlemeSuresi = 180f;  // Temizleme süresi: 3 dakika

    private KirlilikYonetici kirlilikYonetici;
    private bool satinAlindi = false;     // Satýn alma kontrolü
    private GameObject aktifTemizlikci;   // Sahnedeki aktif NPC

    private void Start()
    {
        kirlilikYonetici = FindObjectOfType<KirlilikYonetici>();

        // Fiyatý UI'da göster
        if (fiyatText != null)
            fiyatText.text = fiyat.ToString();

        // Butona týklama eventi ekle
        if (satinAlButton != null)
            satinAlButton.onClick.AddListener(Satinal);
    }

    // Satýn alma butonuna basýldýðýnda çalýþýr
    public void Satinal()
    {
        if (!satinAlindi)
        {
            satinAlindi = true;

            // Prefab yoksa sahnede spawn et
            if (temizlikciPrefab != null && aktifTemizlikci == null)
            {
                aktifTemizlikci = Instantiate(temizlikciPrefab, spawnPozisyon.position, Quaternion.identity);
                TemizlikciNPC npcScript = aktifTemizlikci.AddComponent<TemizlikciNPC>();
                npcScript.hareketHizi = hareketHizi;
                npcScript.temizlemeSuresi = temizlemeSuresi;
                npcScript.kirlilikYonetici = kirlilikYonetici;
            }

            // Butonu pasif yap
            if (satinAlButton != null)
                satinAlButton.interactable = false;

            Debug.Log("Temizlikçi satýn alýndý! Ücret: " + fiyat);
        }
    }
}

// ---------------- Temizlikçi NPC Scripti ----------------
public class TemizlikciNPC : MonoBehaviour
{
    [HideInInspector] public float hareketHizi = 3f;
    [HideInInspector] public float temizlemeSuresi = 180f;
    [HideInInspector] public KirlilikYonetici kirlilikYonetici;

    private void Start()
    {
        StartCoroutine(TemizlikDongusu());
    }

    private IEnumerator TemizlikDongusu()
    {
        while (true)
        {
            // Aktif kirli alanlarý al
            List<GameObject> aktifKirliAlanlar = new List<GameObject>();
            foreach (GameObject alan in kirlilikYonetici.kirliAlanlar)
            {
                if (alan.activeSelf)
                    aktifKirliAlanlar.Add(alan);
            }

            if (aktifKirliAlanlar.Count == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Rastgele bir kirli alan seç
            GameObject hedefAlan = aktifKirliAlanlar[Random.Range(0, aktifKirliAlanlar.Count)];

            // Temizlikçiyi hedef alana taþý
            Vector3 hedefPozisyon = hedefAlan.transform.position;
            yield return transform.DOMove(hedefPozisyon, Vector3.Distance(transform.position, hedefPozisyon) / hareketHizi)
                                  .SetEase(Ease.Linear)
                                  .WaitForCompletion();

            // Temizleme animasyonu (alpha düþürme)
            float zaman = 0f;
            Renderer renderer = hedefAlan.GetComponent<Renderer>();
            Color baslangicRengi = renderer.material.color;

            while (zaman < temizlemeSuresi)
            {
                zaman += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, zaman / temizlemeSuresi);
                renderer.material.color = new Color(baslangicRengi.r, baslangicRengi.g, baslangicRengi.b, alpha);
                yield return null;
            }

            // Temizleme tamamlandý
            hedefAlan.SetActive(false);
            renderer.material.color = new Color(baslangicRengi.r, baslangicRengi.g, baslangicRengi.b, 1f);

            if (kirlilikYonetici != null)
                kirlilikYonetici.KirliAlanTemizlendi();

            Debug.Log("Temizlikçi bir alaný temizledi!");
        }
    }
}
