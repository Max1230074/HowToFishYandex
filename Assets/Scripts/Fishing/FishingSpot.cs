using UnityEngine;

public class FishingSpot : MonoBehaviour
{
    public static FishingSpot Instance { get; private set; }

    public int maxFish = 6;
    private float respawnTimer;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < maxFish; i++)
        {
            SpawnFish();
        }
    }

    private void Update()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var f = transform.GetChild(i).GetComponent<Fish>();
            if (f == null) f = transform.GetChild(i).GetComponentInChildren<Fish>();
            if (f == null)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
        }
        respawnTimer += Time.deltaTime;
        if (respawnTimer > 2f)
        {
            respawnTimer = 0f;
            CountSpawn();
        }
    }

    private void CountSpawn()
    {
        int count = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            var f = transform.GetChild(i).GetComponentInChildren<Fish>();
            if (f != null && f.state != FishState.Dead) count++;
        }
        while (count < maxFish)
        {
            SpawnFish();
            count++;
        }
    }

    public void SpawnFish()
    {
        var go = new GameObject("Fish");
        go.transform.SetParent(transform, true);
        go.transform.position = RandomWaterPos();

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;

        var vis = new GameObject("Visual");
        vis.transform.SetParent(go.transform, false);
        var sr = vis.AddComponent<SpriteRenderer>();
        sr.sprite = Palette.Circle(Palette.White);
        sr.sortingOrder = 2;

        var fish = go.AddComponent<Fish>();
        fish.Init(FishDefs.PickWeighted());
    }

    private Vector2 RandomWaterPos()
    {
        return new Vector2(
            Random.Range(World.WaterMinX + 1.4f, World.WaterMaxX - 0.5f),
            Random.Range(World.WaterMinY + 1.2f, World.WaterMaxY - 1.2f));
    }

    public bool IsWater(Vector3 p)
    {
        return p.x >= World.WaterMinX && p.x <= World.WaterMaxX &&
               p.y >= World.WaterMinY && p.y <= World.WaterMaxY;
    }

    public Fish FindNearest(Vector2 pos, float maxDist)
    {
        Fish best = null;
        float bestD = maxDist * maxDist;
        for (int i = 0; i < transform.childCount; i++)
        {
            var f = transform.GetChild(i).GetComponentInChildren<Fish>();
            if (f == null || f.state != FishState.Swimming) continue;
            float d = ((Vector2)f.transform.position - pos).sqrMagnitude;
            if (d < bestD)
            {
                bestD = d;
                best = f;
            }
        }
        return best;
    }
}