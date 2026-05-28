using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int health = 3;

    public void TakeDamage(int amount)
    {
        health -= amount;
        Debug.Log($"[Enemy] {name} took {amount} damage, health: {health}");
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"[Enemy] {name} destroyed");
        Destroy(gameObject);
    }
}
