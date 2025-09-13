using UnityEngine;

/// ���L�[ + ���N���b�N�� XZ ���ʂ��ړ��iY �͌Œ�j
public class PlayerController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float speed = 5f;                 // ���j�b�g/�b
    [SerializeField] float arriveThreshold = 0.05f;    // �������肵�����l

    [Header("Facing")]
    [SerializeField] bool faceMoveDirection = false;   // �i�s�����֌��������킹��
    [SerializeField] float turnSpeed = 10f;            // ��]�Ǐ]���x

    [Header("Input")]
    [SerializeField] bool keyboardCancelsClick = true; // �L�[���͂ŃN���b�N�ڕW���L�����Z��

    Vector3? clickTarget;   // �N���b�N�ړI�n�iXZ�j
    float fixedY;           // �����͌Œ�i�J�n����Y�j

    void Start()
    {
        fixedY = transform.position.y;
    }

    void Update()
    {
        // --- �N���b�N�F��ʍ��W�� y=0 �̒n�ʕ��ʂɓ��e����X/Z���擾 ---
        if (Input.GetMouseButtonDown(0))
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                // y=0 ���ʁi������@���j
                Plane ground = new Plane(Vector3.up, Vector3.zero);
                if (ground.Raycast(ray, out float enter))
                {
                    Vector3 hit = ray.GetPoint(enter);
                    clickTarget = new Vector3(hit.x, fixedY, hit.z); // �� Z ���g���BY �͌Œ�
                }
            }
        }

        // --- ���L�[���́iXZ�j ---
        float h = Input.GetAxisRaw("Horizontal"); // ����
        float v = Input.GetAxisRaw("Vertical");   // ����
        Vector3 inputDir = new Vector3(h, 0f, v);
        bool hasKeyInput = inputDir.sqrMagnitude > 0f;

        if (keyboardCancelsClick && hasKeyInput) clickTarget = null;

        // --- ���ۂ̈ړ��x�N�g�������� ---
        Vector3 velocity = Vector3.zero;

        if (hasKeyInput)
        {
            velocity = inputDir.normalized * speed;
        }
        else if (clickTarget.HasValue)
        {
            Vector3 toTarget = clickTarget.Value - transform.position;
            toTarget.y = 0f; // ���������݂̂ŋ������聕�ړ�
            if (toTarget.sqrMagnitude <= arriveThreshold * arriveThreshold)
            {
                clickTarget = null; // ����
            }
            else
            {
                velocity = toTarget.normalized * speed;
            }
        }

        // --- �ʒu�X�V�iY�͌Œ�j ---
        Vector3 pos = transform.position + velocity * Time.deltaTime;
        pos.y = fixedY;
        transform.position = pos;

        // --- �i�s��������������i������]�̂݁j ---
        if (faceMoveDirection && velocity.sqrMagnitude > 0f)
        {
            Vector3 flat = new Vector3(velocity.x, 0f, velocity.z);
            Quaternion look = Quaternion.LookRotation(flat);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }
    }

    // �ڕW�̉����i�C�Ӂj
    void OnDrawGizmos()
    {
        if (clickTarget.HasValue)
        {
            Gizmos.DrawWireSphere(clickTarget.Value, 0.1f);
        }
    }
}
