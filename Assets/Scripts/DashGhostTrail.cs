using UnityEngine;

public class DashGhostTrail : MonoBehaviour
{
    [SerializeField] private Color ghostColor = new Color(0.78f, 0.08f, 0.52f);
    [SerializeField] private float fadeDuration = 0.3f;
    [SerializeField] private int poolSize = 5;

    private GameObject[] ghosts;
    private SpriteRenderer[] ghostRenderers;
    private float[] ghostTimers;

    public void Initialize()
    {
        ghosts = new GameObject[poolSize];
        ghostRenderers = new SpriteRenderer[poolSize];
        ghostTimers = new float[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject("DashGhost");
            go.transform.SetParent(transform);
            go.SetActive(false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = -1;
            sr.color = ghostColor;
            ghosts[i] = go;
            ghostRenderers[i] = sr;
            ghostTimers[i] = 0f;
        }
    }

    public void SpawnGhost(Vector2 position, Sprite sprite, bool flipX)
    {
        int slot = FindSlot();
        ghosts[slot].transform.position = position;
        ghostRenderers[slot].sprite = sprite;
        ghostRenderers[slot].flipX = flipX;
        Color c = ghostColor;
        c.a = 0.6f;
        ghostRenderers[slot].color = c;
        ghosts[slot].SetActive(true);
        ghostTimers[slot] = fadeDuration;
    }

    private int FindSlot()
    {
        int best = 0;
        float lowest = ghostTimers[0];
        for (int i = 1; i < poolSize; i++)
        {
            if (!ghosts[i].activeSelf)
                return i;
            if (ghostTimers[i] < lowest)
            {
                lowest = ghostTimers[i];
                best = i;
            }
        }
        return best;
    }

    private void Update()
    {
        for (int i = 0; i < poolSize; i++)
        {
            if (!ghosts[i].activeSelf)
                continue;

            ghostTimers[i] -= Time.deltaTime;
            if (ghostTimers[i] <= 0f)
            {
                ghosts[i].SetActive(false);
            }
            else
            {
                float t = ghostTimers[i] / fadeDuration;
                Color c = ghostRenderers[i].color;
                c.a = t * 0.6f;
                ghostRenderers[i].color = c;
            }
        }
    }
}
