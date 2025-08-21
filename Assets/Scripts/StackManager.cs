using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UIElements;

public class StackManager : MonoBehaviour
{
    [Header("Stack Ayarları")]
    public GameObject cubePrefab;
    public Transform stackParent;
    public float stackInterval = 0.5f;
    public float moveDuration = 0.3f;

    [Header("Stack Silme Ayarları")]
    public Transform stackArea;
    public float transferDuration = 0.5f;
    public float transferDelay = 0.2f;

    [Header("Küp Boyut & Aralık")]
    public float cubeScale = 1.0f;   // Hamburgerlerin büyüklüğü
    public float cubeHeight = 0.5f;  // Küpler arası yükseklik (public yaptık)

    public List<GameObject> stackList = new List<GameObject>();       // Oyuncu üzerindeki küpler
    public List<GameObject> areaStackList = new List<GameObject>();   // Area2'deki küpler
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

        // Hamburger boyutunu Inspector’dan ayarlayabilirsin
        newCube.transform.localScale = cubePrefab.transform.localScale * cubeScale;

        Vector3 localOffset = new Vector3(0, stackList.Count * cubeHeight, -1f);
        newCube.transform.localPosition = Vector3.zero;

        newCube.transform.DOLocalMove(localOffset, moveDuration).SetEase(Ease.OutQuad);

        stackList.Add(newCube);
    }

    private IEnumerator TransferCubesToAreaRoutine()
    {
        float currentY = 0f;

        if (areaStackList.Count > 0)
        {
            GameObject lastCube = areaStackList[areaStackList.Count - 1];
            currentY = lastCube.transform.localPosition.y + cubeHeight + 0.05f;
        }

        for (int i = 0; i < stackList.Count; i++)
        {
            GameObject cube = stackList[i];
            cube.transform.SetParent(stackArea);

            Vector3 targetPos = new Vector3(0, currentY, 0);
            cube.transform.DOLocalMove(targetPos, transferDuration).SetEase(Ease.OutBounce);

            areaStackList.Add(cube);

            currentY += cubeHeight + 0.05f;
            yield return new WaitForSeconds(transferDelay);
        }

        stackList.Clear();
    }
}
