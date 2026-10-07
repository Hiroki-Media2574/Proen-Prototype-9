using UnityEngine;
using TMPro;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class NPCController1 : MonoBehaviour
{
    public Transform target;            // プレイヤーのTransform
    public float viewDistance = 10.0f;  // 視界の届く最大距離
    [Range(0, 360)]
    public float viewAngle = 90.0f;     // 視界の角度（例: 90度）
    public LayerMask obstacleMask;      // 視線遮断用の障害物レイヤー（Wallなど）

    public TextMeshProUGUI MessageText;

    [Header("視界の可視化設定")]
    public int meshResolution = 30;     // 視界メッシュの滑らかさ（分割数）
    public Material viewMeshMaterial;   // 視界用マテリアル（半透明の赤など）

    private Mesh viewMesh;
    private MeshFilter viewMeshFilter;

    void Start()
    {
        // 視界描画用のメッシュを初期化
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        viewMeshFilter = GetComponent<MeshFilter>();
        viewMeshFilter.mesh = viewMesh;

        // マテリアルを適用
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (viewMeshMaterial != null)
        {
            meshRenderer.material = viewMeshMaterial;
        }
    }

    void Update()
    {
        if (target != null && IsTargetInSight())
        {
            Debug.Log("プレイヤーを視界内に捕捉！");
            transform.LookAt(target);
            PlayerController.MP -= Time.deltaTime * 4;
        }
    }

    void LateUpdate()
    {
        // 毎フレーム視界メッシュを計算して描画
        DrawFieldOfView();
    }

    /// <summary>
    /// 扇形視界の中にターゲットがいるかを判定する
    /// </summary>
    private bool IsTargetInSight()
    {
        Vector3 dirToTarget = (target.position - transform.position);
        float distanceToTarget = dirToTarget.magnitude;

        if (distanceToTarget > viewDistance) return false;

        dirToTarget.Normalize();
        float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);

        if (angleToTarget > viewAngle / 2.0f) return false;

        if (Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// ゲーム画面内に視界メッシュを動的生成する
    /// </summary>
    private void DrawFieldOfView()
    {
        int stepCount = meshResolution;
        float stepAngleSize = viewAngle / stepCount;

        Vector3[] vertices = new Vector3[stepCount + 2];
        int[] triangles = new int[stepCount * 3];

        // メッシュの中心（NPCの位置）
        vertices[0] = Vector3.zero;

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -viewAngle / 2.0f + stepAngleSize * i;
            Vector3 dir = DirectionFromAngle(angle, false);

            RaycastHit hit;
            // 壁に当たった場合は当たった位置まで、当たらない場合は最大距離まで頂点を伸ばす
            if (Physics.Raycast(transform.position, dir, out hit, viewDistance, obstacleMask))
            {
                vertices[i + 1] = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                vertices[i + 1] = transform.InverseTransformPoint(transform.position + dir * viewDistance);
            }

            // 三角形ポリゴンの構成
            if (i < stepCount)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        // メッシュデータの更新
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    /// <summary>
    /// オブジェクトの向きを考慮した角度から方向ベクトルを計算
    /// </summary>
    private Vector3 DirectionFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}