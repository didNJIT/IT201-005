using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // NEW: lets other scripts (NPC, door, plate) find the player easily
    public static PlayerController Instance { get; private set; }

    [Header("Movement")]
    public float speed = 0;
    public float jumpForce = 5f;
    public float dashForce = 10f;
    public float dashCooldown = 1f;
    public float groundCheckDistance = 0.6f;
    public TextMeshProUGUI dashText; // optional UI text for dash cooldown

    [Header("UI")]
    public TextMeshProUGUI countText;
    public GameObject winTextObject;
    public GameObject endScreen; // full-screen ending picture
    public TextMeshProUGUI messageText; // NEW: pop-up messages ("Locked!", "Checkpoint!")

    [Header("Doors")]
    public GameObject door;
    public int doorUnlockCount = 12;
    public GameObject door2;
    public int door2UnlockCount = 22;
    public GameObject door3;
    public int door3UnlockCount = 26;
    public int winCount = 34;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float killHeight = -10f;

    [Header("Key")] // NEW: Progression - key from the NPC opens the LockedDoor
    public TextMeshProUGUI keyText;

    [Header("Lives")]
    public int lives = 3;
    public TextMeshProUGUI livesText;
    public GameObject gameOverTextObject;

    public AudioClip PickupSounds;
    public GameObject exitObject;

    [Header("Exit")]
    public string exitSceneName = "Forest";

    private Rigidbody rb;
    private int count;
    private float movementX;
    private float movementY;
    private float lastDashTime = -10f;
    private Vector3 startPosition;
    private AudioSource audioSource;
    private bool hasKey;               // NEW
    private Coroutine messageRoutine;  // NEW

    // NEW: read-only access for other scripts
    public int Count => count;
    public bool HasKey => hasKey;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        count = 0;
        audioSource = GetComponent<AudioSource>();
        startPosition = transform.position;

        SetCountText();
        if (winTextObject != null) winTextObject.SetActive(false);
        if (gameOverTextObject != null) gameOverTextObject.SetActive(false);
        if (livesText != null) livesText.text = "Lives: " + lives;
        if (messageText != null) messageText.text = "";
        UpdateKeyText();
    }

    void OnMove(InputValue movementValue)
    {
        Vector2 movementVector = movementValue.Get<Vector2>();
        movementX = movementVector.x;
        movementY = movementVector.y;
    }

    // Space bar
    void OnJump(InputValue value)
    {
        if (Time.timeScale == 0f) return;   // no jumping while paused
        if (IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    // Shift
    void OnSprint(InputValue value)
    {
        if (Time.timeScale == 0f) return;
        if (Time.time < lastDashTime + dashCooldown) return;

        Vector3 direction = new Vector3(movementX, 0f, movementY);
        if (direction.sqrMagnitude < 0.01f) direction = transform.forward;

        rb.AddForce(direction.normalized * dashForce, ForceMode.Impulse);
        lastDashTime = Time.time;
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, ~0, QueryTriggerInteraction.Ignore);
    }

    void Update()
    {
        if (dashText)
        {
            float readyTime = lastDashTime + dashCooldown;
            dashText.text = Time.time >= readyTime
                ? "Dash: READY (Shift)"
                : "Dash: " + (readyTime - Time.time).ToString("0.0") + "s";
        }
    }

    void FixedUpdate()
    {
        Vector3 movement = new Vector3(movementX, 0.0f, movementY);
        rb.AddForce(movement * speed);

        // Turn to face the direction of movement
        if (movement.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(movement);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, 0.2f));
        }

        // Backup respawn if the player falls too far
        if (transform.position.y < killHeight)
        {
            LoseLife();
        }
    }

    void SetCountText()
    {
        if (countText != null) countText.text = "Count: " + count.ToString() + "/" + winCount;

        if (count >= doorUnlockCount && door != null) door.SetActive(false);
        if (count >= door2UnlockCount && door2 != null) door2.SetActive(false);
        if (count >= door3UnlockCount && door3 != null) door3.SetActive(false);

        if (count >= winCount)
        {
            if (exitObject != null)
            {
                exitObject.SetActive(true);
            }
            else
            {
                EndGame();
            }

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in enemies)
            {
                Destroy(enemy.transform.root.gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy") ||
            collision.transform.root.CompareTag("Enemy"))
        {
            LoseLife();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            if (audioSource != null && PickupSounds != null) audioSource.PlayOneShot(PickupSounds);
            other.gameObject.SetActive(false);
            count = count + 1;
            SetCountText();
        }
        if (other.gameObject.CompareTag("Exit"))
        {
            if (!string.IsNullOrEmpty(exitSceneName))
                SceneManager.LoadScene(exitSceneName);
            else
                EndGame();
        }
        else if (other.gameObject.CompareTag("KillFloor"))
        {
            LoseLife();
        }
    }

    void EndGame()
    {
        if (endScreen != null) endScreen.SetActive(true);
        if (winTextObject != null) winTextObject.SetActive(true);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Time.timeScale = 0f; // freeze the game on the ending screen
    }

    // CHANGED: now public so Hazard.cs can call it
    public void LoseLife()
    {
        lives--;
        if (livesText != null) livesText.text = "Lives: " + lives;

        if (lives <= 0)
        {
            if (gameOverTextObject != null) gameOverTextObject.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            Respawn();
        }
    }

    void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 spawn = respawnPoint != null ? respawnPoint.position : startPosition;
        rb.position = spawn;
        transform.position = spawn;
    }

    // ---------- NEW: used by the five-feature scripts ----------

    // Checkpoint.cs
    public void SetCheckpoint(Transform point)
    {
        respawnPoint = point;
    }

    // NPCDialogue.cs
    public void GiveKey()
    {
        hasKey = true;
        UpdateKeyText();
        ShowMessage("You got the key!");
    }

    // LockedDoor.cs
    public void UseKey()
    {
        hasKey = false;
        UpdateKeyText();
    }

    void UpdateKeyText()
    {
        if (keyText != null) keyText.text = hasKey ? "Key: YES" : "Key: no";
    }

    public void ShowMessage(string msg, float duration = 2.5f)
    {
        if (messageText == null || !gameObject.activeInHierarchy) return;
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(msg, duration));
    }

    IEnumerator MessageRoutine(string msg, float duration)
    {
        messageText.text = msg;
        yield return new WaitForSeconds(duration);
        messageText.text = "";
    }
}