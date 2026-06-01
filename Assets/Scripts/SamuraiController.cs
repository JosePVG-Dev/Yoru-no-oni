using UnityEngine;
using UnityEngine.InputSystem;

using TMPro;
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
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float attackHeight = 2f;
    [SerializeField] private float attackForwardOffset = 1.5f;
    [SerializeField] private LayerMask enemyLayer = ~0;
    [SerializeField] private float attackCooldown = 0.35f;

    [Header("Audio")]


    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    
    [SerializeField] private TextMeshProUGUI[] healthHearts;
    private Color heartFullColor = new Color(0.78f, 0.082f, 0.522f, 1f);
    private Color heartEmptyColor = new Color(0.176f, 0.063f, 0.306f, 1f);
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
    private float attackCooldownTimer;
    private float dashCooldownTimer;
    private float dashBarFullWidth;
    private bool isDashing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        inputActions = new InputSystem_Actions();

        

        for (int i = 0; i < healthHearts?.Length; i++)
            if (healthHearts[i] != null)
                healthHearts[i].color = heartFullColor;
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
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayJump();
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
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayDash();
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
        if (attackCooldownTimer > 0f) return;
        attackCooldownTimer = attackCooldown;

        if (animator != null)
            animator.SetTrigger("Attack");
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayAttackSlash();

        Vector2 rectCenter = (Vector2)transform.position
            + Vector2.right * facingDirection * attackForwardOffset;

        Vector2 rectSize = new Vector2(attackRange, attackHeight);

        Collider2D[] hits = Physics2D.OverlapBoxAll(rectCenter, rectSize, 0f, enemyLayer);
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
                enemy.TakeDamage(1);
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

        if (attackCooldownTimer > 0f)
        {
            attackCooldownTimer -= Time.deltaTime;
            if (attackCooldownTimer < 0f) attackCooldownTimer = 0f;
        }

        if (dashCooldownBar != null)
        {
            float progress = 1f - (dashCooldownTimer / dashCooldown);
            dashCooldownBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, progress * dashBarFullWidth);
        }

        

        if (healthHearts != null)
        {
            for (int i = 0; i < healthHearts.Length; i++)
            {
                if (healthHearts[i] != null)
                    healthHearts[i].color = i < currentHealth ? heartFullColor : heartEmptyColor;
            }
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

        if (spriteRenderer != null)
            StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Debug.Log("[Samurai] Defeated!");
            var gameOver = FindFirstObjectByType<GameOverUI>();
            if (gameOver != null)
                gameOver.Show("El Samurai ha caido");
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public bool IsInvulnerable => isInvulnerable;


private System.Collections.IEnumerator FlashDamage()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        float elapsed = 0f;
        float duration = 0.15f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            spriteRenderer.color = Color.Lerp(Color.red, Color.white, elapsed / duration);
            yield return null;
        }
        spriteRenderer.color = Color.white;
    }
}
