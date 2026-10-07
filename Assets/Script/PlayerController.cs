using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    public float moveSpeed = 5.0f; // ※TranslateにTime.deltaTimeを掛けるため、値を少し大きめに
    public float dashSpeed = 10.0f;

    [Header("Camera & Mouse Settings")]
    public Transform cameraTransform; // Main Cameraをドラッグ＆ドロップ
    public float mouseSensitivity = 2.0f; // マウス感度
    private float xRotation = 0.0f; // 上下の回転角度制限用

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

    // sound
    AudioSource ongen;
    private AudioSource footstepAudio;
    [SerializeField] private AudioClip SeikouSE;
    [SerializeField] private AudioClip JampSE;
    [SerializeField] private AudioClip asioto1;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        Speed = moveSpeed;
        bulletObject.SetActive(false);

        // マウスカーソルを中央に固定して非表示にする
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // sound
        ongen = GetComponent<AudioSource>();
        footstepAudio = gameObject.AddComponent<AudioSource>();
        footstepAudio.clip = asioto1;
        footstepAudio.loop = true;
        footstepAudio.playOnAwake = false;

        // cameraTransformが未設定の場合はMain Cameraを自動取得
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // 1. マウス操作による視点回転処理
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // プレイヤー全体の左右回転 (Y軸)
        transform.Rotate(Vector3.up * mouseX);

        // カメラのみの上下回転 (X軸) - 上下90度制限
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // 2. 入力・ジャンプ・ダッシュ等の更新処理
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

        MPText.text = "MP  :  " + MP;

        // 3. Eキーのアクション判定（Updateで行うと押し損ねを防げます）
        if (isRangeNPC && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("E Pressed");
            ActionText.text = "";
            success++;
            MessageText.text = "Good job!";
            SuccessText.text = "SUCCESS  :  " + success;
            ongen.PlayOneShot(SeikouSE);
        }

        // Escキーでカーソルロック解除（動作確認・デバッグ用）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // プレイヤーの向いている方向（ローカル座標）基準で移動する
        Vector3 moveDir = transform.right * h + transform.forward * v;
        rb.MovePosition(rb.position + moveDir * Speed * Time.fixedDeltaTime);

        // 足音の再生制御
        if ((h != 0 || v != 0) && isGrounded)
        {
            if (!footstepAudio.isPlaying)
            {
                footstepAudio.Play();
            }
        }
        else
        {
            if (footstepAudio.isPlaying)
            {
                footstepAudio.Stop();
            }
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
            if (Keyboard.current.eKey.isPressed)
            {
                PickText.text = "";
                MessageText.text = "";
                Destroy(other.gameObject);
                haveBall++;
                bulletObject.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ActionText.text = "";
        MessageText.text = "";
    }
}