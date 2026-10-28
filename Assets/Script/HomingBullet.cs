using UnityEngine;

public class HomingBullet : MonoBehaviour
{
    // 追尾対象のTransform
    public Transform target;
    // 弾の飛ぶスピード
    public float speed = 20.0f;

    private Rigidbody rb;
    // 追尾中かどうかを管理するフラグ
    private bool isHoming = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 発射時は重力をオフにして直線で飛ばす
        if (rb != null)
        {
            rb.useGravity = false;
        }
    }

    void FixedUpdate()
    {
        // ヒット後など、追尾が終了している場合は物理挙動に任せる
        if (!isHoming) return;

        // ターゲットが存在しない場合は現在の正面方向に飛ぶ
        if (target == null)
        {
            rb.linearVelocity = transform.forward * speed;
            return;
        }

        // 1. ターゲットへ向かう直線方向ベクトルを計算
        Vector3 targetDirection = (target.position - transform.position).normalized;

        // 2. 弾の向きをターゲットへ一直線に向ける
        transform.forward = targetDirection;

        // 3. 旋回させず、一直線に向かって進む
        rb.linearVelocity = targetDirection * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isHoming) return;

        // "Target" タグに当たった場合の処理
        if (collision.gameObject.CompareTag("Target"))
        {
            Debug.Log("Targetに命中！減速しながら落下を開始させます。");

            GameObject targetObj = collision.gameObject;
            Rigidbody targetRb = targetObj.GetComponent<Rigidbody>();
            if (targetRb == null)
            {
                targetRb = targetObj.AddComponent<Rigidbody>();
            }

            // 物理挙動と重力を有効化
            targetRb.isKinematic = false;
            targetRb.useGravity = true;

            // --- 【追加】 落下後に少しずつ速度が落ちて完全に止まるよう抵抗（減衰）を設定 ---
            // ※Unity 6のプロパティ名です（旧Unityバージョンの場合は drag / angularDrag）
            targetRb.linearDamping = 2.0f;  // 移動速度に対する減衰抵抗
            targetRb.angularDamping = 2.0f; // 回転速度に対する減衰抵抗

            // NPCが「落とされたTarget」だと判別できるように名前を変更
            targetObj.name = "FallenTarget";

            StopHomingAndFall();
        }
        else if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Wall"))
        {
            StopHomingAndFall();
        }
    }

    /// <summary>
    /// 追尾を停止し、Rigidbodyの重力に従って物理落下させるメソッド
    /// </summary>
    private void StopHomingAndFall()
    {
        isHoming = false;
        if (rb != null)
        {
            rb.useGravity = true; // 重力を有効化
        }
        // 5秒後にステージ上の弾を消去する
        Destroy(gameObject, 5.0f);
    }
}