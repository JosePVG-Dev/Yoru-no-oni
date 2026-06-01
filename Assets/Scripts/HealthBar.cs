using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private float barWidth = 1.2f;
    [SerializeField] private float barHeight = 0.12f;
    [SerializeField] private float yOffset = 1.8f;
    [SerializeField] private Color bgColor = new Color(0.1f, 0.04f, 0.18f, 0.8f);
    [SerializeField] private Color fillColor = new Color(0.78f, 0.08f, 0.52f, 0.9f);

    private IHasHealth healthOwner;
    private SpriteRenderer bgRenderer;
    private SpriteRenderer fillRenderer;
    private Transform fillTransform;

    private void Awake()
    {
        healthOwner = GetComponentInParent<Enemy>();
        if (healthOwner == null) healthOwner = GetComponentInParent<Shrine>();
        if (healthOwner == null)
        {
            Debug.LogWarning($"[HealthBar] No IHasHealth component found in parent of {name}");
            Destroy(gameObject);
            return;
        }

        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        Sprite bgSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);

        GameObject bgObj = new GameObject("BG");
        bgObj.transform.SetParent(transform, false);
        bgObj.transform.localPosition = new Vector3(0f, yOffset, 0f);
        bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = bgSprite;
        bgRenderer.color = bgColor;
        bgRenderer.sortingOrder = 5;
        bgObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);

        Sprite fillSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 100f);

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(bgObj.transform, false);
        fillObj.transform.localPosition = new Vector3(-0.5f, 0f, 0f);
        fillRenderer = fillObj.AddComponent<SpriteRenderer>();
        fillRenderer.sprite = fillSprite;
        fillRenderer.color = fillColor;
        fillRenderer.sortingOrder = 6;
        fillTransform = fillObj.transform;
        fillTransform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (healthOwner == null) return;

        float pct = Mathf.Clamp01((float)healthOwner.Health / healthOwner.MaxHealth);
        Vector3 s = fillTransform.localScale;
        s.x = pct;
        fillTransform.localScale = s;
    }
}
