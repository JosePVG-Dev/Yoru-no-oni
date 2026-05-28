using UnityEngine;
using UnityEngine.SceneManagement;

public class Shrine : MonoBehaviour
{
    [SerializeField] private int health = 10;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"[Shrine] Took {amount} damage, health: {health}");
        if (health <= 0)
        {
            StartCoroutine(GameOverRoutine());
        }
    }

    private System.Collections.IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene("Menu");
    }
}
