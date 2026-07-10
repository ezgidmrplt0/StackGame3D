using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SodaStack : MonoBehaviour
{
    [Header("Soda Ayarları")]
    public GameObject sodaPrefab;
    public Transform stackRoot;
    public float cubeHeight = 0.005f;
    public float tweenDuration = 0.3f;
    public Ease tweenEase = Ease.OutCubic;
    public int maxStack = 10;
    public float spawnDelay = 0.4f;
    public float stackSpacingMultiplier = 1.6f;

    [Header("Büyütülebilir Ölçek")]
    public Vector3 sodaTargetScale = new Vector3(0.0025f, 0.0025f, 0.0025f);

    [Header("Bırakma Ayarları")]
    public Transform sodaDropTarget;
    public float dropSpacing = 0.002f;

    // Soda listesi (StackCollector dropList'ten bağımsız)
    public List<Transform> sodaStack = new List<Transform>();
    public List<Transform> sodaDropList = new List<Transform>();

    private bool canCollect = false;
    private bool isInDropArea = false;
    private Coroutine collectRoutine;
    private Coroutine dropRoutine;

    // Singleton
    public static SodaStack Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (sodaTargetScale == Vector3.zero)
        {
            sodaTargetScale = new Vector3(0.0025f, 0.0025f, 0.0025f);
        }

        // Auto-calculate stack spacing from mesh bounds
        if (sodaPrefab != null)
        {
            Renderer r = sodaPrefab.GetComponentInChildren<Renderer>();
            if (r != null)
            {
                float meshHeight = r.bounds.size.y;
                if (meshHeight > 0.01f)
                {
                    cubeHeight = meshHeight * sodaTargetScale.y;
                    dropSpacing = cubeHeight;
                }
            }
        }

        // Ensure stack spacing is not too small (nested sodas)
        if (cubeHeight < 0.05f)
        {
            cubeHeight = 0.25f;
            dropSpacing = 0.25f;
        }
    }

    void Update()
    {
        UpdateStackPositions();
        UpdateDropListPositions();
    }

    public Vector3 GetPlayerSodaWorldScale()
    {
        if (stackRoot != null)
        {
            return Vector3.Scale(sodaTargetScale, stackRoot.lossyScale);
        }
        return sodaTargetScale;
    }

    public float GetPlayerSodaWorldSpacing()
    {
        if (stackRoot != null)
        {
            return cubeHeight * stackRoot.lossyScale.y * stackSpacingMultiplier;
        }
        return cubeHeight * stackSpacingMultiplier;
    }

    private void UpdateStackPositions()
    {
        float currentSpacing = GetPlayerSodaWorldSpacing();
        for (int i = 0; i < sodaStack.Count; i++)
        {
            Transform soda = sodaStack[i];
            Vector3 targetPos = stackRoot.position + Vector3.up * currentSpacing * i;
            soda.position = Vector3.Lerp(soda.position, targetPos, Time.deltaTime * 10f);
            soda.rotation = Quaternion.identity;
            soda.localScale = sodaTargetScale;
        }
    }

    private void UpdateDropListPositions()
    {
        for (int i = sodaDropList.Count - 1; i >= 0; i--)
        {
            if (sodaDropList[i] == null)
            {
                sodaDropList.RemoveAt(i);
            }
        }

        Vector3 targetScale = GetPlayerSodaWorldScale();
        float currentSpacing = GetPlayerSodaWorldSpacing();

        for (int i = 0; i < sodaDropList.Count; i++)
        {
            Transform soda = sodaDropList[i];
            if (soda != null)
            {
                if (!DOTween.IsTweening(soda))
                {
                    Vector3 targetPos = sodaDropTarget.position + Vector3.up * (currentSpacing * (i + 0.5f));
                    soda.position = Vector3.Lerp(soda.position, targetPos, Time.deltaTime * 10f);
                    soda.rotation = Quaternion.identity;
                }
                soda.localScale = targetScale;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SodaNoktasi"))
        {
            if (!canCollect)
            {
                canCollect = true;
                collectRoutine = StartCoroutine(CollectSodaRoutine());
            }
        }

        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            isInDropArea = true;
            if (dropRoutine == null)
                dropRoutine = StartCoroutine(DropSodasRoutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SodaNoktasi"))
        {
            canCollect = false;
            if (collectRoutine != null)
                StopCoroutine(collectRoutine);
        }

        if (other.CompareTag("StackSilmeNoktasi0"))
        {
            isInDropArea = false;
            if (dropRoutine != null)
            {
                StopCoroutine(dropRoutine);
                dropRoutine = null;
            }
        }
    }

    private IEnumerator CollectSodaRoutine()
    {
        while (canCollect && sodaStack.Count < maxStack)
        {
            AddSoda();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    public void AddSoda()
    {
        Vector3 spawnPos = stackRoot.position + Vector3.up * (GetPlayerSodaWorldSpacing() * sodaStack.Count);
        GameObject newSoda = Instantiate(sodaPrefab, spawnPos, Quaternion.identity);

        foreach (var rb in newSoda.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        foreach (var col in newSoda.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }

        newSoda.transform.localScale = Vector3.zero;
        newSoda.transform.SetParent(stackRoot);
        newSoda.transform.DOScale(sodaTargetScale, tweenDuration).SetEase(tweenEase);

        sodaStack.Add(newSoda.transform);
    }

    public IEnumerator DropSodasRoutine()
    {
        while (isInDropArea && sodaStack.Count > 0)
        {
            Transform soda = sodaStack[sodaStack.Count - 1];
            sodaStack.RemoveAt(sodaStack.Count - 1);
            soda.SetParent(null);

            sodaDropList.Add(soda);
            soda.tag = "SodaProduct";

            if (soda.GetComponent<SodaProduct>() == null)
                soda.gameObject.AddComponent<SodaProduct>();

            int dropIndex = sodaDropList.Count - 1;
            float currentSpacing = GetPlayerSodaWorldSpacing();
            Vector3 targetPos = sodaDropTarget.position + Vector3.up * (currentSpacing * (dropIndex + 0.5f));

            soda.DOJump(targetPos, currentSpacing * 0.5f, 1, 0.4f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    soda.rotation = Quaternion.identity;
                });

            yield return new WaitForSeconds(0.1f);
        }
    }

    public bool SellSodaProduct()
    {
        if (sodaDropList.Count == 0) return false;

        Transform soda = sodaDropList[sodaDropList.Count - 1];
        sodaDropList.RemoveAt(sodaDropList.Count - 1);
        Destroy(soda.gameObject);

        return true;
    }

    public int SodaDropCount => sodaDropList.Count;

    // Yeni eklenen fonksiyonlar: StackCollector pivot değişimi için
    public int SodaStackCount => sodaStack.Count;

    public Transform GetSodaAt(int index)
    {
        if (index < 0 || index >= sodaStack.Count) return null;
        return sodaStack[index];
    }
}

// Soda ürünü tanımlama
public class SodaProduct : MonoBehaviour
{
    public int price = 2;
}