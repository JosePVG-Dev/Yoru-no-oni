using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class SamuraiController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 3f;
    [SerializeField] private float dashDuration = 0.12f;
    [SerializeField] private int dashInvulnerableFrames = 2;
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] private RectTransform dashCooldownBar;
    [SerializeField] private DashGhostTrail dashGhostTrail;

    [Header("Attack")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackConeAngle = 60f;
    [SerializeField] private Vector2 attackOriginOffset = new Vector2(0f, 0.5f);
    [SerializeField] private LayerMask enemyLayer = ~0;

    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    private int currentHealth;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private int facingDirection = 1;
    private bool isGrounded;
    private bool isInvulnerable;
    private int invulnerableFrameCounter;
    private float dashCooldownTimer;
    private float dashBarFullWidth;
    private bool isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inputActions = new InputSystem_Actions();

        currentHealth = maxHealth;

        if (groundCheckPoint == null)
            groundCheckPoint = transform.Find("GroundCheck");
        if (groundLayer == 0)
            groundLayer = LayerMask.GetMask("Ground");
        if (enemyLayer == 0 || enemyLayer == ~0)
            enemyLayer = LayerMask.GetMask("Enemy");

        if (dashCooldownBar != null)
            dashBarFullWidth = dashCooldownBar.sizeDelta.x;

        if (dashGhostTrail != null)
            dashGhostTrail.Initialize();
    }

    private void OnEnable()
    {
        if (inputActions == null) return;
        inputActions.Player.Enable();
        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Dash.performed += OnDash;
        inputActions.Player.Attack.performed += OnAttack;
    }

    private void OnDisable()
    {
        if (inputActions == null) return;
        inputActions.Player.Move.performed -= OnMove;
        inputActions.Player.Move.canceled -= OnMove;
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Dash.performed -= OnDash;
        inputActions.Player.Attack.performed -= OnAttack;
        inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        if (inputActions != null)
            inputActions.Dispose();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isGrounded = false;
            if (animator != null)
                animator.SetBool("isGrounded", false);
        }
    }

    private void OnDash(InputAction.CallbackContext ctx)
    {
        if (dashCooldownTimer > 0f || isDashing) return;

        float direction = moveInput.x != 0f ? Mathf.Sign(moveInput.x) : facingDirection;
        dashCooldownTimer = dashCooldown;
        isDashing = true;
        isInvulnerable = true;
        if (animator != null)
            animator.SetTrigger("Dash");
        StartCoroutine(DashCoroutine(direction));
    }

    private IEnumerator DashCoroutine(float direction)
    {
        Vector2 startPos = rb.position;
        Vector2 endPos = startPos + Vector2.right * direction * dashDistance;
        float elapsed = 0f;
        float ghostInterval = dashDuration / 5f;
        float nextGhostTime = ghostInterval;

        while (elapsed < dashDuration)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / dashDuration);
            rb.MovePosition(Vector2.Lerp(startPos, endPos, t));

            if (elapsed >= nextGhostTime && dashGhostTrail != null)
            {
                dashGhostTrail.SpawnGhost(rb.position, spriteRenderer.sprite, facingDirection == -1);
                nextGhostTime += ghostInterval;
            }

            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(endPos);
        if (dashGhostTrail != null)
            dashGhostTrail.SpawnGhost(endPos, spriteRenderer.sprite, facingDirection == -1);
        isDashing = false;
        isInvulnerable = false;
    }

    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (animator != null)
            animator.SetTrigger("Attack");

        Vector2 origin = (Vector2)transform.position + attackOriginOffset;
        Vector2 attackDir = facingDirection == 1 ? Vector2.right : Vector2.left;

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, attackRange, enemyLayer);
        foreach (var hit in hits)
        {
            Vector2 dirToTarget = ((Vector2)hit.transform.position - origin).normalized;
            float angle = Vector2.Angle(attackDir, dirToTarget);
            if (angle <= attackConeAngle / 2f)
            {
                var enemy = hit.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(1);
                }
            }
        }
    }

    private void Update()
    {
        if (isInvulnerable && !isDashing)
        {
            invulnerableFrameCounter--;
            if (invulnerableFrameCounter <= 0)
            {
                isInvulnerable = false;
            }
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer < 0f) dashCooldownTimer = 0f;
        }

        if (dashCooldownBar != null)
        {
            float progress = 1f - (dashCooldownTimer / dashCooldown);
            dashCooldownBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, progress * dashBarFullWidth);
        }

        UpdateFacingDirection();
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (animator == null) return;
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("isGrounded", isGrounded);
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        ApplyMovement();
    }

    private void CheckGrounded()
    {
        if (groundCheckPoint != null)
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    private void ApplyMovement()
    {
        if (isDashing) return;
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    private void UpdateFacingDirection()
    {
        if (moveInput.x > 0f)
            facingDirection = 1;
        else if (moveInput.x < 0f)
            facingDirection = -1;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -facingDirection;
        transform.localScale = scale;
    }

    public void TakeDamage(int amount)
    {
        if (isInvulnerable) return;
        currentHealth -= amount;
        Debug.Log($"[Samurai] Took {amount} damage, health: {currentHealth}/{maxHealth}");
        if (currentHealth <= 0)
        {
            Debug.Log("[Samurai] Defeated!");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public bool IsInvulnerable => isInvulnerable;
}
