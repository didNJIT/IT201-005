using System.Collections.Generic;
using UnityEngine;

// PHYSICS: a trigger plate that adds up the Rigidbody mass of everything on it.
// When the total reaches requiredMass, the target (gate/bridge) moves to its open position.
// Ball mass 1, crate mass 3, requiredMass 2 -> you must push the crate onto it.
public class PressurePlate : MonoBehaviour
{
    public float requiredMass = 2f;

    [Header("What it opens")]
    public Transform target;
    public Vector3 openOffset = new Vector3(0f, -3f, 0f);
    public float moveSpeed = 3f;

    [Header("Feedback")]
    public Renderer plateRenderer;
    public Color idleColor = Color.red;
    public Color activeColor = Color.green;

    readonly HashSet<Rigidbody> bodies = new HashSet<Rigidbody>();
    Vector3 closedPos;
    bool pressed;

    void Start()
    {
        if (target) closedPos = target.position;
        if (plateRenderer) plateRenderer.material.color = idleColor;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody) bodies.Add(other.attachedRigidbody);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.attachedRigidbody) bodies.Remove(other.attachedRigidbody);
    }

    void Update()
    {
        bodies.RemoveWhere(b => b == null || !b.gameObject.activeInHierarchy);

        float total = 0f;
        foreach (Rigidbody b in bodies) total += b.mass;

        bool nowPressed = total >= requiredMass;
        if (nowPressed != pressed)
        {
            pressed = nowPressed;
            if (plateRenderer) plateRenderer.material.color = pressed ? activeColor : idleColor;
            if (pressed && PlayerController.Instance) PlayerController.Instance.ShowMessage("You opended the shed!");
        }

        if (target)
        {
            Vector3 goal = pressed ? closedPos + openOffset : closedPos;
            target.position = Vector3.MoveTowards(target.position, goal, moveSpeed * Time.deltaTime);
        }
    }
}