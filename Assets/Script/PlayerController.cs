using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    public float moveSpeed = 5.0f; // TranslateにTime.deltaTimeを掛けるため大きめに設定
    public float dashSpeed = 10.0f;

    [Header("Camera & Mouse Settings")]
    public Transform cameraTransform; // Main Cameraをドラッグ＆ドロップ
    public float mouseSensitivity = 2.0f; // マウス感度
    private float xRotation = 0.0f; // 上下の回転角度制限用

    [Header("Shooting Settings")]
    public GameObject bulletPrefab;     // 発射するボールのプレハブ
    public Transform spawnPoint;        // ボールが出現する位置
    public float bulletSpeed = 20.0f;   // ボールを飛ばすスピード
    public string targetTag = "Target"; // 狙うターゲットのタグ名

    public TextMeshProUGUI ActionText;
    public TextMeshProUGUI MessageText;
    public TextMeshProUGUI PickText;
    public TextMeshProUGUI SuccessText;
    public TextMeshProUGUI MPText;
    public TextMeshProUGUI BallCountText;
    public GameObject bulletObject;     // 手元のボール（見た目用）
    private GameObject currentItem = null;
    private float Speed = 0.0f;
    int jumpCount = 0;
    private bool isGrounded = true;
    private bool isRangeNPC = false;
    private bool isRangeItem = false;
    private Renderer playerRenderer;
    private int haveBall = 0;
    private int success = 0;
    public static float MP = 100;

    // sound
    AudioSource ongen;
    private AudioSource footstepAudio;
    [SerializeField] private AudioClip SeikouSE;
    [SerializeField] private AudioClip JampSE;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        Speed = moveSpeed;
        bulletObject.SetActive(false);

        // 物理衝突によるプレイヤーの回転・転倒をコード側でも防止
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        // マウスカーソルを中央に固定して非表示にする
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // sound
        ongen = GetComponent<AudioSource>();
        footstepAudio = gameObject.AddComponent<AudioSource>();
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
        BallCountText.text = "BALL  :  " + haveBall;

        // 3. Eキーのアクション判定（NPC用）
        if (isRangeNPC && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("E Pressed");
            ActionText.text = "";
            AddSuccessCount(); // スコア加算と表示更新
            MessageText.text = "Good job!";
            ongen.PlayOneShot(SeikouSE);
        }

        // 4. Eキーのアクション判定（アイテム拾い用）
        if (isRangeItem && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("E Pressed");
            PickText.text = "";
            MessageText.text = "";
            if (currentItem != null)
            {
                Destroy(currentItem);
                currentItem = null;
                haveBall++;
                bulletObject.SetActive(true);
            }
            isRangeItem = false;
        }

        // 5. マウス左クリックによるボール発射処理
        if (Mouse.current.leftButton.wasPressedThisFrame && haveBall > 0)
        {
            ShootBall();
        }

        // Escキーでカーソルロック解除（動作確認・デバッグ用）
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    /// <summary>
    /// NPC等が成功数（SuccessCount）を加算してUIに反映させるための外部用メソッド
    /// </summary>
    public void AddSuccessCount()
    {
        success++;
        if (SuccessText != null)
        {
            SuccessText.text = "SUCCESS  :  " + success;
        }
        if (ongen != null && SeikouSE != null)
        {
            ongen.PlayOneShot(SeikouSE);
        }
    }

    /// <summary>
    /// ボール（弾）を生成して最も近い Target へ向けて打ち出す処理
    /// </summary>
    private void ShootBall()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("bulletPrefab が設定されていません。Inspectorでセットしてください。");
            return;
        }

        Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.position : cameraTransform.position + cameraTransform.forward * 0.5f;
        Quaternion spawnRot = (spawnPoint != null) ? spawnPoint.rotation : cameraTransform.rotation;

        // ボールオブジェクトを生成
        GameObject ball = Instantiate(bulletPrefab, spawnPos, spawnRot);

        // プレイヤー自身と生成したボールの物理衝突を無視
        Collider ballCollider = ball.GetComponent<Collider>();
        Collider[] playerColliders = GetComponentsInChildren<Collider>();
        if (ballCollider != null)
        {
            foreach (Collider playerCol in playerColliders)
            {
                if (playerCol != ballCollider)
                {
                    Physics.IgnoreCollision(ballCollider, playerCol);
                }
            }
        }

        // シーン内の "Target" タグの中で最も距離が近いものを検索
        Transform closestTarget = GetClosestTarget();

        // 生成したボールに HomingBullet スクリプトが付いているかチェック
        HomingBullet homing = ball.GetComponent<HomingBullet>();
        if (homing != null)
        {
            homing.target = closestTarget;
            homing.speed = bulletSpeed;
        }
        else
        {
            Rigidbody ballRb = ball.GetComponent<Rigidbody>();
            if (ballRb != null)
            {
                Vector3 shootDirection = cameraTransform.forward;
                if (closestTarget != null)
                {
                    shootDirection = (closestTarget.position - spawnPos).normalized;
                }
                ballRb.linearVelocity = shootDirection * bulletSpeed;
            }
        }

        Destroy(ball, 5.0f);

        haveBall--;
        if (haveBall <= 0)
        {
            bulletObject.SetActive(false);
        }
    }

    /// <summary>
    /// シーン内の "Target" タグを持つオブジェクトの中からプレイヤーに一番近いものを探すメソッド
    /// </summary>
    private Transform GetClosestTarget()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);
        Transform closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;

        foreach (GameObject targetObj in targets)
        {
            float distance = Vector3.Distance(targetObj.transform.position, currentPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = targetObj.transform;
            }
        }

        return closest;
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = transform.right * h + transform.forward * v;
        rb.MovePosition(rb.position + moveDir * Speed * Time.fixedDeltaTime);

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
            isRangeItem = false;
        }
        if (other.gameObject.CompareTag("Item"))
        {
            currentItem = other.gameObject;
            PickText.text = "Pick E key";
            isRangeNPC = false;
            isRangeItem = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ActionText.text = "";
        MessageText.text = "";
    }
}