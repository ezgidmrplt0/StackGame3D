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
    public float temizlemeSuresi = 2f;

    private KirlilikYonetici kirlilikYonetici;
    private bool satinAlindi = false;
    private GameObject aktifTemizlikci;

    private void Start()
    {
        // Disable duplicate or unconfigured spawner components in the scene or on clones
        if (spawnPozisyon == null)
        {
            Debug.LogWarning($"TemizlikciPanelVeNPC on {gameObject.name} has no spawnPozisyon set. Disabling to prevent errors.");
            enabled = false;
            return;
        }

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
    [HideInInspector] public float temizlemeSuresi = 2f;
    [HideInInspector] public KirlilikYonetici kirlilikYonetici;

    public bool sadeceYEkseni = true;
    public float donusHizi = 10f;

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

            Vector3 targetPos = hedefAlan.transform.position;

            // Move and Rotate towards targetPos without depending on DOTween's WaitForCompletion which could block
            while (Vector3.Distance(transform.position, targetPos) > 0.5f)
            {
                if (hedefAlan == null) break;
                targetPos = hedefAlan.transform.position;

                // Rotation
                Vector3 direction = targetPos - transform.position;
                if (sadeceYEkseni) direction.y = 0;
                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * donusHizi);
                }

                // Movement
                transform.position = Vector3.MoveTowards(transform.position, targetPos, hareketHizi * Time.deltaTime);
                yield return null;
            }

            if (hedefAlan == null) continue;

            // Clean the dirt over time (fading it)
            Renderer rend = hedefAlan.GetComponent<Renderer>();
            float elapsed = 0f;
            Color startColor = Color.white;
            if (rend != null && rend.material != null)
            {
                startColor = rend.material.color;
            }

            while (elapsed < temizlemeSuresi)
            {
                if (hedefAlan == null) break;
                elapsed += Time.deltaTime;
                float progress = elapsed / temizlemeSuresi;

                if (rend != null && rend.material != null)
                {
                    Color c = startColor;
                    c.a = Mathf.Lerp(startColor.a, 0f, progress);
                    rend.material.color = c;
                }
                yield return null;
            }

            if (hedefAlan != null)
            {
                kirlilikYonetici.KirliAlanTemizlendi(hedefAlan);
                Destroy(hedefAlan);
            }
        }
    }
}
