using UnityEngine;

public class Shrine : MonoBehaviour, IHasHealth
{
    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Sprites")]
    public Sprite fullSprite;
    public Sprite damagedSprite;
    public Sprite criticalSprite;
    public Sprite destroyedSprite;

    [Header("Audio")]

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

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayShrineDamage();

        UpdateSprite();

        if (currentHealth <= 0)
        {
            isDead = true;
            var gameOver = FindFirstObjectByType<GameOverUI>();
            if (gameOver != null)
                gameOver.Show("El Santuario ha sido destruido");
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        UpdateSprite();
    }

    public void IncreaseMaxHealth(int amount)
    {
        if (isDead) return;

        maxHealth += amount;
        currentHealth += amount;
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

    public int Health => currentHealth;
    public int MaxHealth => maxHealth;

    private void OnGUI()
    {
        if (isDead) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = transform.position + Vector3.up * 2.5f;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0f) return;

        float barW = 80f;
        float barH = 8f;
        float x = screenPos.x - barW * 0.5f;
        float y = Screen.height - screenPos.y - barH * 0.5f;

        Texture2D white = Texture2D.whiteTexture;

        GUI.color = new Color(0.1f, 0.04f, 0.18f, 0.8f);
        GUI.DrawTexture(new Rect(x, y, barW, barH), white);

        float pct = Mathf.Clamp01((float)currentHealth / maxHealth);
        GUI.color = new Color(0.78f, 0.08f, 0.52f, 0.9f);
        GUI.DrawTexture(new Rect(x, y, barW * pct, barH), white);

        GUI.color = Color.white;
    }


}
