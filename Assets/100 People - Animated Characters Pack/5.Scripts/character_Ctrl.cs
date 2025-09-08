using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterWithAnim : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [Header("Hareket Ayarları")]
    public float walkSpeed = 2f;
    public float runSpeed = 4f;
    public float turnSpeed = 120f;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animator.speed = Random.Range(0.8f, 1.0f);
    }

    void Update()
    {
        // Mevcut hareket hızını al
        Vector3 horizontalVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        float speedMagnitude = horizontalVelocity.magnitude;

        // --- Animasyon Güncelle ---
        if (speedMagnitude < 0.1f)
        {
            animator.SetInteger("move", 0); // Idle
        }
        else if (speedMagnitude < walkSpeed + 0.1f)
        {
            animator.SetInteger("move", 1); // Walk
        }
        else
        {
            animator.SetInteger("move", 2); // Run
        }

        // --- Dönüş Animasyonu ---
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);
            animator.SetInteger("head_turn", 1);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);
            animator.SetInteger("head_turn", 2);
        }
        else
        {
            animator.SetInteger("head_turn", 0);
        }

        // --- Diğer Trigger Animasyonlar ---
        if (Input.GetKeyDown(KeyCode.Alpha1)) animator.SetTrigger("forward_fall");
        if (Input.GetKeyDown(KeyCode.Alpha2)) animator.SetTrigger("backward_fall");
        if (Input.GetKeyDown(KeyCode.Alpha3)) animator.SetTrigger("sitting");
        if (Input.GetKeyDown(KeyCode.Alpha4)) animator.SetTrigger("sitting_hand_up");
        if (Input.GetKeyDown(KeyCode.Alpha5)) animator.SetTrigger("happy_dance");
        if (Input.GetKeyDown(KeyCode.Alpha6)) animator.SetTrigger("happy_dance_2");
        if (Input.GetKeyDown(KeyCode.Alpha7)) animator.SetTrigger("jump");
        if (Input.GetKeyDown(KeyCode.Alpha8)) animator.SetTrigger("hands_on_head");
        if (Input.GetKeyDown(KeyCode.Alpha9)) animator.SetTrigger("lying");
        if (Input.GetKeyDown(KeyCode.Alpha0)) animator.SetTrigger("on_all_fours");
    }
}
