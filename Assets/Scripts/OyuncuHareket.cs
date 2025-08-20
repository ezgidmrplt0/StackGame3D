using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OyuncuHareket : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private Rigidbody rb;
    private bool isGrounded;

    [Header("Stack Ayarları")]
    public GameObject cubePrefab;
    public Transform stackParent;
    public Transform stackArea2;
    private List<GameObject> stackList = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(moveX, 0, moveZ) * moveSpeed;
        rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);

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
            InvokeRepeating("AddCubeToStack", 0f, 0.5f);
        }

        if (collision.gameObject.CompareTag("StackSilmeNoktasi0"))
        {
            CancelInvoke("AddCubeToStack");
            StartCoroutine(TransferStackToArea2()); // animasyonlu aktarım
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("StackNoktasi0"))
        {
            CancelInvoke("AddCubeToStack");
        }
    }

    void AddCubeToStack()
    {
        GameObject newCube = Instantiate(cubePrefab);
        float height = cubePrefab.transform.localScale.y; // küp yüksekliği
        Vector3 offset = new Vector3(0, stackList.Count * height, -1f);
        newCube.transform.position = stackParent.position + offset;
        newCube.transform.SetParent(stackParent);
        stackList.Add(newCube);

        Debug.Log("Yeni küp eklendi! Stack boyutu: " + stackList.Count);
    }

    IEnumerator TransferStackToArea2()
    {
        Debug.Log("Küpler animasyonla yeni alana aktarılıyor...");

        float height = cubePrefab.transform.localScale.y;

        for (int i = stackList.Count - 1; i >= 0; i--) // üstten başla
        {
            GameObject cube = stackList[i];

            // Hedef pozisyon: en alttan başla
            Vector3 targetPos = stackArea2.position + new Vector3(0, (stackList.Count - 1 - i) * height, 0);

            // Animasyonik geçiş
            float t = 0f;
            Vector3 startPos = cube.transform.position;

            while (t < 1f)
            {
                t += Time.deltaTime * 2f; // hız
                cube.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            cube.transform.position = targetPos;
            cube.transform.SetParent(stackArea2);
        }

        stackList.Clear();
    }
}
