using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ControlsIntro : MonoBehaviour
{
    void Start()
    {
        var btn = GameObject.Find("StartButton")?.GetComponent<UnityEngine.UI.Button>();
        if (btn != null) btn.onClick.AddListener(StartGame);
    }

    private void Update()
    {
        if (Keyboard.current?.enterKey.wasPressedThisFrame == true || Keyboard.current?.spaceKey.wasPressedThisFrame == true)
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        Debug.Log("[ControlsIntro] StartGame loading scene: Game");
        if (gameObject.activeInHierarchy)
            SceneManager.LoadScene("Game");
    }
}