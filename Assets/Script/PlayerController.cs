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
    [SerializeField] Animator animator;                 // ���f������ Animator �����蓖��
    [SerializeField] string moveSpeedParam = "MoveSpeed";
    [SerializeField] float dampTime = 0.1f;             // �����Ă��鎞�����g���_���s���O
    [SerializeField] float idleEpsilon = 0.0005f;       // ���ꖢ���͊��S��~�Ƃ݂Ȃ�

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
        // �N���b�N�ړI�n
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

        // ���́����x����
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
                clickTarget = null;          // �����F���t���[���� velocity=0
            }
            else
            {
                velocity = toTarget.normalized * speed;
            }
        }

        // �ʒu�X�V�iY�Œ�j
        Vector3 pos = transform.position + velocity * Time.deltaTime;
        pos.y = fixedY;
        transform.position = pos;

        // ����
        if (faceMoveDirection && velocity.sqrMagnitude > 0f)
        {
            Vector3 flat = new Vector3(velocity.x, 0f, velocity.z);
            Quaternion look = Quaternion.LookRotation(flat);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }

        // ---- �������́FMoveSpeed �� 0.01 ���ň��肳���� ----
        if (animator)
        {
            float current = animator.GetFloat(moveSpeedParam);

            // �����x (0�`speed) �� 0�`1 �ɐ��K��
            float target01 = 0f;
            float mag = velocity.magnitude;

            if (mag <= idleEpsilon)
            {
                // ���S��~�Ɣ���F�� 0 �ɂ���i�_���s���O���g��Ȃ��j
                target01 = 0f;
                animator.SetFloat(moveSpeedParam, 0f); // �� �L�b�p�� 0
            }
            else
            {
                target01 = Mathf.Clamp01(mag / Mathf.Max(0.0001f, speed));
                // �����Ă���Ԃ̓_���s���O�Ŋ��炩�ɒǏ]
                animator.SetFloat(moveSpeedParam, target01, dampTime, Time.deltaTime);
            }

            // �f�o�b�O���������̓R�����g�����F
            // Debug.Log($"MoveSpeed target={target01:F3}, actual={animator.GetFloat(moveSpeedParam):F3}");
        }
    }

    void OnDrawGizmos()
    {
        if (clickTarget.HasValue) Gizmos.DrawWireSphere(clickTarget.Value, 0.1f);
    }
}
