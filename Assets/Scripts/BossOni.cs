using System.Collections;
using UnityEngine;

public class BossOni : Enemy
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 0.4f;

    [Header("Shrine Detection")]
    [SerializeField] private float attackRange = 1.5f;

    [Header("Health")]
    [SerializeField] private int bossMaxHealth = 45;

    private Shrine shrine;
    private Rigidbody2D rb;

    protected override void Awake()
    {
        base.Awake();
        InitHealth(bossMaxHealth);
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject shrineObj = GameObject.Find("Shrine");
        if (shrineObj != null)
            shrine = shrineObj.GetComponent<Shrine>();

        baseColor = Color.red;
        sr.color = Color.red;
    }

    private void Update()
    {
        if (isDead || shrine == null) return;

        float dist = Vector2.Distance(transform.position, shrine.transform.position);
        if (dist <= attackRange)
        {
            if (shrine != null)
                shrine.TakeDamage(shrine.maxHealth);
        }
    }

    private void FixedUpdate()
    {
        if (isDead || rb == null || shrine == null) return;

        Vector2 direction = ((Vector2)shrine.transform.position - rb.position).normalized;

        float dist = Mathf.Abs(direction.x);
        if (dist > attackRange * 0.5f)
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
        else
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (sr != null && Mathf.Abs(direction.x) > 0.1f)
            sr.flipX = direction.x < 0;
    }

    protected override IEnumerator DieRoutine()
    {
        isDead = true;

        if (animator != null)
            animator.SetFloat("Dead", 1f);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        yield return new WaitForSeconds(1f);

        if (waveSpawner != null)
            waveSpawner.OnEnemyDied();

        enabled = false;
    }
}
