using UnityEngine;

public class TestRailMovement : MonoBehaviour
{
    // public Transform startPoint;
    // public Transform endPoint;
    public float speed = 5.0f;

    private Vector3 currentTarget;
    private bool isMoving = false;

    void Start()
    {
        // currentTarget = startPoint.position;
        isMoving = true;
    }

    void Update()
    {
        if (isMoving)
        {
            // transform.position = Vector3.MoveTowards(transform.position, currentTarget, speed * Time.deltaTime);
            // Vector3 tmp_position = transform.position;
            // tmp_position.x -= 50f;
            // transform.position = tmp_position;
            // using delta time to move the object
            transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
        // if (transform.position == currentTarget)
        // {
        //     isMoving = false;
        //     currentTarget = (currentTarget == startPoint.position) ? endPoint.position : startPoint.position;
        //     isMoving = true;
        // }
    }
}
