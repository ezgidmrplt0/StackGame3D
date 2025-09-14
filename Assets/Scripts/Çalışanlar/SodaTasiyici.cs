using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class SodaTasiyici : MonoBehaviour
{
    [Header("Ayarlar")]
    public float speed = 8f;
    public int stackLimit = 10;
    public float toplamaAraligi = 0.2f;
    public float calismaSuresi = 20f; // 20 saniye çalýþacak

    [Header("Referanslar")]
    public GameObject sodaPrefab;
    public Transform stackRoot;
    public Vector3 sodaBoyutu = new Vector3(0.003f, 0.003f, 0.003f);
    public float stackSpacing = 0.003f;

    [Header("Buton ve Fiyat")]
    public Button satinAlButton;
    public TextMeshProUGUI fiyatText;
    public int fiyat = 100;
    public float fiyatArtis = 0.2f;
    private bool aktif = false;

    [Header("Stack Noktalarý")]
    public Transform stackAlmaNoktasi;
    public Transform stackBirakmaNoktasi;

    private List<Transform> stack = new List<Transform>();
    private bool isInDropArea = false;

    void Start()
    {
        Debug.Log("SodaTasiyici Start: GameObject aktif mi: " + gameObject.activeSelf);

        // Referanslarý otomatik ata
        OtomatikReferansAta();

        if (satinAlButton != null)
        {
            satinAlButton.onClick.AddListener(SatinAl);
            Debug.Log("Buton onClick listener eklendi");
        }

        if (fiyatText != null)
        {
            fiyatText.text = fiyat.ToString();
        }
    }

    void OtomatikReferansAta()
    {
        // StackRoot atanmamýþsa bu GameObject'i ata
        if (stackRoot == null)
        {
            stackRoot = transform;
            Debug.Log("StackRoot otomatik atandý: " + stackRoot.name);
        }

        // SodaPrefab bulunamazsa resources'tan yükle
        if (sodaPrefab == null)
        {
            sodaPrefab = Resources.Load<GameObject>("SodaPrefab");
            if (sodaPrefab != null)
            {
                Debug.Log("SodaPrefab Resources'tan yüklendi");
            }
            else
            {
                Debug.LogError("SodaPrefab bulunamadý! Resources klasörüne soda prefabý ekleyin.");
            }
        }

        // Stack noktalarý atanmamýþsa otomatik ata
        if (stackAlmaNoktasi == null)
        {
            GameObject almaNoktasi = new GameObject("StackAlmaNoktasi");
            almaNoktasi.transform.SetParent(transform);
            almaNoktasi.transform.localPosition = new Vector3(0, 0, 2f);
            stackAlmaNoktasi = almaNoktasi.transform;
            Debug.Log("StackAlmaNoktasi otomatik oluþturuldu");
        }

        if (stackBirakmaNoktasi == null)
        {
            GameObject birakmaNoktasi = new GameObject("StackBirakmaNoktasi");
            birakmaNoktasi.transform.SetParent(transform);
            birakmaNoktasi.transform.localPosition = new Vector3(0, 0, -2f);
            stackBirakmaNoktasi = birakmaNoktasi.transform;
            Debug.Log("StackBirakmaNoktasi otomatik oluþturuldu");
        }
    }

    void Update()
    {
        UpdateStackPositions();
    }

    private void UpdateStackPositions()
    {
        for (int i = 0; i < stack.Count; i++)
        {
            Transform soda = stack[i];
            Vector3 targetPos = stackRoot.position + Vector3.up * stackSpacing * i;
            soda.position = Vector3.Lerp(soda.position, targetPos, Time.deltaTime * 10f);
            soda.rotation = Quaternion.identity;
        }
    }

    public void SatinAl()
    {
        Debug.Log("=== SATIN ALMA ÝÞLEMÝ BAÞLADI ===");

        if (aktif)
        {
            Debug.Log("Zaten aktif, iþlem iptal edildi");
            return;
        }

        int mevcutPara = PlayerPrefs.GetInt("Money", 500);
        Debug.Log("Para kontrolü: " + mevcutPara + " >= " + fiyat + " = " + (mevcutPara >= fiyat));

        if (mevcutPara >= fiyat)
        {
            Debug.Log("Para yeterli, satýn alma yapýlýyor...");

            int yeniPara = mevcutPara - fiyat;
            PlayerPrefs.SetInt("Money", yeniPara);

            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            aktif = true;

            fiyat = Mathf.RoundToInt(fiyat * (1 + fiyatArtis));
            if (fiyatText != null)
                fiyatText.text = fiyat.ToString();

            StartCoroutine(CalismaRutini());
        }
        else
        {
            Debug.Log("Yeterli para yok!");
        }

        Debug.Log("=== SATIN ALMA ÝÞLEMÝ TAMAMLANDI ===");
    }

    IEnumerator CalismaRutini()
    {
        Debug.Log("Çalýþma rutini baþladý");
        float timer = 0f;
        while (timer < calismaSuresi)
        {
            timer += Time.deltaTime;

            if (stackAlmaNoktasi != null)
            {
                yield return StartCoroutine(Git(stackAlmaNoktasi.position));
            }

            while (stack.Count < stackLimit)
            {
                AddSoda();
                yield return new WaitForSeconds(toplamaAraligi);
            }

            if (stackBirakmaNoktasi != null)
            {
                yield return StartCoroutine(Git(stackBirakmaNoktasi.position));
            }

            isInDropArea = true;
            yield return StartCoroutine(DropSequence());
            isInDropArea = false;
        }
        Debug.Log("Çalýþma rutini bitti");
    }

    void AddSoda()
    {
        if (stack.Count >= stackLimit)
        {
            return;
        }

        if (sodaPrefab == null || stackRoot == null)
        {
            Debug.LogError("SodaPrefab veya StackRoot referansý null! Lütfen Inspector'da ata.");
            return;
        }

        Vector3 spawnPos = stackRoot.position + Vector3.up * stackSpacing * stack.Count;
        GameObject newSoda = Instantiate(sodaPrefab, spawnPos, Quaternion.identity, stackRoot);

        newSoda.transform.localScale = Vector3.zero;
        newSoda.transform.DOScale(sodaBoyutu, 0.3f).SetEase(Ease.OutCubic);

        stack.Add(newSoda.transform);
    }

    IEnumerator DropSequence()
    {
        while (stack.Count > 0 && isInDropArea)
        {
            Transform soda = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);

            soda.SetParent(null);
            soda.tag = "SodaProduct";

            if (SodaStack.Instance != null)
            {
                SodaStack.Instance.sodaDropList.Add(soda);
            }

            Vector3 targetPos = stackBirakmaNoktasi.position + Vector3.up * 0.002f * SodaStack.Instance.sodaDropList.Count;
            soda.DOJump(targetPos, 0.002f, 1, 0.4f).SetEase(Ease.OutQuad);

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator Git(Vector3 hedef)
    {
        while (Vector3.Distance(transform.position, hedef) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, hedef, speed * Time.deltaTime);

            Vector3 dir = (hedef - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);

            yield return null;
        }
    }
}