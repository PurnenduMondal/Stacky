using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;

    private Vector3 offset;
    private bool zoomingOut = false;
    private float towerHeight = 0f;
    private float zoomOutSize = 8f;
    private float normalSize = 5f;
    private Camera cam;

    void Start()
    {
        offset = transform.position;
        cam = GetComponent<Camera>();
        normalSize = cam.orthographicSize;
    }

    void LateUpdate()
    {
        if (target == null) return;

        if (zoomingOut)
        {
            // Smoothly zoom out camera size
            cam.orthographicSize = Mathf.Lerp(
                cam.orthographicSize,
                zoomOutSize,
                Time.deltaTime * 2f
            );

            // Move camera to centre of tower
            float towerMidY = towerHeight / 2f;
            Vector3 desiredPos = new Vector3(
                transform.position.x,
                towerMidY + offset.y,
                transform.position.z
            );

            transform.position = Vector3.Lerp(
                transform.position,
                desiredPos,
                Time.deltaTime * 2f
            );

            return;
        }

        // Normal follow — only move upward
        float targetY = target.position.y + offset.y;
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

    // Call this when game over happens
    public void ZoomOutToShowTower(float height)
    {
        zoomingOut = true;
        towerHeight = height;

        // Calculate how much to zoom based on tower height
        // Taller tower = more zoom out needed
        zoomOutSize = Mathf.Max(8f, height * 0.6f);
    }

    // Call this when game restarts
    public void ResetCamera()
    {
        zoomingOut = false;
        cam.orthographicSize = normalSize;
    }
}