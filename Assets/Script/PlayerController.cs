using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Transform point1;
    public Transform point2;

    [Header("ˆÚ“®Ý’è")]
    [SerializeField] float speed = 3f;          // 1•b‚ ‚½‚è‚ÌˆÚ“®‹——£
    [SerializeField] float stopDuration = 3f;   // ’[‚ÅŽ~‚Ü‚é•b”
    [SerializeField] float arriveThreshold = 0.02f; // “ž’…”»’è‚Ì‚µ‚«‚¢’l
    [SerializeField] bool faceMoveDirection = false; // is•ûŒü‚ðŒü‚©‚¹‚é
    [SerializeField] float turnSpeed = 10f;     // ‰ñ“]‚Ì’Ç]‘¬“x

    enum State { MoveToPoint1, StopOnPoint1, MoveToPoint2, StopOnPoint2 }
    State currentState = State.MoveToPoint1;
    bool stateEnter = true;
    float stateTime = 0f;

    void ChangeState(State s)
    {
        currentState = s;
        stateEnter = true;
        stateTime = 0f;
        // Debug.Log(currentState); // •K—v‚È‚ç
    }

    void Update()
    {
        stateTime += Time.deltaTime;

        switch (currentState)
        {
            case State.MoveToPoint1:
                MoveTowards(point1.position);
                if (HasArrived(point1.position)) { ChangeState(State.StopOnPoint1); return; }
                return;

            case State.StopOnPoint1:
                if (stateTime >= stopDuration) { ChangeState(State.MoveToPoint2); return; }
                return;

            case State.MoveToPoint2:
                MoveTowards(point2.position);
                if (HasArrived(point2.position)) { ChangeState(State.StopOnPoint2); return; }
                return;

            case State.StopOnPoint2:
                if (stateTime >= stopDuration) { ChangeState(State.MoveToPoint1); return; }
                return;
        }
    }

    void MoveTowards(Vector3 target)
    {
        // ’¼üƒXƒ‰ƒCƒh
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // is•ûŒü‚ðŒü‚©‚¹‚éi”CˆÓj
        if (faceMoveDirection)
        {
            var dir = target - transform.position;
            dir.y = 0f; // …•½‚¾‚¯‰ñ“]
            if (dir.sqrMagnitude > 0.0001f)
            {
                var look = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
            }
        }

        stateEnter = false;
    }

    bool HasArrived(Vector3 target)
    {
        return (transform.position - target).sqrMagnitude <= arriveThreshold * arriveThreshold;
    }

    void OnDrawGizmosSelected()
    {
        if (point1 && point2) Gizmos.DrawLine(point1.position, point2.position);
    }
}
