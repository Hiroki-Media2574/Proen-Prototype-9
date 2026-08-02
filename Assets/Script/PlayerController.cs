using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    public float moveSpeed = 0.0f;
    public float dashSpeed = 0.0f;
    public TextMeshProUGUI ActionText;
    public TextMeshProUGUI MessageText;
    public TextMeshProUGUI PickText;
    public TextMeshProUGUI SuccessText;
    public TextMeshProUGUI MPText;
    public GameObject bulletObject;
    private float Speed = 0.0f;
    int jumpCount = 0;
    private bool isGrounded = true;
    private bool isRangeNPC = false;
    private Renderer playerRenderer;
    private int haveBall = 0;
    private int success = 0;
    public static float MP = 100;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        Speed = moveSpeed;
        bulletObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)              //スペースを押すとジャンプする
        {
            rb.AddForce(Vector3.up * 7f, ForceMode.Impulse);
            isGrounded = false;
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))                        //左シフトを押している時
        {
            Speed = dashSpeed;                                          //Speedに走る速度dashSpeedを入れる。
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))                          //左シフトを押していない時
        {
            Speed = moveSpeed;                                          //Speedに歩く速度moveSpeedを入れる。
        }
        MPText.text = "MP  :  " + MP;
    }

    void FixedUpdate()
    {
        //Debug.Log("ok");
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
        if (isRangeNPC && Keyboard.current.eKey.isPressed)      //プレイヤーの距離内かつEキーが押されたとき
        {
            Debug.Log("E Pressed");
            ActionText.text = "";
            success++;
            MessageText.text = "Good job!";                     //成功のメッセージを出力
            SuccessText.text = "SUCCESS  :  " + success;        //成功した数をUIに反映
        }
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Action"))
        {
            ActionText.text = "Press E key!";
            isRangeNPC = true;
        }
        if (other.gameObject.CompareTag("Item"))
        {
            PickText.text = "Pick E key";
            isRangeNPC = false;
            if(Keyboard.current.eKey.isPressed)
            {
                PickText.text = "";
                MessageText.text = "";
                Destroy(other.gameObject);
                haveBall++;
                bulletObject.SetActive(true);
            }
            //isRangeNPC = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ActionText.text = "";
        MessageText.text = "";
    }
}
