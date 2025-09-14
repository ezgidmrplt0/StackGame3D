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
    public float calismaSuresi = 20f; // saniye

    [Header("Drop Ayarlarý")]
    public Transform dropTargetTransform; // Inspector'dan sürükleyeceðin hedef

    [Header("Soda Boyut ve Stack Ayarlarý")]
    public Vector3 sodaScale = new Vector3(0.005f, 0.005f, 0.005f);
    public float stackSpacing = 0.15f;
    public float jumpHeightMultiplier = 0.5f;

    [Header("Prefab ve Referanslar")]
    public GameObject sodaPrefab;
    public Transform stackRoot;

    [Header("SodaTasiyici Prefab ve Spawn Pozisyon")]
    public GameObject sodaTasiyiciPrefab;
    public Transform spawnPozisyon;
    public string sodaciTag = "Sodaci";

    [Header("Buton ve Fiyat")]
    public Button satinAlButton;
    public TextMeshProUGUI fiyatText;
    public int fiyat = 100;
    public float fiyatArtis = 0.2f;

    private List<Transform> stack = new List<Transform>();
    private bool aktif = false;

    void Start()
    {
        if (stackRoot == null)
            stackRoot = transform;

        if (satinAlButton != null)
            satinAlButton.onClick.AddListener(SatinAl);

        if (fiyatText != null)
            fiyatText.text = fiyat.ToString();
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
        if (MoneyManager.Instance == null)
        {
            Debug.LogError("MoneyManager bulunamadý! Sahneye ekleyin.");
            return;
        }

        if (!MoneyManager.Instance.SpendMoney(fiyat))
        {
            Debug.Log("Yeterli para yok!");
            return;
        }

        GameObject existingSodaci = GameObject.FindWithTag(sodaciTag);
        if (existingSodaci == null)
        {
            if (sodaTasiyiciPrefab != null && spawnPozisyon != null)
            {
                GameObject obj = Instantiate(sodaTasiyiciPrefab, spawnPozisyon.position, Quaternion.identity);
                obj.SetActive(true);
                obj.tag = sodaciTag;

                SodaTasiyici yeniTasiyici = obj.GetComponent<SodaTasiyici>();
                if (yeniTasiyici != null)
                {
                    yeniTasiyici.aktif = true;
                    yeniTasiyici.sodaScale = this.sodaScale;
                    yeniTasiyici.stackSpacing = this.stackSpacing;
                    yeniTasiyici.jumpHeightMultiplier = this.jumpHeightMultiplier;
                    yeniTasiyici.StartCoroutine(yeniTasiyici.CalismaRutini());
                }

                Debug.Log("SodaTasiyici sahneye spawn edildi.");
            }
            else
            {
                Debug.LogError("sodaTasiyiciPrefab veya spawnPozisyon null!");
                return;
            }
        }
        else
        {
            Debug.Log("Zaten bir Sodaci var: " + existingSodaci.name);
        }

        if (!aktif)
        {
            aktif = true;
            fiyat = Mathf.RoundToInt(fiyat * (1 + fiyatArtis));
            if (fiyatText != null)
                fiyatText.text = fiyat.ToString();

            StartCoroutine(CalismaRutini());
        }
    }

    IEnumerator CalismaRutini()
    {
        Debug.Log("Çalýþma rutini baþladý");
        float timer = 0f;

        // Tek seferlik tag bulma
        GameObject almaObj = GameObject.FindGameObjectWithTag("SodaNoktasi");
        if (almaObj == null)
        {
            Debug.LogError("SodaNoktasi bulunamadý!");
            yield break;
        }
        Transform almaNoktasi = almaObj.transform;

        // Drop hedefi için artýk Inspector'dan sürüklenen kullanýlacak
        Transform birakmaNoktasi = dropTargetTransform;
        if (birakmaNoktasi == null)
        {
            // Fallback olarak eski tag-based sistem
            GameObject fallback = GameObject.FindGameObjectWithTag("StackSilmeNoktasi0");
            if (fallback != null)
                birakmaNoktasi = fallback.transform;
        }

        if (birakmaNoktasi == null)
        {
            Debug.LogError("Drop hedefi atanmadý!");
            yield break;
        }

        while (timer < calismaSuresi)
        {
            timer += Time.deltaTime;

            // 1) Alma noktasýna git
            yield return StartCoroutine(Git(almaNoktasi.position));

            // 2) stackLimit kadar soda topla
            while (stack.Count < stackLimit)
            {
                AddSoda();
                yield return new WaitForSeconds(toplamaAraligi);
            }

            // 3) Býrakma noktasýna git
            yield return StartCoroutine(Git(birakmaNoktasi.position));

            // 4) Stack'i býrak
            yield return StartCoroutine(DropSequence(birakmaNoktasi));
        }

        Debug.Log("Çalýþma rutini bitti");
        aktif = false;
    }

    void AddSoda()
    {
        if (stack.Count >= stackLimit) return;
        if (sodaPrefab == null || stackRoot == null) return;

        GameObject newSoda = Instantiate(sodaPrefab, stackRoot);
        newSoda.transform.localPosition = Vector3.up * stackSpacing * stack.Count;
        newSoda.transform.localRotation = Quaternion.identity;
        newSoda.transform.localScale = Vector3.zero;
        newSoda.transform.DOScale(sodaScale, 0.3f).SetEase(Ease.OutCubic);

        stack.Add(newSoda.transform);
        Debug.Log($"Soda eklendi. Toplam: {stack.Count}");
    }

    IEnumerator DropSequence(Transform hedef)
    {
        Debug.Log("Drop sequence baþladý");

        while (stack.Count > 0)
        {
            Transform soda = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            soda.SetParent(null);

            if (SodaStack.Instance != null)
            {
                SodaStack.Instance.sodaDropList.Add(soda);

                int dropIndex = SodaStack.Instance.sodaDropList.Count - 1;
                Vector3 targetPos = hedef.position + Vector3.up * (SodaStack.Instance.cubeHeight * dropIndex);

                soda.DOJump(targetPos, stackSpacing * jumpHeightMultiplier, 1, 0.4f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => { soda.rotation = Quaternion.identity; });
            }
            else
            {
                // SodaStack yoksa doðrudan hedef pozisyona býrak
                Vector3 targetPos = hedef.position + Vector3.up * (stackSpacing * (stack.Count));
                soda.DOJump(targetPos, stackSpacing * jumpHeightMultiplier, 1, 0.4f)
                    .SetEase(Ease.OutQuad);
                Debug.LogWarning("SodaStack.Instance bulunamadý! Doðrudan hedefe býrakýlýyor.");
            }

            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log("Drop sequence bitti");
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

    void OnValidate()
    {
        sodaScale = new Vector3(
            Mathf.Max(0.0001f, sodaScale.x),
            Mathf.Max(0.0001f, sodaScale.y),
            Mathf.Max(0.0001f, sodaScale.z)
        );
        stackSpacing = Mathf.Max(0.01f, stackSpacing);
        jumpHeightMultiplier = Mathf.Max(0.01f, jumpHeightMultiplier);
    }
}