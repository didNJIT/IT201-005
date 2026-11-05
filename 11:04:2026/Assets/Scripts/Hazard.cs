using UnityEngine;

// TACTICAL MANEUVERING: a spinning or sliding obstacle. Touching it costs a life.
// Jump over it or dash (Shift) past it. Do NOT tag it "Enemy" (that would cost 2 lives).
[RequireComponent(typeof(Rigidbody))]
public class Hazard : MonoBehaviour
{
    public enum MotionType { Rotate, PingPong }
    public MotionType motion = MotionType.Rotate;

    [Header("Rotate")]
    public Vector3 rotationSpeed = new Vector3(0f, 90f, 0f); // degrees per second

    [Header("PingPong")]
    public Vector3 moveOffset = new Vector3(6f, 0f, 0f);
    public float moveSpeed = 0.5f;

    Rigidbody rb;
    Vector3 startPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        startPos = rb.position;
    }

    void FixedUpdate()
    {
        if (motion == MotionType.Rotate)
        {
            rb.MoveRotation(rb.rotation * Quaternion.Euler(rotationSpeed * Time.fixedDeltaTime));
        }
        else
        {
            float t = Mathf.PingPong(Time.time * moveSpeed, 1f);
            rb.MovePosition(startPos + moveOffset * t);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        PlayerController player = collision.gameObject.GetComponent<PlayerController>();
        if (player != null) player.LoseLife();
    }
}