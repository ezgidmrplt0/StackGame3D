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

    [Header("Büyütülebilir Ölçek")]
    public Vector3 sodaTargetScale = new Vector3(0.003f, 0.003f, 0.003f); // Inspector’dan değiştirebilirsin

    [Header("Bırakma Ayarları")]
    public Transform sodaDropTarget;  // Sodaların bırakılacağı yer
    public float dropSpacing = 0.002f; // Bırakılan sodaların arası mesafe

    private List<Transform> sodaStack = new List<Transform>();
    private bool canCollect = false;
    private bool isInDropArea = false;
    private Coroutine collectRoutine;
    private Coroutine dropRoutine;

    private void Update()
    {
        UpdateStackPositions();
    }

    private void UpdateStackPositions()
    {
        for (int i = 0; i < sodaStack.Count; i++)
        {
            Transform soda = sodaStack[i];
            // Üst üste stack
            Vector3 targetPos = stackRoot.position + Vector3.up * cubeHeight * i;
            soda.position = Vector3.Lerp(soda.position, targetPos, Time.deltaTime * 10f);
            soda.rotation = Quaternion.identity;
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

    private void AddSoda()
    {
        Vector3 spawnPos = stackRoot.position + Vector3.up * (cubeHeight * sodaStack.Count);
        GameObject newSoda = Instantiate(sodaPrefab, spawnPos, Quaternion.identity);

        newSoda.transform.localScale = Vector3.zero;
        newSoda.transform.SetParent(stackRoot);

        // Target scale artık public, Inspector’dan değiştirebilirsin
        newSoda.transform.DOScale(sodaTargetScale, tweenDuration).SetEase(tweenEase);

        sodaStack.Add(newSoda.transform);
    }

    private IEnumerator DropSodasRoutine()
    {
        while (isInDropArea && sodaStack.Count > 0)
        {
            Transform soda = sodaStack[sodaStack.Count - 1];
            sodaStack.RemoveAt(sodaStack.Count - 1);
            soda.SetParent(null);

            // StackCollector'ın dropList'ine ekle
            StackCollector.Instance.AddToDropList(soda);

            // Soda'ya bir tag veya component ekleyerek farklı olduğunu belirt
            soda.tag = "SodaProduct";
            if (soda.GetComponent<SodaProduct>() == null)
                soda.gameObject.AddComponent<SodaProduct>(); // Yeni component

            int dropIndex = StackCollector.Instance.DropCount;
            Vector3 targetPos = sodaDropTarget.position + Vector3.up * (cubeHeight * dropIndex);

            soda.DOJump(targetPos, 0.002f, 1, 0.4f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => { soda.rotation = Quaternion.identity; });

            yield return new WaitForSeconds(0.1f);
        }
    }
}

// Yeni Component: Soda ürünlerini tanımlamak için
public class SodaProduct : MonoBehaviour
{
    public int price = 2; // Soda fiyatı
}
