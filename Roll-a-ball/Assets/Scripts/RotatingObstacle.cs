using UnityEngine;

public class RotatingObstacle : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 90, 0); // degrees per second
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotationSpeed * Time.fixedDeltaTime));
    }
}
