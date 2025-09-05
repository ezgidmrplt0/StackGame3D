using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UrunTasiyici : MonoBehaviour
{
    [Header("Ayarlar")]
    public float speed = 12f;
    public int kapasite = 5; // Bir seferde kaç ürün taşıyacak
    public float toplamaAraligi = 0.2f;

    [Header("Referanslar")]
    public GameObject urunPrefab;       // Taşınacak prefab
    public Transform stackRoot;         // Sırtında stacklenecek yer
    public float stackSpacing = 0.3f;
    public Transform stackAreaTarget;   // Bırakma alanı (raf gibi)

    [Header("Çalışma Noktaları")]
    public Transform stackAlmaNoktasi;   // "StackNoktasi0"
    public Transform stackBirakmaNoktasi; // "StackSilmeNoktasi0"

    private List<Transform> stack = new List<Transform>();
    private bool calisiyor = true;

    void Start()
    {
        StartCoroutine(CalismaRutini());
    }

    IEnumerator CalismaRutini()
    {
        while (calisiyor)
        {
            // 1. Alma noktasına git
            yield return StartCoroutine(Git(stackAlmaNoktasi.position));

            // 2. 5 ürün stackle
            for (int i = 0; i < kapasite; i++)
            {
                AddUrun();
                yield return new WaitForSeconds(toplamaAraligi);
            }

            // 3. Bırakma noktasına git
            yield return StartCoroutine(Git(stackBirakmaNoktasi.position));

            // 4. Ürünleri bırak
            for (int i = stack.Count - 1; i >= 0; i--)
            {
                Transform urun = stack[i];
                stack.RemoveAt(i);

                Vector3 hedefPos = stackAreaTarget.position + Vector3.up * (0.5f * i);
                urun.SetParent(null);
                urun.DOJump(hedefPos, 0.5f, 1, 0.4f).OnComplete(() =>
                {
                    urun.SetParent(stackAreaTarget);
                });
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    IEnumerator Git(Vector3 hedef)
    {
        while (Vector3.Distance(transform.position, hedef) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, hedef, speed * Time.deltaTime);

            // Karakterin yönünü dönmesi için
            Vector3 dir = (hedef - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);

            yield return null;
        }
    }

    void AddUrun()
    {
        if (urunPrefab == null || stackRoot == null) return;
        Vector3 offset = Vector3.up * stackSpacing * stack.Count;
        GameObject yeniUrun = Instantiate(urunPrefab, stackRoot.position + offset, Quaternion.identity, stackRoot);
        yeniUrun.transform.localScale = Vector3.zero;
        yeniUrun.transform.DOScale(Vector3.one * 0.3f, 0.3f).SetEase(Ease.OutBack);
        stack.Add(yeniUrun.transform);
    }

    public void CalismayiBitir()
    {
        calisiyor = false;
        StopAllCoroutines();
        Destroy(gameObject);
    }
}
