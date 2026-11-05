using UnityEngine;

// PROGRESSION MECHANISM: only opens if the player has the key from the NPC.
// Use a normal (solid) collider, not a trigger.
public class LockedDoor : MonoBehaviour
{
    public Vector3 openOffset = new Vector3(0f, 4f, 0f);
    public float openSpeed = 2f;

    bool opening;
    Vector3 openPos;

    void OnCollisionEnter(Collision collision)
    {
        if (opening) return;
        PlayerController p = collision.gameObject.GetComponent<PlayerController>();
        if (p == null) return;

        if (p.HasKey)
        {
            p.UseKey();
            opening = true;
            openPos = transform.position + openOffset;
            p.ShowMessage("The door unlocks!");
        }
        else
        {
            p.ShowMessage("Locked. Maybe someone around here has a key...");
        }
    }

    void Update()
    {
        if (opening)
            transform.position = Vector3.MoveTowards(transform.position, openPos, openSpeed * Time.deltaTime);
    }
}
