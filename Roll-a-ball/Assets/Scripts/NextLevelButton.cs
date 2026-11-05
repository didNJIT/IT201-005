using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Attach this to a UI Button. When clicked, it loads the next scene
/// in Build Settings (or a specific scene if you fill in sceneName).
/// </summary>
[RequireComponent(typeof(Button))]
public class NextLevelButton : MonoBehaviour
{
    [Tooltip("Optional: type a scene name to load that scene instead of the next one in Build Settings.")]
    [SerializeField] private string sceneName = "";

    [Tooltip("If this is the last level, go back to the first scene (index 0).")]
    [SerializeField] private bool loopToFirstScene = true;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(LoadNextLevel);
    }

    public void LoadNextLevel()
    {
        // Load a specific scene if a name was given
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
        }
        else if (loopToFirstScene)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            Debug.Log("No more levels — this is the last scene.");
        }
    }
}