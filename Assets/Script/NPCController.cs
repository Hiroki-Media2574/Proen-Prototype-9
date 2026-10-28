using UnityEngine;
using TMPro;

public class NPCController : MonoBehaviour
{
    // ターゲット（プレイヤー）のTransform
    public Transform target;
    // 視界の届く最大距離
    public float viewDistance = 10.0f;
    // 視界の角度（例: 90度）
    [Range(0, 360)]
    public float viewAngle = 90.0f;
    // 視線遮断用の障害物レイヤー（Wallなど）
    public LayerMask obstacleMask;

    [Header("うろうろ（徘徊）設定")]
    public float moveSpeed = 2.0f;
    public float moveDuration = 3.0f;
    public float waitDuration = 2.0f;

    [Header("視界の可視化設定")]
    public MeshFilter viewMeshFilter;
    public int meshResolution = 30;

    [Header("NPCの腕用オブジェクト設定")]
    public GameObject rightArm;
    public GameObject leftArm;

    public enum RotateAxis { XAxis, YAxis, ZAxis }
    [Header("腕を上げる回転設定")]
    public RotateAxis armRotateAxis = RotateAxis.XAxis;
    public float raiseAngle = -180f;
    public float armKeepDuration = 1.5f;

    public TextMeshProUGUI MessageText;

    private Mesh viewMesh;
    private float stateTimer = 0.0f;
    private bool isMoving = false;

    private Transform fallenTargetTransform = null;

    private Quaternion initialRightArmRotation;
    private Quaternion initialLeftArmRotation;
    private float armResetTimer = 0.0f;
    private bool isArmsRaised = false;

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";

        if (viewMeshFilter != null)
        {
            viewMeshFilter.mesh = viewMesh;
        }

        // 腕の初期ローカル回転を保存
        if (rightArm != null) initialRightArmRotation = rightArm.transform.localRotation;
        if (leftArm != null) initialLeftArmRotation = leftArm.transform.localRotation;

        SetNewRandomDirection();
    }

    void Update()
    {
        // バンザイ状態のタイマー管理
        if (isArmsRaised)
        {
            armResetTimer += Time.deltaTime;
            if (armResetTimer >= armKeepDuration)
            {
                isArmsRaised = false; // 時間経過でバンザイ解除
            }
        }

        // 1. プレイヤーが視界内にいる場合
        if (target != null && IsTargetInSight())
        {
            Vector3 targetPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
            transform.LookAt(targetPosition);
            PlayerController.MP -= Time.deltaTime * 4;
            return;
        }

        // 2. 落とされたTargetがあるか探索
        FindFallenTarget();

        // 3. 追尾・移動
        if (fallenTargetTransform != null)
        {
            MoveToFallenTarget();
        }
        else
        {
            WanderBehavior();
        }
    }

    /// <summary>
    /// アニメーション処理の後に実行されるLateUpdateで腕の回転を強制反映
    /// </summary>
    void LateUpdate()
    {
        // 1. 視界メッシュの描画
        DrawFieldOfView();

        // 2. バンザイ状態の場合は Animator の上書き後に腕の回転を強制適用
        if (isArmsRaised)
        {
            ApplyRaiseArmRotation();
        }
    }

    private void FindFallenTarget()
    {
        GameObject fallenObj = GameObject.Find("FallenTarget");

        if (fallenObj == null)
        {
            fallenTargetTransform = null;
            return;
        }

        NPCController[] allNPCs = FindObjectsByType<NPCController>(FindObjectsSortMode.None);
        NPCController closestNPC = null;
        float minDistance = Mathf.Infinity;

        foreach (NPCController npc in allNPCs)
        {
            float distance = Vector3.Distance(npc.transform.position, fallenObj.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestNPC = npc;
            }
        }

        if (closestNPC == this)
        {
            fallenTargetTransform = fallenObj.transform;
        }
        else
        {
            fallenTargetTransform = null;
        }
    }

    private void MoveToFallenTarget()
    {
        if (fallenTargetTransform == null) return;

        Vector3 targetPos = new Vector3(fallenTargetTransform.position.x, transform.position.y, fallenTargetTransform.position.z);
        transform.LookAt(targetPos);
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        float distanceToTarget = Vector3.Distance(transform.position, fallenTargetTransform.position);

        if (distanceToTarget <= 3.0f)
        {
            CollectTarget(fallenTargetTransform.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "FallenTarget" || collision.gameObject.CompareTag("Target"))
        {
            CollectTarget(collision.gameObject);
        }
    }

    private void CollectTarget(GameObject targetObject)
    {
        if (targetObject == null) return;

        Debug.Log($"SUCCESS: [{gameObject.name}] が Target を回収・消去し、腕を上げます！");

        // バンザイフラグをONにする
        isArmsRaised = true;
        armResetTimer = 0.0f;

        if (target != null)
        {
            PlayerController player = target.GetComponent<PlayerController>();
            if (player != null)
            {
                player.AddSuccessCount();
            }
        }

        Destroy(targetObject);
        fallenTargetTransform = null;
    }

    /// <summary>
    /// Animatorの上書きに負けないようにLateUpdate内で両腕を回転させるメソッド
    /// </summary>
    private void ApplyRaiseArmRotation()
    {
        Vector3 rotateEuler = Vector3.zero;
        switch (armRotateAxis)
        {
            case RotateAxis.XAxis: rotateEuler = new Vector3(raiseAngle, 0, 0); break;
            case RotateAxis.YAxis: rotateEuler = new Vector3(0, raiseAngle, 0); break;
            case RotateAxis.ZAxis: rotateEuler = new Vector3(0, 0, raiseAngle); break;
        }

        Quaternion armRotation = Quaternion.Euler(rotateEuler);

        if (rightArm != null)
        {
            rightArm.transform.localRotation = initialRightArmRotation * armRotation;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] rightArm が Inspector でアタッチされていません！");
        }

        if (leftArm != null)
        {
            leftArm.transform.localRotation = initialLeftArmRotation * armRotation;
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] leftArm が Inspector でアタッチされていません！");
        }
    }

    private void WanderBehavior()
    {
        stateTimer += Time.deltaTime;
        if (isMoving)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            if (stateTimer >= moveDuration)
            {
                isMoving = false;
                stateTimer = 0.0f;
            }
        }
        else
        {
            if (stateTimer >= waitDuration)
            {
                SetNewRandomDirection();
                isMoving = true;
                stateTimer = 0.0f;
            }
        }
    }

    private void SetNewRandomDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        transform.rotation = Quaternion.Euler(0, randomAngle, 0);
    }

    private bool IsTargetInSight()
    {
        Vector3 dirToTarget = (target.position - transform.position);
        float distanceToTarget = dirToTarget.magnitude;
        if (distanceToTarget > viewDistance) return false;
        dirToTarget.Normalize();
        float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);
        if (angleToTarget > viewAngle / 2.0f) return false;
        if (Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask)) return false;
        return true;
    }

    private void DrawFieldOfView()
    {
        if (viewMeshFilter == null) return;
        int stepCount = meshResolution;
        float stepAngleSize = viewAngle / stepCount;
        Vector3[] vertices = new Vector3[stepCount + 2];
        int[] triangles = new int[stepCount * 3];
        vertices[0] = Vector3.zero;

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -viewAngle / 2.0f + stepAngleSize * i;
            Vector3 dir = DirectionFromAngle(angle, false);
            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit, viewDistance, obstacleMask))
            {
                vertices[i + 1] = viewMeshFilter.transform.InverseTransformPoint(hit.point);
            }
            else
            {
                Vector3 targetPoint = transform.position + dir * viewDistance;
                vertices[i + 1] = viewMeshFilter.transform.InverseTransformPoint(targetPoint);
            }
            if (i < stepCount)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    private Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}