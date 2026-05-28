using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        Debug.Log("[MainMenu] PlayGame called, loading 'Game' scene...");
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        Debug.Log("[MainMenu] QuitGame called");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

void Start()
    {
        var playBtn = GameObject.Find("PlayButton")?.GetComponent<UnityEngine.UI.Button>();
        if (playBtn != null) playBtn.onClick.AddListener(PlayGame);
        var quitBtn = GameObject.Find("QuitButton")?.GetComponent<UnityEngine.UI.Button>();
        if (quitBtn != null) quitBtn.onClick.AddListener(QuitGame);
        var canvas = GameObject.Find("Canvas");
        var settingsBtn = GameObject.Find("SettingsButton")?.GetComponent<UnityEngine.UI.Button>();
        if (canvas != null)
        {
            var sm = canvas.transform.Find("SettingsPanel")?.GetComponent<SettingsMenu>();
            if (settingsBtn != null && sm != null) settingsBtn.onClick.AddListener(sm.OpenSettings);
        }
    }

}
