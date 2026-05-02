using UnityEngine;

public class BlockController : MonoBehaviour
{
    public float speed = 3f;
    public bool isMoving = true;
    public bool moveOnX = true; // true = X axis, false = Z axis

    private int direction = 1;
    private float boundary = 3f;

    void Update()
    {
        if (!isMoving) return;

        if (moveOnX)
        {
            transform.position += new Vector3(speed * direction * Time.deltaTime, 0, 0);
            if (transform.position.x >= boundary || transform.position.x <= -boundary)
                direction *= -1;
        }
        else
        {
            transform.position += new Vector3(0, 0, speed * direction * Time.deltaTime);
            if (transform.position.z >= boundary || transform.position.z <= -boundary)
                direction *= -1;
        }
    }

    public void Stop() => isMoving = false;
}