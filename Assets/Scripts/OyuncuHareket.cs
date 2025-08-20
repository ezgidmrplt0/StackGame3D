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
        // Oyuncu hareketleri - Kameraya göre relatif hale getirdik
        float moveX = Input.GetAxis("Horizontal");  // A/D için sağ/sol
        float moveZ = Input.GetAxis("Vertical");    // W/S için ileri/geri

        // Kameranın forward ve right vektörlerini al (y=0 için yatay tut)
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = Camera.main.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        // Hareket vektörü: kameraya göre hesapla
        Vector3 move = (camRight * moveX + camForward * moveZ) * moveSpeed;

        // Velocity'i uygula, y bileşenini koru (zıplama için)
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);

        // Zıplama (değişmedi)
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
            StartCoroutine(TransferStackToArea2());
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

        // Yeni küpün pozisyonu: stackParent'ın dünya pozisyonuna göre
        Vector3 offset = new Vector3(0, (stackList.Count * height) + (height / 2f), -1f);
        newCube.transform.position = stackParent.position + offset;
        newCube.transform.SetParent(stackParent);
        stackList.Add(newCube);

        Debug.Log($"Yeni küp eklendi! Stack boyutu: {stackList.Count}, Pozisyon: {newCube.transform.position}");
    }

    IEnumerator TransferStackToArea2()
    {
        Debug.Log("Küpler animasyonla yeni alana aktarılıyor...");

        float height = cubePrefab.transform.localScale.y;

        // Ters sırayla aktar (en üstteki küpten başla)
        for (int i = stackList.Count - 1; i >= 0; i--)
        {
            GameObject cube = stackList[i];

            // Hedef pozisyonu hesapla
            Vector3 targetPos;
            if (area2List.Count == 0)
            {
                // İlk küp: stackArea2'nin pozisyonuna, pivot ortada olduğu için +height/2
                targetPos = stackArea2.position + Vector3.up * (height / 2f);
            }
            else
            {
                // Son eklenen küpün üstüne bitişik
                GameObject lastCube = area2List[area2List.Count - 1];
                targetPos = lastCube.transform.position + Vector3.up * height;
            }

            // Animasyon
            float t = 0f;
            Vector3 startPos = cube.transform.position;

            while (t < 1f)
            {
                t += Time.deltaTime * 2f;
                cube.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            // Son pozisyonu kesinleştir
            cube.transform.position = targetPos;
            cube.transform.SetParent(stackArea2);
            area2List.Add(cube);

            Debug.Log($"Küp aktarıldı! area2List boyutu: {area2List.Count}, Pozisyon: {cube.transform.position}");
        }

        // stackList'i sıfırla
        stackList.Clear();
        Debug.Log("stackList sıfırlandı.");
    }
}