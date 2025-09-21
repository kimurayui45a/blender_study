using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float speed = 5f;
    [SerializeField] float arriveThreshold = 0.05f;

    [Header("Facing")]
    [SerializeField] bool faceMoveDirection = false;
    [SerializeField] float turnSpeed = 10f;

    [Header("Input")]
    [SerializeField] bool keyboardCancelsClick = true;

    [Header("Animation")]
    [SerializeField] Animator animator;                 // モデル側の Animator を割り当て
    [SerializeField] string moveSpeedParam = "MoveSpeed";
    [SerializeField] float dampTime = 0.1f;             // 動いている時だけ使うダンピング
    [SerializeField] float idleEpsilon = 0.0005f;       // これ未満は完全停止とみなす

    Vector3? clickTarget;
    float fixedY;

    void Start()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (animator) animator.applyRootMotion = false;
        fixedY = transform.position.y;
    }

    void Update()
    {
        // クリック目的地
        if (Input.GetMouseButtonDown(0))
        {
            var cam = Camera.main;
            if (cam)
            {
                var ray = cam.ScreenPointToRay(Input.mousePosition);
                var ground = new Plane(Vector3.up, Vector3.zero); // y=0
                if (ground.Raycast(ray, out var enter))
                {
                    var hit = ray.GetPoint(enter);
                    clickTarget = new Vector3(hit.x, fixedY, hit.z);
                }
            }
        }

        // 入力→速度決定
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v);
        bool hasKeyInput = inputDir.sqrMagnitude > 0f;
        if (keyboardCancelsClick && hasKeyInput) clickTarget = null;

        Vector3 velocity = Vector3.zero;
        if (hasKeyInput)
        {
            velocity = inputDir.normalized * speed;
        }
        else if (clickTarget.HasValue)
        {
            Vector3 toTarget = clickTarget.Value - transform.position; toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= arriveThreshold * arriveThreshold)
            {
                clickTarget = null;          // 到着：次フレームは velocity=0
            }
            else
            {
                velocity = toTarget.normalized * speed;
            }
        }

        // 位置更新（Y固定）
        Vector3 pos = transform.position + velocity * Time.deltaTime;
        pos.y = fixedY;
        transform.position = pos;

        // 向き
        if (faceMoveDirection && velocity.sqrMagnitude > 0f)
        {
            Vector3 flat = new Vector3(velocity.x, 0f, velocity.z);
            Quaternion look = Quaternion.LookRotation(flat);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }

        // ---- ここが肝：MoveSpeed を 0.01 境で安定させる ----
        if (animator)
        {
            float current = animator.GetFloat(moveSpeedParam);

            // 実速度 (0～speed) を 0～1 に正規化
            float target01 = 0f;
            float mag = velocity.magnitude;

            if (mag <= idleEpsilon)
            {
                // 完全停止と判定：即 0 にする（ダンピングを使わない）
                target01 = 0f;
                animator.SetFloat(moveSpeedParam, 0f); // ← キッパリ 0
            }
            else
            {
                target01 = Mathf.Clamp01(mag / Mathf.Max(0.0001f, speed));
                // 動いている間はダンピングで滑らかに追従
                animator.SetFloat(moveSpeedParam, target01, dampTime, Time.deltaTime);
            }

            // デバッグしたい時はコメント解除：
            // Debug.Log($"MoveSpeed target={target01:F3}, actual={animator.GetFloat(moveSpeedParam):F3}");
        }
    }

    void OnDrawGizmos()
    {
        if (clickTarget.HasValue) Gizmos.DrawWireSphere(clickTarget.Value, 0.1f);
    }
}
