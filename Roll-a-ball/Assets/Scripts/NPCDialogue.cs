using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// SOCIAL INTERACTION: walk up to the NPC and press E to talk.
// The NPC gives hints and hands over the door key once you've collected enough pickups.
// What it says depends on the game state.
// Put a Collider set to "Is Trigger" (the talk radius) on the SAME object as this script.
public class NPCDialogue : MonoBehaviour
{
    public string npcName = "Meatball";
    public int requiredPickups = 11;

    [TextArea] public string[] introLines =
    {
        "Well hello there player!",
        "I guard the key to the vault door up ahead.",
        "Prove yourself by collecting 11 pickups, then come back and talk to me."
    };

    [TextArea] public string[] notEnoughLines =
    {
        "You have {have}. I need to see 11.",
        "Tip: that gate by the red plate won't stay open on its own. Something heavy might help..."
    };

    [TextArea] public string[] giveKeyLines =
    {
        "{have} pickups! Impressive.",
        "Here's the key. Watch out for the spinning bars on the way.",
        "Jump them with SPACE, or dash past with SHIFT when the gap opens!"
    };

    [TextArea] public string[] afterLines =
    {
        "Go on, the door is waiting!"
    };

    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI promptText;   // "Press E to talk"

    public float turnSpeed = 5f;

    PlayerController player;
    string[] currentLines;
    int lineIndex = -1;
    bool metPlayer;
    bool keyGiven;

    void Start()
    {
        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (promptText) promptText.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController p = other.GetComponent<PlayerController>();
        if (p == null) return;
        player = p;
        if (promptText)
        {
            promptText.text = "Press E to talk to " + npcName;
            promptText.gameObject.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() == null) return;
        player = null;
        EndDialogue();
        if (promptText) promptText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        // Turn to face the player
        Vector3 dir = player.transform.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (lineIndex < 0) StartDialogue();
            else NextLine();
        }
    }

    void StartDialogue()
    {
        currentLines = ChooseLines();
        lineIndex = 0;
        if (dialoguePanel) dialoguePanel.SetActive(true);
        if (promptText) promptText.gameObject.SetActive(false);
        ShowLine();
    }

    string[] ChooseLines()
    {
        if (keyGiven) return afterLines;
        if (!metPlayer)
        {
            metPlayer = true;
            return introLines;
        }
        if (player.Count >= requiredPickups)
        {
            keyGiven = true;
            player.GiveKey();
            return giveKeyLines;
        }
        return notEnoughLines;
    }

    void NextLine()
    {
        lineIndex++;
        if (lineIndex >= currentLines.Length)
        {
            EndDialogue();
            if (player && promptText) promptText.gameObject.SetActive(true);
            return;
        }
        ShowLine();
    }

    void ShowLine()
    {
        string line = currentLines[lineIndex]
            .Replace("{need}", requiredPickups.ToString())
            .Replace("{have}", player.Count.ToString());
        if (dialogueText) dialogueText.text = npcName + ": " + line + "\n<size=70%>(E to continue)</size>";
    }

    void EndDialogue()
    {
        lineIndex = -1;
        if (dialoguePanel) dialoguePanel.SetActive(false);
    }
}