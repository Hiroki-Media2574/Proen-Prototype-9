using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform player; // プレイヤーのTransformをドラッグ＆ドロップ

    void LateUpdate()
    {
        if (player != null)
        {
            // プレイヤーの位置を取得し、カメラの高さ(Y軸)だけは維持する
            Vector3 newPosition = player.position;
            newPosition.y = transform.position.y;
            transform.position = newPosition;
        }
    }
}