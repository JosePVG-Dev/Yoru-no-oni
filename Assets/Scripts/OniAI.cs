using UnityEngine;

public class OniAI : Enemy
{
    private enum State { WalkingToShrine, Fighting, AttackingShrine, Dead }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 6f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 1;

    private State currentState;
    private Transform shrine;
    private Transform samurai;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float attackTimer;
    private bool isDead;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override System.Collections.IEnumerator DieRoutine()
    {
        isDead = true;
        yield return base.DieRoutine();
    }

    private void Start()
    {
        GameObject shrineObj = GameObject.Find("Shrine");
        if (shrineObj != null)
            shrine = shrineObj.transform;

        GameObject samuraiObj = GameObject.Find("Samurai");
        if (samuraiObj != null)
            samurai = samuraiObj.transform;

        currentState = State.WalkingToShrine;
    }

    private void Update()
    {
        if (isDead) return;

        attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case State.WalkingToShrine:
                UpdateWalkingToShrine();
                break;
            case State.Fighting:
                UpdateFighting();
                break;
            case State.AttackingShrine:
                UpdateAttackingShrine();
                break;
        }

        UpdateAnimation();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        MoveTowardsTarget();
    }

    private void UpdateWalkingToShrine()
    {
        if (shrine == null) return;

        if (samurai != null)
        {
            float distToSamurai = Vector2.Distance(transform.position, samurai.position);
            if (distToSamurai <= detectionRadius)
            {
                currentState = State.Fighting;
                Debug.Log($"[OniAI] {name} detected Samurai, switching to Fighting");
                return;
            }
        }

        float distToShrine = Vector2.Distance(transform.position, shrine.position);
        if (distToShrine <= attackRange)
        {
            currentState = State.AttackingShrine;
            Debug.Log($"[OniAI] {name} reached Shrine, attacking!");
        }
    }

    private void UpdateFighting()
    {
        if (samurai == null)
        {
            currentState = State.WalkingToShrine;
            return;
        }

        float distToSamurai = Vector2.Distance(transform.position, samurai.position);
        if (distToSamurai > detectionRadius * 1.5f)
        {
            currentState = State.WalkingToShrine;
            Debug.Log($"[OniAI] {name} lost Samurai, returning to Shrine");
            return;
        }

        if (distToSamurai <= attackRange && attackTimer <= 0f)
        {
            AttackTarget(samurai);
            attackTimer = attackCooldown;
        }

        if (shrine != null)
        {
            float distToShrine = Vector2.Distance(transform.position, shrine.position);
            if (distToShrine <= attackRange && attackTimer <= 0f)
            {
                AttackTarget(shrine);
                attackTimer = attackCooldown;
            }
        }
    }

    private void UpdateAttackingShrine()
    {
        if (shrine == null) return;

        float distToShrine = Vector2.Distance(transform.position, shrine.position);
        if (distToShrine <= attackRange && attackTimer <= 0f)
        {
            AttackTarget(shrine);
            attackTimer = attackCooldown;
        }

        if (samurai != null)
        {
            float distToSamurai = Vector2.Distance(transform.position, samurai.position);
            if (distToSamurai <= detectionRadius)
            {
                currentState = State.Fighting;
                Debug.Log($"[OniAI] {name} Samurai near Shrine, switching to Fighting");
            }
        }
    }

    private void MoveTowardsTarget()
    {
        Transform target = GetCurrentTarget();
        if (target == null) return;

        Vector2 direction = ((Vector2)target.position - rb.position);
        float dist = direction.magnitude;

        if ((currentState == State.WalkingToShrine || currentState == State.Fighting) && dist > attackRange)
        {
            rb.linearVelocity = new Vector2(direction.normalized.x * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        if (Mathf.Abs(direction.x) > 0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction.x > 0 ? -1 : 1);
            transform.localScale = scale;
        }
    }

    private Transform GetCurrentTarget()
    {
        switch (currentState)
        {
            case State.Fighting:
                return samurai;
            case State.WalkingToShrine:
            case State.AttackingShrine:
                return shrine;
            default:
                return null;
        }
    }

    private void AttackTarget(Transform target)
    {
        if (target == null) return;

        var shrineComp = target.GetComponent<Shrine>();
        if (shrineComp != null)
        {
            shrineComp.TakeDamage(attackDamage);
            return;
        }

        var samuraiController = target.GetComponent<SamuraiController>();
        if (samuraiController != null && !samuraiController.IsInvulnerable)
        {
            samuraiController.TakeDamage(attackDamage);
        }
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;
        float speed = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", speed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
