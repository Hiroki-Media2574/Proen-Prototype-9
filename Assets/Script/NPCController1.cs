using UnityEngine;
using TMPro;

public class NPCController1 : MonoBehaviour
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

    // 成功時等のメッセージ表示用UI
    public TextMeshProUGUI MessageText;

    [Header("視界の可視化設定")]
    // 視界メッシュの描画を担当する子オブジェクトのMeshFilter
    public MeshFilter viewMeshFilter;
    // 視界メッシュの滑らかさ（分割数）
    public int meshResolution = 30;

    // 動的に生成する視界用のメッシュ
    private Mesh viewMesh;

    void Start()
    {
        // 視界描画用のメッシュを新しく作成して初期化
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";

        // viewMeshFilterが設定されている場合のみメッシュをセット
        if (viewMeshFilter != null)
        {
            viewMeshFilter.mesh = viewMesh;
        }
        else
        {
            Debug.LogWarning("viewMeshFilter が設定されていません。Inspectorで子オブジェクトのMeshFilterを割り当ててください。");
        }
    }

    void Update()
    {
        // ターゲットが存在し、視界内にいる場合の処理
        if (target != null && IsTargetInSight())
        {
            Debug.Log("プレイヤーを視界内に捕捉！");
            transform.LookAt(target);
            // MPを減らす処理
            PlayerController.MP -= Time.deltaTime * 4;
        }
    }

    void LateUpdate()
    {
        // 毎フレーム視界メッシュを更新して描画
        DrawFieldOfView();
    }

    /// <summary>
    /// 扇形視界の中にターゲットがいるかを判定するメソッド
    /// </summary>
    private bool IsTargetInSight()
    {
        // プレイヤーへの方向ベクトルと距離を計算
        Vector3 dirToTarget = (target.position - transform.position);
        float distanceToTarget = dirToTarget.magnitude;

        // 距離チェック（最大距離より遠い場合は視界外）
        if (distanceToTarget > viewDistance) return false;

        // 方向を正規化して角度チェック（正面からの角度が指定角度の半分より大きい場合は視界外）
        dirToTarget.Normalize();
        float angleToTarget = Vector3.Angle(transform.forward, dirToTarget);

        if (angleToTarget > viewAngle / 2.0f) return false;

        // 遮蔽物チェック（NPCからプレイヤー間に障害物レイヤーがある場合は視界外）
        if (Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
        {
            return false;
        }

        // すべての判定をクリアした場合のみ視認成功
        return true;
    }

    /// <summary>
    /// ゲーム画面内に視界メッシュを動的生成するメソッド
    /// </summary>
    private void DrawFieldOfView()
    {
        // viewMeshFilterが未設定の場合は描画処理をスキップ
        if (viewMeshFilter == null) return;

        int stepCount = meshResolution;
        float stepAngleSize = viewAngle / stepCount;

        // ポリゴン形成に必要な頂点配列と三角形インデックス配列を定義
        Vector3[] vertices = new Vector3[stepCount + 2];
        int[] triangles = new int[stepCount * 3];

        // メッシュの中心点（NPCの足元/原点）
        vertices[0] = Vector3.zero;

        // 扇形の外周頂点を計算
        for (int i = 0; i <= stepCount; i++)
        {
            float angle = -viewAngle / 2.0f + stepAngleSize * i;
            Vector3 dir = DirectionFromAngle(angle, false);

            RaycastHit hit;
            // レイキャストを飛ばし、障害物に当たった場合はその位置まで、当たらない場合は最大距離まで伸ばす
            if (Physics.Raycast(transform.position, dir, out hit, viewDistance, obstacleMask))
            {
                vertices[i + 1] = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                vertices[i + 1] = transform.InverseTransformPoint(transform.position + dir * viewDistance);
            }

            // 三角形ポリゴン（面の情報）を構築
            if (i < stepCount)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        // 生成した頂点データをメッシュに反映
        viewMesh.Clear();
        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    /// <summary>
    /// NPCの現在の向きを考慮して、角度から方向ベクトルを求めるヘルパーメソッド
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