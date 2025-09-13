using UnityEngine;

/// 矢印キー + 左クリックで XZ 平面を移動（Y は固定）
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float speed = 5f;                 // ユニット/秒
    [SerializeField] float arriveThreshold = 0.05f;    // 到着判定しきい値

    [Header("Facing")]
    [SerializeField] bool faceMoveDirection = false;   // 進行方向へ向きを合わせる
    [SerializeField] float turnSpeed = 10f;            // 回転追従速度

    [Header("Input")]
    [SerializeField] bool keyboardCancelsClick = true; // キー入力でクリック目標をキャンセル

    Vector3? clickTarget;   // クリック目的地（XZ）
    float fixedY;           // 高さは固定（開始時のY）

    void Start()
    {
        fixedY = transform.position.y;
    }

    void Update()
    {
        // --- クリック：画面座標を y=0 の地面平面に投影してX/Zを取得 ---
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                // y=0 平面（上向き法線）
                Plane ground = new Plane(Vector3.up, Vector3.zero);
                if (ground.Raycast(ray, out float enter))
                {
                    Vector3 hit = ray.GetPoint(enter);
                    clickTarget = new Vector3(hit.x, fixedY, hit.z); // ★ Z も使う。Y は固定
                }
            }
        }

        // --- 矢印キー入力（XZ） ---
        float h = Input.GetAxisRaw("Horizontal"); // ←→
        float v = Input.GetAxisRaw("Vertical");   // ↑↓
        Vector3 inputDir = new Vector3(h, 0f, v);
        bool hasKeyInput = inputDir.sqrMagnitude > 0f;

        if (keyboardCancelsClick && hasKeyInput) clickTarget = null;

        // --- 実際の移動ベクトルを決定 ---
        Vector3 velocity = Vector3.zero;

        if (hasKeyInput)
        {
            velocity = inputDir.normalized * speed;
        }
        else if (clickTarget.HasValue)
        {
            Vector3 toTarget = clickTarget.Value - transform.position;
            toTarget.y = 0f; // 水平成分のみで距離判定＆移動
            if (toTarget.sqrMagnitude <= arriveThreshold * arriveThreshold)
            {
                clickTarget = null; // 到着
            }
            else
            {
                velocity = toTarget.normalized * speed;
            }
        }

        // --- 位置更新（Yは固定） ---
        Vector3 pos = transform.position + velocity * Time.deltaTime;
        pos.y = fixedY;
        transform.position = pos;

        // --- 進行方向を向かせる（水平回転のみ） ---
        if (faceMoveDirection && velocity.sqrMagnitude > 0f)
        {
            Vector3 flat = new Vector3(velocity.x, 0f, velocity.z);
            Quaternion look = Quaternion.LookRotation(flat);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }
    }

    // 目標の可視化（任意）
    void OnDrawGizmos()
    {
        if (clickTarget.HasValue)
        {
            Gizmos.DrawWireSphere(clickTarget.Value, 0.1f);
        }
    }
}
