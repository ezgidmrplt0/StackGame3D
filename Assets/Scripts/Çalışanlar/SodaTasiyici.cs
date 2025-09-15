using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class SodaTasiyici : MonoBehaviour
{
    [Header("Ayarlar")]
    public float speed = 200f;
    public int stackLimit = 7;
    public float toplamaAraligi = 0.2f;
    public float calismaSuresi = 20f; // saniye

    [Header("Drop Ayarlarý")]
    public Transform dropTargetTransform; // Inspector'dan sürükleyeceðin hedef

    [Header("Soda Boyut ve Stack Ayarlarý")]
    public Vector3 sodaScale = new Vector3(0.0025f, 0.0025f, 0.0025f);
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

    [Header("Hedef Noktalar (Inspector’dan ekle)")]
    public Transform[] yolNoktalari; // Oklarla çizdiðin ara noktalar

    private List<Transform> stack = new List<Transform>();
    private bool aktif = false;
    private bool dropAlaniTemas = false;

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
                    yeniTasiyici.dropTargetTransform = this.dropTargetTransform;
                    yeniTasiyici.yolNoktalari = this.yolNoktalari;
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

            // 3) Oklarla çizilen yol noktalarý üzerinden ilerle
            if (yolNoktalari != null && yolNoktalari.Length > 0)
            {
                yield return StartCoroutine(GitSirasiyla(yolNoktalari));
            }

            // 4) StackSilmeNoktasi0 tag'li alana git
            GameObject dropArea = GameObject.FindGameObjectWithTag("StackSilmeNoktasi0");
            if (dropArea != null)
            {
                yield return StartCoroutine(Git(dropArea.transform.position));

                // Drop alanýnda bekleyerek temas kontrolü
                float waitTime = 0f;
                while (!dropAlaniTemas && waitTime < 3f) // Maksimum 3 saniye bekleyelim
                {
                    waitTime += Time.deltaTime;
                    yield return null;
                }

                if (dropAlaniTemas)
                {
                    // 5) Stack'i belirlenen transforma býrak
                    yield return StartCoroutine(DropSequence(dropTargetTransform));
                }
            }
            else
            {
                Debug.LogError("StackSilmeNoktasi0 bulunamadý!");
            }
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

        dropAlaniTemas = false; // Temas durumunu sýfýrla
        Debug.Log("Drop sequence bitti");
    }

    IEnumerator Git(Vector3 hedef)
    {
        while (Vector3.Distance(transform.position, hedef) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, hedef,3.5f * speed * Time.deltaTime);

            Vector3 dir = (hedef - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);


            yield return null;
        }
    }

    IEnumerator GitSirasiyla(Transform[] noktalar)
    {
        foreach (Transform hedef in noktalar)
        {
            if (hedef != null)
                yield return StartCoroutine(Git(hedef.position));
        }
    }

    // Collision ile temas kontrolü
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            dropAlaniTemas = true;
            Debug.Log("Drop alanýna temas edildi");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            dropAlaniTemas = false;
            Debug.Log("Drop alanýndan çýkýldý");
        }
    }

    void OnValidate()
    {
        sodaScale = new Vector3(
            Mathf.Max(0.0025f, sodaScale.x),
            Mathf.Max(0.0025f, sodaScale.y),
            Mathf.Max(0.0025f, sodaScale.z)
        );
        stackSpacing = Mathf.Max(0.01f, stackSpacing);
        jumpHeightMultiplier = Mathf.Max(0.01f, jumpHeightMultiplier);
    }
}
