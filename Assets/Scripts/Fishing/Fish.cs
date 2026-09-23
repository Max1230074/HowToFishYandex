using System;
using System.Collections;
using UnityEngine;

public enum FishKind
{
    Sardine,
    Herring,
    Mackerel,
    Salmon
}

public struct FishInfo
{
    public string name;
    public Color color;
    public float size;
    public int hp;
    public int value;
    public float weight;
}

public static class FishDefs
{
    public static FishInfo Info(FishKind k)
    {
        switch (k)
        {
            case FishKind.Herring:
                return New("Сельдь", 0x8d, 0xb8, 0x6f, 0.75f, 26, 8, 30f);
            case FishKind.Mackerel:
                return New("Скумбрия", 0x3f, 0xa9, 0x8f, 0.9f, 34, 12, 20f);
            case FishKind.Salmon:
                return New("Лосось", 0xf0, 0x7f, 0x6f, 1.05f, 46, 20, 10f);
            default:
                return New("Сардина", 0x9a, 0xbf, 0xd9, 0.6f, 20, 5, 40f);
        }
    }

    private static FishInfo New(string name, int r, int g, int b, float size, int hp, int value, float weight)
    {
        var fi = new FishInfo();
        fi.name = name;
        fi.color = new Color32((byte)r, (byte)g, (byte)b, 255);
        fi.size = size;
        fi.hp = hp;
        fi.value = value;
        fi.weight = weight;
        return fi;
    }

    public static FishKind PickWeighted()
    {
        float total = 0f;
        foreach (FishKind k in Enum.GetValues(typeof(FishKind))) total += Info(k).weight;
        float r = UnityEngine.Random.value * total;
        foreach (FishKind k in Enum.GetValues(typeof(FishKind)))
        {
            r -= Info(k).weight;
            if (r <= 0f) return k;
        }
        return FishKind.Sardine;
    }
}

public enum FishState
{
    Swimming,
    Hooked,
    CaughtFly,
    Flopping,
    Dead
}

public class Fish : MonoBehaviour
{
    public static event Action<Fish> AnyKilled;

    public FishKind kind;
    public FishState state = FishState.Swimming;
    public int hp;
    public int maxHp;
    public int baseValue;
    public float size;
    public string displayName;
    public int computeValue;
    public float mult;
    public bool wasAirborne;
    public bool oneShot;
    public bool quick;
    public bool sellable { get { return state == FishState.Dead; } }
    public bool carried;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private CircleCollider2D col;
    private Vector2 swimDir;
    private float dirTimer;
    private int hits;
    private float landY;
    private float catchableAt;
    private float hurtCd;
    private float bodyAge;
    private float flyLandY;
    private float fallVy;
    private Color baseColor;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CircleCollider2D>();
        if (transform.childCount > 0) sr = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        var vis = transform.Find("Visual");
        if (vis != null) sr = vis.GetComponent<SpriteRenderer>();
    }

    public void Init(FishKind k)
    {
        kind = k;
        var info = FishDefs.Info(k);
        displayName = info.name;
        maxHp = info.hp;
        hp = maxHp;
        baseValue = info.value;
        size = info.size;
        baseColor = info.color;
        transform.localScale = Vector3.one * size;
        if (sr != null)
        {
            sr.color = baseColor;
            sr.transform.localScale = VisualScale;
        }
        if (col != null) col.radius = 0.5f;
        swimDir = UnityEngine.Random.insideUnitCircle.normalized;
        state = FishState.Swimming;
    }

    private void Update()
    {
        switch (state)
        {
            case FishState.Swimming:
                SwimUpdate();
                break;
            case FishState.Flopping:
                FlopUpdate();
                break;
            case FishState.Dead:
                DeadUpdate();
                break;
        }
    }

    private static readonly Vector3 VisualScale = new Vector3(1f, 0.8f, 1f);

    private void SwimUpdate()
    {
        dirTimer -= Time.deltaTime;
        if (dirTimer <= 0f)
        {
            dirTimer = 1.2f + UnityEngine.Random.value * 1.5f;
            swimDir = UnityEngine.Random.insideUnitCircle.normalized;
        }
        Vector2 p = rb.position;
        p += swimDir * 0.9f * Time.deltaTime;
        p.x = Mathf.Clamp(p.x, World.WaterMinX + 0.8f, World.WaterMaxX - 0.6f);
        p.y = Mathf.Clamp(p.y, World.WaterMinY + 0.8f, World.WaterMaxY - 0.8f);
        rb.MovePosition(p);
        if (sr != null) sr.transform.localScale = VisualScale;
    }

    private void FlopUpdate()
    {
        float speed = 1.7f;
        Vector3 p = transform.position;
        p.x -= speed * Time.deltaTime;
        p.y = landY + Mathf.Sin(Time.time * 9f) * 0.07f;
        rb.MovePosition(p);

        if (sr != null) sr.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 18f) * 18f);

        if (hurtCd > 0f) hurtCd -= Time.deltaTime;
        var pc = PlayerController.Instance;
        if (pc != null && hurtCd <= 0f && Vector2.Distance(transform.position, pc.transform.position) < 1.1f)
        {
            pc.TakeDamage(5f);
            hurtCd = 1.4f;
        }

        if (p.x < World.ShoreX + 0.3f)
        {
            EscapeToWater();
        }
    }

    private void DeadUpdate()
    {
        if (carried)
        {
            if (sr != null)
            {
                sr.color = baseColor;
                sr.transform.rotation = Quaternion.identity;
            }
            return;
        }
        if (wasAirborne && transform.position.y > flyLandY + 0.05f)
        {
            fallVy += 14f * Time.deltaTime;
            Vector2 fp = rb.position;
            fp.y -= fallVy * Time.deltaTime;
            if (fp.y < flyLandY) fp.y = flyLandY;
            rb.MovePosition(fp);
        }
        if (sr != null) sr.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 90f) * 8f);
        if (transform.position.y <= flyLandY + 0.05f)
        {
            bodyAge += Time.deltaTime;
        }

        if (sr != null)
        {
            float a = 1f;
            if (bodyAge > 45f) a = 1f - Mathf.Clamp01((bodyAge - 45f) / 8f);
            else a = (Mathf.Sin(Time.time * 6f) > 0f) ? 1f : 0.85f;
            var c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, a);
        }
        if (bodyAge > 53f)
        {
            Destroy(gameObject);
        }
    }

    public void BeginHooked()
    {
        state = FishState.Hooked;
        hits = 0;
        if (col != null) col.enabled = true;
        if (sr != null)
        {
            sr.color = baseColor;
            sr.transform.rotation = Quaternion.identity;
        }
    }

    public void FollowToward(Vector2 pos)
    {
        if (state != FishState.Hooked) return;
        Vector2 dir = (pos - rb.position).normalized;
        float speed = 3.2f + GameState.rodLevel * 2f;
        rb.MovePosition(Vector2.MoveTowards(rb.position, pos, speed * Time.deltaTime));
        if (sr != null) sr.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 26f) * 14f);
    }

    public void StartCaughtFly(Vector3 land)
    {
        StartCoroutine(FlyToLand(land));
    }

    private IEnumerator FlyToLand(Vector3 land)
    {
        state = FishState.CaughtFly;
        wasAirborne = true;
        catchableAt = Time.time;
        flyLandY = land.y;
        Vector3 a = transform.position;
        float dur = 0.85f;
        float t = 0f;
        while (t < 1f)
        {
            if (state == FishState.Dead) yield break;
            t += Time.deltaTime / dur;
            t = Mathf.Clamp01(t);
            Vector3 p = Vector3.Lerp(a, land, t);
            p.y += Mathf.Sin(t * Mathf.PI) * 2.2f;
            rb.MovePosition(p);
            yield return null;
        }
        rb.MovePosition(new Vector3(land.x, land.y, 0f));
        flyLandY = land.y;
        landY = land.y;
        state = FishState.Flopping;
        if (sr != null) sr.color = baseColor;
    }

    public void EscapeToWater()
    {
        state = FishState.Swimming;
        hits = 0;
        Vector2 p = new Vector2(
            UnityEngine.Random.Range(World.WaterMinX + 4f, World.WaterMaxX - 1f),
            UnityEngine.Random.Range(World.WaterMinY + 1.5f, World.WaterMaxY - 1.5f));
        rb.MovePosition(p);
        if (sr != null)
        {
            sr.color = baseColor;
            sr.transform.rotation = Quaternion.identity;
        }
        FloatingText.Flash(transform.position, new Color32(0x8f, 0xd3, 0xff, 220), 0.9f);
    }

    public void Damage(int dmg)
    {
        if (state == FishState.Dead || state == FishState.Swimming) return;
        hits++;
        FastGhostHits(dmg);
        if (hp <= 0) Die();
    }

    private void FastGhostHits(int dmg)
    {
        hp -= dmg;
        if (hp < 0) hp = 0;
        if (sr != null) StartCoroutine(Punch());
    }

    private IEnumerator Punch()
    {
        var vis = sr.transform;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.15f;
            t = Mathf.Clamp01(t);
            float s = 1f + Mathf.Sin(t * Mathf.PI) * 0.25f;
            vis.localScale = new Vector3(VisualScale.x * s, VisualScale.y * s, 1f);
            yield return null;
        }
        vis.localScale = VisualScale;
    }

    public void Die()
    {
        state = FishState.Dead;
        oneShot = hits == 1;
        quick = (Time.time - catchableAt) <= 3.5f;
        mult = KillScore.Compute(this);
        computeValue = Mathf.RoundToInt(baseValue * mult);
        bodyAge = 0f;
        landY = transform.position.y;

        if (sr != null)
        {
            sr.color = new Color(baseColor.r * 0.7f, baseColor.g * 0.7f, baseColor.b * 0.7f, 1f);
            sr.transform.rotation = Quaternion.identity;
        }

        string msg = mult >= 1.5f ? "x" + mult.ToString("0.##") + "  $" + computeValue : "+$" + computeValue;
        Color c = mult >= 1.5f ? Palette.Gold : Palette.White;
        FloatingText.Create(transform.position + Vector3.up * 0.6f, msg, c);

        if (AnyKilled != null) AnyKilled(this);
    }

    public void BeginCarry(Transform slot)
    {
        carried = true;
        state = FishState.Dead;
        transform.SetParent(slot, false);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one * size * 0.45f;
        if (col != null) col.enabled = false;
        if (sr != null)
        {
            sr.color = baseColor;
            sr.transform.rotation = Quaternion.identity;
            sr.transform.localScale = VisualScale;
        }
    }

    public void EndCarry(Vector3 pos)
    {
        carried = false;
        transform.SetParent(null, true);
        transform.position = pos;
        transform.localScale = Vector3.one * size;
        if (col != null) col.enabled = true;
        if (sr != null)
        {
            sr.color = baseColor;
            sr.transform.localScale = VisualScale;
        }
    }

    public void SellFromHand()
    {
        Destroy(gameObject);
    }
}