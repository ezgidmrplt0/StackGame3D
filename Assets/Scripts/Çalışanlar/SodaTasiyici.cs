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
    public Transform dropTargetTransform;

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
    public Transform[] yolNoktalari;

    private List<Transform> stack = new List<Transform>();
    private bool aktif = false;
    private bool dropAlaniTemas = false;
    private Coroutine dropCoroutine = null;

    private enum SodaState
    {
        Idle,
        GoingToCollect,
        Collecting,
        GoingThroughPath,
        GoingToDrop,
        Dropping,
        Returning
    }

    private SodaState currentState = SodaState.Idle;
    private Transform almaNoktasi;
    private Transform[] currentPath;
    private int pathIndex;
    private float nextCollectTime;
    private float calismaBitisZamani;

    void Start()
    {
        if (stackRoot == null)
            stackRoot = transform;

        if (satinAlButton != null)
            satinAlButton.onClick.AddListener(SatinAl);

        if (fiyatText != null)
            fiyatText.text = fiyat.ToString();

        almaNoktasi = GameObject.FindGameObjectWithTag("SodaNoktasi")?.transform;
        if (almaNoktasi == null)
            Debug.LogError("SodaNoktasi bulunamadý!");
    }

    private void Update()
    {
        UpdateStackPositions();

        if (!aktif) return;

        switch (currentState)
        {
            case SodaState.GoingToCollect:
                MoveTowards(almaNoktasi.position);
                RotateTowards(almaNoktasi.position);
                if (Vector3.Distance(transform.position, almaNoktasi.position) < 0.05f)
                {
                    currentState = SodaState.Collecting;
                    nextCollectTime = Time.time;
                }
                break;

            case SodaState.Collecting:
                if (Time.time >= nextCollectTime && stack.Count < stackLimit)
                {
                    AddSoda();
                    nextCollectTime = Time.time + toplamaAraligi;
                }
                if (stack.Count >= stackLimit)
                {
                    if (yolNoktalari != null && yolNoktalari.Length > 0)
                    {
                        currentPath = yolNoktalari;
                        pathIndex = 0;
                        currentState = SodaState.GoingThroughPath;
                    }
                    else
                    {
                        currentState = SodaState.GoingToDrop;
                    }
                }
                break;

            case SodaState.GoingThroughPath:
                if (currentPath != null && pathIndex < currentPath.Length)
                {
                    MoveTowards(currentPath[pathIndex].position);
                    RotateTowards(currentPath[pathIndex].position);

                    if (Vector3.Distance(transform.position, currentPath[pathIndex].position) < 0.05f)
                        pathIndex++;
                }
                else
                {
                    if (currentPath == yolNoktalari)
                    {
                        currentState = SodaState.GoingToDrop;
                    }
                    else
                    {
                        currentState = SodaState.GoingToCollect;
                    }
                }
                break;

            case SodaState.GoingToDrop:
                Transform dropArea = GameObject.FindGameObjectWithTag("StackSilmeNoktasi0")?.transform;
                if (dropArea != null)
                {
                    MoveTowards(dropArea.position);
                    RotateTowards(dropArea.position);

                    if (Vector3.Distance(transform.position, dropArea.position) < 0.05f || dropAlaniTemas)
                    {
                        if (dropCoroutine == null)
                        {
                            currentState = SodaState.Dropping;
                            dropCoroutine = StartCoroutine(DropAllSodasCoroutine());
                        }
                    }
                }
                break;

            case SodaState.Dropping:
                // This state is now managed by the coroutine, so we do nothing here.
                break;

            case SodaState.Returning:
                if (yolNoktalari != null && yolNoktalari.Length > 0)
                {
                    Transform[] reversePath = new Transform[yolNoktalari.Length];
                    for (int i = 0; i < yolNoktalari.Length; i++)
                        reversePath[i] = yolNoktalari[yolNoktalari.Length - 1 - i];

                    currentPath = reversePath;
                    pathIndex = 0;
                    currentState = SodaState.GoingThroughPath;
                }
                else
                {
                    currentState = SodaState.GoingToCollect;
                }

                if (Time.time >= calismaBitisZamani)
                {
                    KaybolVeYokOl();
                    currentState = SodaState.Idle;
                }
                break;
        }
    }

    private IEnumerator DropAllSodasCoroutine()
    {
        while (stack.Count > 0)
        {
            Transform soda = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            soda.SetParent(null);

            if (SodaStack.Instance != null)
            {
                SodaStack.Instance.sodaDropList.Add(soda);
                int dropIndex = SodaStack.Instance.sodaDropList.Count - 1;
                Vector3 targetPos = dropTargetTransform.position + Vector3.up * (SodaStack.Instance.cubeHeight * dropIndex);
                soda.DOJump(targetPos, stackSpacing * jumpHeightMultiplier, 1, 0.4f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => soda.rotation = Quaternion.identity);
            }
            else
            {
                Vector3 targetPos = dropTargetTransform.position + Vector3.up * (stackSpacing * stack.Count);
                soda.DOJump(targetPos, stackSpacing * jumpHeightMultiplier, 1, 0.4f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => soda.rotation = Quaternion.identity);
                Debug.LogWarning("SodaStack.Instance bulunamadý! Doðrudan hedefe býrakýlýyor.");
            }

            yield return new WaitForSeconds(toplamaAraligi);
        }

        currentState = SodaState.Returning;
        dropCoroutine = null;
    }

    private void MoveTowards(Vector3 hedef)
    {
        transform.position = Vector3.MoveTowards(transform.position, hedef, speed * Time.deltaTime);
    }

    private void RotateTowards(Vector3 hedef)
    {
        Vector3 dir = (hedef - transform.position).normalized;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
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
                    yeniTasiyici.calismaBitisZamani = Time.time + calismaSuresi;
                    yeniTasiyici.currentState = SodaState.GoingToCollect;
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

            calismaBitisZamani = Time.time + calismaSuresi;
            currentState = SodaState.GoingToCollect;
        }
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

    private void KaybolVeYokOl()
    {
        if (gameObject == null) return;

        aktif = false;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InBack)
            .OnStart(() => Debug.Log("Sodacý küçülmeye baþladý"))
            .OnKill(() => Debug.Log("DOTween animasyonu iptal edildi"))
            .OnComplete(() =>
            {
                Debug.Log("Sodacý tamamen yok oldu. Destroy çaðrýldý.");
                gameObject.SetActive(false);
                Destroy(gameObject);
            });
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