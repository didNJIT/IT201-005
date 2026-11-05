using UnityEngine;

public class PetFollow : MonoBehaviour
{
    public Transform player;
    public float followDistance = 2f;   // how far it stays away
    public float smoothSpeed = 4f;      // how quickly it catches up
    public float hoverHeight = 0.0f;   // how high above the ball it floats

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 playerPos = player.position + Vector3.up * hoverHeight;

        // Flat direction from the player to the pet
        Vector3 dir = transform.position - playerPos;
        dir.y = 0;
        if (dir.sqrMagnitude < 0.001f) dir = Vector3.back;

        // Spot that is exactly followDistance away from the player
        Vector3 targetPos = playerPos + dir.normalized * followDistance;
        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);

        // Face the player
        transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
    }
}