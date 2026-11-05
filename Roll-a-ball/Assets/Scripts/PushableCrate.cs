using UnityEngine;

// PHYSICS: a dynamic Rigidbody crate the ball can shove around.
// It's heavy enough to hold down a PressurePlate when the ball alone can't.
// If it falls off the level it goes back to where it started.
[RequireComponent(typeof(Rigidbody))]
public class PushableCrate : MonoBehaviour
{
    public float resetHeight = -10f;

    Rigidbody rb;
    Vector3 startPos;
    Quaternion startRot;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPos = rb.position;
        startRot = rb.rotation;
    }

    void FixedUpdate()
    {
        if (rb.position.y < resetHeight)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = startPos;
            rb.rotation = startRot;
            if (PlayerController.Instance) PlayerController.Instance.ShowMessage("The crate fell... it's back at the start.");
        }
    }
}