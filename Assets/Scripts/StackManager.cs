using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class StackManager : MonoBehaviour
{
    [Header("Stack Ayarlarý")]
    public GameObject cubePrefab;        // Eklenecek küp prefabý
    public Transform stackParent;        // Küplerin ekleneceði kapsül veya boþ obje
    public float stackInterval = 0.5f;   // Küplerin eklenme aralýðý
    public float moveDuration = 0.3f;    // DOTween ile animasyon süresi

    [Header("Stack Silme Ayarlarý")]
    public Transform stackArea;          // Küplerin aktarýlacaðý hedef alan
    public float transferDuration = 0.5f; // Aktarma animasyon süresi
    public float transferDelay = 0.2f;    // Küplerin sýrayla gitmesi için gecikme

    private List<GameObject> stackList = new List<GameObject>();
    private bool stacking = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StackNoktasi0"))
        {
            stacking = true;
            StartCoroutine(AddCubesRoutine());
        }
        else if (other.CompareTag("StackSilmeNoktasi0"))
        {
            StartCoroutine(TransferCubesToAreaRoutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("StackNoktasi0"))
        {
            stacking = false;
        }
    }

    private IEnumerator AddCubesRoutine()
    {
        while (stacking)
        {
            AddCube();
            yield return new WaitForSeconds(stackInterval);
        }
    }

    private void AddCube()
    {
        GameObject newCube = Instantiate(cubePrefab, stackParent);

        // Küpün boyutunu yarýya indir
        newCube.transform.localScale = cubePrefab.transform.localScale * 0.5f;

        float height = newCube.transform.localScale.y;

        // Yeni küpün local pozisyonunu ayarla
        Vector3 localOffset = new Vector3(0, stackList.Count * height, -1f);
        newCube.transform.localPosition = Vector3.zero; // Baþlangýç noktasý stackParent merkezinde olsun

        // DOTween ile hedef local pozisyona taþý
        newCube.transform.DOLocalMove(localOffset, moveDuration).SetEase(Ease.OutQuad);

        stackList.Add(newCube);
    }

    private IEnumerator TransferCubesToAreaRoutine()
{
    int index = 0; // stackArea içindeki sýra

    for (int i = stackList.Count - 1; i >= 0; i--)
    {
        GameObject cube = stackList[i];
        cube.transform.SetParent(stackArea);

        // Bu küpün yüksekliðini al
        float height = cube.transform.localScale.y;

        // Hedef pozisyon: stackArea içinde sýrayla dizilsin
        Vector3 targetPos = new Vector3(0, index * height, 0);

        // DOTween ile hedef pozisyona taþý
        cube.transform.DOLocalMove(targetPos, transferDuration).SetEase(Ease.OutBounce);

        // Biraz bekle, sonra diðer küpü gönder
        yield return new WaitForSeconds(transferDelay);

        index++;
    }

    // Tüm küpler aktarýldý, listemizi temizle
    stackList.Clear();
}


}
