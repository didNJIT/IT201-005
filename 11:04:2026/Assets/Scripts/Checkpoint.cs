
using UnityEngine;
 
// TACTICAL MANEUVERING support: roll through it to make this your respawn point.
// Needs a Collider set to "Is Trigger".
public class Checkpoint : MonoBehaviour
{
    public float spawnHeight = 0.5f; // how far above the checkpoint you reappear
    Transform spawnPoint;
    bool reached;
 
    void Start()
    {
        spawnPoint = new GameObject(name + "_Spawn").transform;
        spawnPoint.position = transform.position + Vector3.up * spawnHeight;
    }
 
    void OnTriggerEnter(Collider other)
    {
        PlayerController p = other.GetComponent<PlayerController>();
        if (p == null || reached) return;
        reached = true;
        p.SetCheckpoint(spawnPoint);
        p.ShowMessage("Checkpoint!");
    }
}
 