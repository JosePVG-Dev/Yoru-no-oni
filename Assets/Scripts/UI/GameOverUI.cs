using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text causeText;
    [SerializeField] private TMP_Text waveText;

    public bool IsOpen => panel != null && panel.activeSelf;

    private void Start()
    {
        if (panel != null)
        {
            var retryBtn = panel.transform.Find("RetryButton")?.GetComponent<UnityEngine.UI.Button>();
            if (retryBtn != null) retryBtn.onClick.AddListener(Reiniciar);

            var menuBtn = panel.transform.Find("MenuButton")?.GetComponent<UnityEngine.UI.Button>();
            if (menuBtn != null) menuBtn.onClick.AddListener(IrAlMenu);

            panel.SetActive(false);
        }
    }

    public void Show(string cause)
    {
        if (panel != null)
            panel.SetActive(true);

        Time.timeScale = 0f;

        if (causeText != null)
            causeText.text = cause;

        var spawner = FindFirstObjectByType<WaveSpawner>();
        int wave = spawner != null ? spawner.CurrentWave : 1;
        if (waveText != null)
            waveText.text = "Oleada alcanzada: " + wave;
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}
