using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Shrine : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Sprites")]
    public Sprite fullSprite;
    public Sprite damagedSprite;
    public Sprite criticalSprite;
    public Sprite destroyedSprite;

    [Header("Death Delay")]
    [Min(0f)]
    public float deathDelay = 1.5f;

    private SpriteRenderer spriteRenderer;
    private bool isDead = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        UpdateSprite();
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        UpdateSprite();

        if (currentHealth <= 0)
        {
            isDead = true;
            StartCoroutine(GameOverRoutine());
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (spriteRenderer == null) return;

        float healthPercent = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

        Sprite targetSprite;

        if (currentHealth <= 0 && destroyedSprite != null)
        {
            targetSprite = destroyedSprite;
        }
        else if (healthPercent <= 0.33f && criticalSprite != null)
        {
            targetSprite = criticalSprite;
        }
        else if (healthPercent <= 0.66f && damagedSprite != null)
        {
            targetSprite = damagedSprite;
        }
        else if (fullSprite != null)
        {
            targetSprite = fullSprite;
        }
        else
        {
            return;
        }

        spriteRenderer.sprite = targetSprite;
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(deathDelay);
        SceneManager.LoadScene("Menu");
    }
}
