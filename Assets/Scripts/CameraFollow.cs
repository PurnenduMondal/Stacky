using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;

    private Vector3 offset;

    void Start()
    {
        offset = transform.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float targetY = target.position.y + offset.y;

        // Only move camera upward, never back down
        if (targetY > transform.position.y)
        {
            Vector3 desiredPos = new Vector3(
                transform.position.x,
                targetY,
                transform.position.z
            );

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPos,
                smoothSpeed * Time.deltaTime
            );
        }
    }
}