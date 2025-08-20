using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OyuncuHareket : MonoBehaviour
{
    [Header("Oyuncu Ayarları")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private Rigidbody rb;
    private bool isGrounded;

    [Header("Stack Ayarları")]
    public GameObject cubePrefab;
    public Transform stackParent;
    public Transform stackArea2;

    private List<GameObject> stackList = new List<GameObject>();
    private List<GameObject> area2List = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Oyuncu hareketleri
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed;
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);

        // Zıplama
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag("StackNoktasi0"))
        {
            InvokeRepeating(nameof(AddCubeToStack), 0f, 0.5f);
        }

        if (collision.gameObject.CompareTag("StackSilmeNoktasi0"))
        {
            CancelInvoke(nameof(AddCubeToStack));
            StartCoroutine(TransferStackToArea2()); // animasyonlu aktarım
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("StackNoktasi0"))
        {
            CancelInvoke(nameof(AddCubeToStack));
        }
    }

    void AddCubeToStack()
    {
        GameObject newCube = Instantiate(cubePrefab);

        float height = cubePrefab.transform.localScale.y;

        // pivot ortada olduğu için +height/2 ekliyoruz
        Vector3 offset = new Vector3(0, (stackList.Count * height) + height / 2f, -1f);

        newCube.transform.position = stackParent.position + offset;
        newCube.transform.SetParent(stackParent);
        stackList.Add(newCube);

        Debug.Log("Yeni küp eklendi! Stack boyutu: " + stackList.Count);
    }

    IEnumerator TransferStackToArea2()
    {
        Debug.Log("Küpler animasyonla yeni alana aktarılıyor...");

        float height = cubePrefab.transform.localScale.y;

        for (int i = stackList.Count - 1; i >= 0; i--)
        {
            GameObject cube = stackList[i];

            Vector3 targetPos;
            if (area2List.Count == 0)
            {
                // İlk küp → stackArea2'nin pozisyonu (world space)
                targetPos = stackArea2.position;
            }
            else
            {
                // Son eklenen küpün üstüne koy
                GameObject lastCube = area2List[area2List.Count - 1];
                targetPos = lastCube.transform.position + new Vector3(0, height, 0);
            }

            // Animasyon
            float t = 0f;
            Vector3 startPos = cube.transform.position;

            while (t < 1f)
            {
                t += Time.deltaTime * 4f;
                cube.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            cube.transform.position = targetPos;

            // Parent’e ekle ve lokal pozisyonu sıfırla
            cube.transform.SetParent(stackArea2);
            cube.transform.localPosition = new Vector3(cube.transform.localPosition.x, cube.transform.localPosition.y, cube.transform.localPosition.z);

            area2List.Add(cube);
        }

        stackList.Clear();
    }
}