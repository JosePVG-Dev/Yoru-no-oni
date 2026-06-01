using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour, IHasHealth
{
    [SerializeField] protected int maxHealth = 3;
    private int health;
    
    private Coroutine flashCoroutine;
    protected Animator animator;
    protected WaveSpawner waveSpawner;
    protected bool isDead = false;
    protected SpriteRenderer sr;
    protected Color baseColor = Color.white;

    public int Health => health;
    public int MaxHealth => maxHealth;
    public WaveSpawner WaveSpawner { set => waveSpawner = value; }

    protected void InitHealth(int newMaxHealth)
    {
        maxHealth = newMaxHealth;
        health = newMaxHealth;
    }

    protected virtual void Awake()
    {
        health = maxHealth;

        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

public void TakeDamage(int amount)
    {
        if (isDead) return;

        health -= amount;
        Debug.Log($"[Enemy] {name} took {amount} damage, health: {health}");

        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashDamage());
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayAttackHit();

        if (health <= 0)
        {
            StartCoroutine(DieRoutine());
        }
    }

    protected virtual System.Collections.IEnumerator DieRoutine()
    {
        isDead = true;

        var rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.linearVelocity = new Vector2(0, rb2d.linearVelocity.y);
            rb2d.gravityScale = 2f;
        }

        if (animator != null)
            animator.SetFloat("Dead", 1f);

        yield return new WaitForSeconds(1f);

        if (waveSpawner != null)
            waveSpawner.OnEnemyDied();

        if (rb2d != null)
        {
            rb2d.simulated = false;
        }

        enabled = false;
    }


private IEnumerator FlashDamage()
    {
        if (sr == null) yield break;
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        float elapsed = 0f;
        float duration = 0.15f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            sr.color = Color.Lerp(Color.red, baseColor, elapsed / duration);
            yield return null;
        }
        sr.color = baseColor;
    }

    private void OnGUI()
    {
        if (isDead) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = transform.position + Vector3.up * 2.5f;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        if (screenPos.z < 0f) return;

        float barW = 70f;
        float barH = 8f;
        float x = screenPos.x - barW * 0.5f;
        float y = Screen.height - screenPos.y - barH * 0.5f;

        Texture2D white = Texture2D.whiteTexture;

        GUI.color = bgColor;
        GUI.DrawTexture(new Rect(x, y, barW, barH), white);

        float pct = Mathf.Clamp01((float)health / maxHealth);
        GUI.color = new Color(0.78f, 0.08f, 0.52f, 0.9f);
        GUI.DrawTexture(new Rect(x, y, barW * pct, barH), white);

        GUI.color = Color.white;
    }

    private static readonly Color bgColor = new Color(0.1f, 0.04f, 0.18f, 0.8f);
}
