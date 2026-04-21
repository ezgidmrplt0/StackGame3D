using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class CoffeeStackCollector : MonoBehaviour
{
    public static CoffeeStackCollector Instance;

    [Header("Stack Ayarları")]
    public GameObject kahvePrefab;
    public Transform stackRoot;
    public float stackHeight = 0.2f;
    public float stackSpawnInterval = 0.5f;
    public int stackLimit = 10;
    public Vector3 kahveTargetScale = new Vector3(0.5f, 0.5f, 0.5f);
    public Ease tweenEase = Ease.OutBack;

    [Header("Stok ve UI")]
    public TextMeshPro stokText;
    public TextMeshPro hamKahveText;
    public int kahveStogu = 100;
    public int hamKahve = 10;
    public GameObject alan;
    public GameObject buton;
    public GameObject butonKahve;

    // Stack Listesi
    public readonly List<Transform> stack = new List<Transform>();
    private Coroutine stackingLoop;

    // --- Bırakma (Drop) Ayarları ---
    [Header("Bırakma (Drop) Ayarları")]
    public string dropAreaTag = "KahveBirakmaNoktasi";
    public List<Transform> dropList = new List<Transform>();
    public Transform dropAreaTarget;
    public float dropSpacing = 2f;

    private Coroutine dropLoop;
    private bool isInDropArea = false;
    private int currentPrice = 100;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        UpdateStackPositions();
        GuncelleUI();
    }

    void UpdateStackPositions()
    {
        for (int i = 0; i < stack.Count; i++)
        {
            Transform cube = stack[i];
            float yOffset = kahveTargetScale.y * 0.5f;
            Vector3 targetPos = stackRoot.position + Vector3.up * (stackHeight * i + yOffset);
            cube.position = targetPos;
            cube.rotation = Quaternion.identity;
        }
    }

    void AddOneKahveToStack()
    {
        if (stack.Count >= stackLimit) return;
        if (kahveStogu <= 0) return;
        kahveStogu--;
        float yOffset = kahveTargetScale.y * 0.5f;
        Vector3 spawnPosition = stackRoot.position + Vector3.up * (stackHeight * stack.Count + yOffset);
        GameObject newCube = Instantiate(kahvePrefab, spawnPosition, Quaternion.identity);
        newCube.transform.SetParent(stackRoot);
        Rigidbody cubeRb = newCube.GetComponent<Rigidbody>();
        if (cubeRb != null) { cubeRb.isKinematic = true; cubeRb.useGravity = false; }
        newCube.transform.localScale = Vector3.zero;
        newCube.transform.DOScale(kahveTargetScale, 0.4f).SetEase(tweenEase);
        stack.Add(newCube.transform);
    }

    IEnumerator StackSpawnLoop()
    {
        var wait = new WaitForSeconds(stackSpawnInterval);
        while (true)
        {
            if (kahveStogu > 0 && stack.Count < stackLimit) AddOneKahveToStack();
            yield return wait;
        }
    }

    IEnumerator DropSequence()
    {
        while (stack.Count > 0 && isInDropArea)
        {
            if (dropAreaTarget == null) yield break;
            Transform cube = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);
            cube.SetParent(null);
            Rigidbody cubeRb = cube.GetComponent<Rigidbody>();
            if (cubeRb != null) { cubeRb.isKinematic = false; cubeRb.useGravity = true; cubeRb.drag = 1f; }
            int targetIndex = dropList.Count;
            Vector3 targetPos = dropAreaTarget.position + Vector3.up * (dropSpacing * targetIndex);
            cube.DOJump(targetPos, 0.5f, 1, 0.4f).SetEase(Ease.OutQuad);
            dropList.Add(cube);
            yield return new WaitForSeconds(0.1f);
        }
        dropLoop = null;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("KahveAl")) { if (stackingLoop == null) stackingLoop = StartCoroutine(StackSpawnLoop()); }
        if (other.CompareTag(dropAreaTag)) { isInDropArea = true; if (dropLoop == null) dropLoop = StartCoroutine(DropSequence()); }
    }

    public void Aktifet()
    {
        if (MoneyManager.Instance == null) return;

        if (MoneyManager.Instance.money < currentPrice)
        {
            Debug.LogWarning("Yetersiz bakiye! 100$ gerekli.");
            return;
        }

        MoneyManager.Instance.SpendMoney(currentPrice);

        if (butonKahve != null)
        {
            if (butonKahve.TryGetComponent<UnityEngine.UI.Button>(out var buttonComponent))
                buttonComponent.interactable = false;
            
            butonKahve.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => butonKahve.SetActive(false));
        }

        if (alan != null)
        {
            alan.SetActive(true);
            Vector3 originalPosition = alan.transform.localPosition;
            alan.transform.localPosition = originalPosition + Vector3.up * 1f;
            alan.transform.DOLocalMove(originalPosition, 0.6f).SetEase(Ease.OutBack);
        }

        if (buton != null)
        {
            buton.SetActive(true);
            buton.transform.localScale = Vector3.zero;
            buton.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }

        MusteriHareket.kahveAcik = true;
        if (StackCollector.Instance != null)
            StackCollector.Instance.KahveSatisiniAc();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("KahveAl")) { if (stackingLoop != null) { StopCoroutine(stackingLoop); stackingLoop = null; } }
        if (other.CompareTag(dropAreaTag)) { isInDropArea = false; if (dropLoop != null) { StopCoroutine(dropLoop); dropLoop = null; } }
    }

    void GuncelleUI()
    {
        if (stokText != null) 
            stokText.text = kahveStogu.ToString();
        
        if (hamKahveText != null) 
            hamKahveText.text = hamKahve + "/10";
    }

    public int StackCount => stack.Count;
    public int DropCount => dropList.Count;
}
