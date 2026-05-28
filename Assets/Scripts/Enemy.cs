using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int health = 3;
    protected Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"[Enemy] {name} took {amount} damage, health: {health}");
        if (health <= 0)
        {
            StartCoroutine(DieRoutine());
        }
    }

    private void Die()
    {
        StartCoroutine(DieRoutine());
    }

    protected virtual System.Collections.IEnumerator DieRoutine()
    {
        if (animator != null)
            animator.SetFloat("Dead", 1f);
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}
