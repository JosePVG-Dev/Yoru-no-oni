using UnityEngine;

public enum OniType { Balanced, Fast, Tank, Jumper }

public class OniAI : Enemy
{
    private enum State { WalkingToShrine, Fighting, AttackingShrine, Dead }

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [SerializeField] private OniType oniType = OniType.Balanced;

    [Header("Detection")]
    [SerializeField] private float detectionRadius = 3f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 1;

    [Header("Jumper")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float jumpInterval = 1.5f;

    private State currentState;
    private Transform shrine;
    private Transform samurai;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float attackTimer;
    private float jumpTimer;

    private static System.Collections.Generic.HashSet<Collider2D> _allOniColliders = new System.Collections.Generic.HashSet<Collider2D>();

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            _allOniColliders.RemoveWhere(c => c == null);
            foreach (var other in _allOniColliders)
                Physics2D.IgnoreCollision(col, other);
            _allOniColliders.Add(col);
        }
    }

    protected override System.Collections.IEnumerator DieRoutine()
    {
        currentState = State.Dead;
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

        switch (oniType)
        {
            case OniType.Fast:
                InitHealth(1);
                moveSpeed = 5f;
                baseColor = new Color(1f, 0.5f, 0.1f);
                break;
            case OniType.Tank:
                InitHealth(8);
                moveSpeed = 1.5f;
                baseColor = new Color(1f, 0.8f, 0.15f);
                attackDamage = 2;
                break;
            case OniType.Jumper:
                InitHealth(2);
                moveSpeed = 2.5f;
                baseColor = new Color(0.3f, 0.9f, 0.2f);
                jumpTimer = jumpInterval;
                break;
            default:
                InitHealth(3);
                moveSpeed = 2f;
                baseColor = Color.white;
                break;
        }

        sr.color = baseColor;
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
        if (isDead || rb == null) return;
        MoveTowardsTarget();

        if (oniType == OniType.Jumper)
        {
            jumpTimer -= Time.fixedDeltaTime;
            if (jumpTimer <= 0f && Mathf.Abs(rb.linearVelocity.y) < 0.1f &&
                (currentState == State.WalkingToShrine || currentState == State.Fighting))
            {
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                jumpTimer = jumpInterval;
            }
        }
    }

    private void UpdateWalkingToShrine()
    {
        if (shrine == null) return;

        float distToShrine = Vector2.Distance(transform.position, shrine.position);
        float xDistToShrine = Mathf.Abs(transform.position.x - shrine.position.x);

        if (oniType != OniType.Tank && samurai != null)
        {
            float distToSamurai = Vector2.Distance(transform.position, samurai.position);
            if (distToSamurai <= detectionRadius && distToSamurai < distToShrine)
            {
                currentState = State.Fighting;
                return;
            }
        }

        if (xDistToShrine <= attackRange)
        {
            currentState = State.AttackingShrine;
        }
    }

    private void UpdateFighting()
    {
        if (oniType == OniType.Tank)
        {
            currentState = State.AttackingShrine;
            return;
        }

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

        float xDistToShrine = Mathf.Abs(transform.position.x - shrine.position.x);
        if (xDistToShrine <= attackRange && attackTimer <= 0f)
        {
            AttackTarget(shrine);
            attackTimer = attackCooldown;
        }
    }

    private void MoveTowardsTarget()
    {
        Transform target = GetCurrentTarget();
        if (target == null) return;

        Vector2 direction = ((Vector2)target.position - rb.position);
        float dist = direction.magnitude;

        bool shouldMove;
        if (currentState == State.WalkingToShrine || currentState == State.AttackingShrine)
            shouldMove = Mathf.Abs(direction.x) > attackRange;
        else
            shouldMove = dist > attackRange;

        if (shouldMove)
        {
            rb.linearVelocity = new Vector2(direction.normalized.x * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        if (Mathf.Abs(direction.x) > 0.1f)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
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
