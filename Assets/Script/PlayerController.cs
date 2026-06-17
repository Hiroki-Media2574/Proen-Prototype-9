using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    public float moveSpeed = 0.0f;
    public float dashSpeed = 0.0f;
    private float Speed = 0.0f;
    int jumpCount = 0;
    private bool isGrounded = true;
    private Renderer playerRenderer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        Speed = moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * 7f, ForceMode.Impulse);
            isGrounded = false;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Speed = dashSpeed;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            Speed = moveSpeed;
        }
    }

    void FixedUpdate()
    {
        Debug.Log("ok");
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        /*if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * 7f, ForceMode.Impulse);
            isGrounded = false;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            moveSpeed = 0.4f;
        }*/
        transform.Translate(h * Speed, 0, v * Speed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
