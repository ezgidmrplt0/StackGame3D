using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UrunTasiyici : MonoBehaviour
{
    [Header("Ayarlar")]
    public float speed = 12f;
    public int kapasite = 5;
    public float toplamaAraligi = 0.2f;

    [Header("Referanslar")]
    public GameObject urunPrefab;
    public Transform stackRoot;
    public float stackSpacing = 0.3f;
    public Transform stackAreaTarget;

    [Header("Çalışma Noktaları")]
    public Transform stackAlmaNoktasi;
    public Transform stackBirakmaNoktasi;

    [Header("Ham Çay Kontrolü")]
    public StackCollector stackCollector;
    public int gerekliHamCayMiktari = 1;

    [Header("Ürün Boyutu")]
    public Vector3 urunBoyutu = new Vector3(0.3f, 0.3f, 0.3f);

    [Header("Stack Ayarları")]
    public int stackLimit = 5;
    public float tweenDuration = 0.3f;
    public Ease tweenEase = Ease.OutBack;
    public float dropSpacing = 2f;

    private List<Transform> stack = new List<Transform>();
    private bool calisiyor = true;
    private bool isInDropArea = false;

    void Start()
    {
        if (stackCollector == null)
            stackCollector = FindObjectOfType<StackCollector>();

        StartCoroutine(CalismaRutini());
    }

    IEnumerator CalismaRutini()
    {
        while (calisiyor)
        {
            yield return StartCoroutine(Git(stackAlmaNoktasi.position));

            // Bekleme ve toplama döngüsü
            while (stack.Count < stackLimit && stackCollector.uretimStogu >= gerekliHamCayMiktari)
            {
                stackCollector.uretimStogu -= gerekliHamCayMiktari;
                AddUrun();
                yield return new WaitForSeconds(toplamaAraligi);
            }

            // Eğer hiç ürün yoksa ve stokta da yoksa bekle
            if (stack.Count == 0 && stackCollector.uretimStogu < gerekliHamCayMiktari)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            yield return StartCoroutine(Git(stackBirakmaNoktasi.position));
            isInDropArea = true;
            yield return StartCoroutine(DropSequence());
            isInDropArea = false;
        }
    }

    void AddUrun()
    {
        if (stack.Count >= stackLimit) return;
        if (urunPrefab == null || stackRoot == null) return;

        float yOffset = urunBoyutu.y * 0.5f;
        Vector3 spawnPosition = stackRoot.position + Vector3.up * (stackSpacing * stack.Count + yOffset);
        GameObject newCube = Instantiate(urunPrefab, spawnPosition, Quaternion.identity, stackRoot);

        Rigidbody cubeRb = newCube.GetComponent<Rigidbody>();
        if (cubeRb != null)
        {
            cubeRb.isKinematic = true;
            cubeRb.useGravity = false;
            cubeRb.drag = 10f;
        }

        Collider cubeCollider = newCube.GetComponent<Collider>();
        if (cubeCollider != null && cubeCollider is BoxCollider boxCollider)
            boxCollider.size = urunBoyutu * 0.9f;

        newCube.transform.localScale = Vector3.zero;
        newCube.transform.DOScale(urunBoyutu, tweenDuration).SetEase(tweenEase);
        stack.Add(newCube.transform);
    }

    IEnumerator DropSequence()
    {
        while (stack.Count > 0 && isInDropArea)
        {
            Transform cube = stack[stack.Count - 1];
            stack.RemoveAt(stack.Count - 1);

            Rigidbody cubeRb = cube.GetComponent<Rigidbody>();
            if (cubeRb != null)
            {
                cubeRb.isKinematic = false;
                cubeRb.useGravity = true;
                cubeRb.drag = 1f;
            }

            Collider cubeCollider = cube.GetComponent<Collider>();
            if (cubeCollider != null && cubeCollider is BoxCollider boxCollider)
                boxCollider.size = Vector3.one;

            cube.SetParent(null);
            int targetIndex = stackCollector.dropList.Count;
            Vector3 targetPos = stackCollector.stackAreaTarget.position + new Vector3(0f, dropSpacing * targetIndex, 0f);
            cube.DOJump(targetPos, 0.5f, 1, 0.4f).SetEase(Ease.OutQuad);

            stackCollector.dropList.Add(cube);
            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator Git(Vector3 hedef)
    {
        while (Vector3.Distance(transform.position, hedef) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, hedef, speed * Time.deltaTime);

            Vector3 dir = (hedef - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);

            yield return null;
        }
    }

    public void CalismayiBitir()
    {
        calisiyor = false;
        StopAllCoroutines();

        // Eğer üzerinde ürün varsa, önce onları bırak sonra yok ol
        if (stack.Count > 0)
        {
            StartCoroutine(UrunleriBirakVeYokOl());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator UrunleriBirakVeYokOl()
    {
        // Bırakma noktasına git
        if (stackBirakmaNoktasi != null)
        {
            yield return StartCoroutine(Git(stackBirakmaNoktasi.position));

            // Tüm ürünleri bırak
            isInDropArea = true;
            yield return StartCoroutine(DropSequence());
            isInDropArea = false;
        }

        // Tüm ürünler bırakıldıktan sonra yok ol
        Destroy(gameObject);
    }
}