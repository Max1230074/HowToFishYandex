using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    public float speed = 5f;
    public float maxHp = 100f;
    public float hp;
    public bool OnCast;

    public Vector2 Facing { get; private set; }
    public Fish carriedFish;
    public bool InShopZone;

    private Rigidbody2D rb;
    private Camera cam;
    private FishingSpot spot;
    private HUD hud;
    private RodController rod;
    private Transform carrySlot;
    private Vector3 startPos;
    private float hurtCd;
    private float hpRegenTimer;

    private void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        cam = Camera.main;
        spot = FindObjectOfType<FishingSpot>();
        hud = HUD.Instance;
        rod = GetComponent<RodController>();
        carrySlot = transform.Find("CarrySlot");
        startPos = World.PlayerStart;
        hp = maxHp;
        transform.position = startPos;
        if (hud != null) hud.SetHp(hp, maxHp);
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        Vector2 dir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (dir.sqrMagnitude > 1f) dir = dir.normalized;

        float mult = 1f;
        if (spot != null && spot.IsWater(transform.position)) mult = 0.35f;
        rb.velocity = dir * speed * mult;

        Vector3 p = transform.position;
        p.x = Mathf.Clamp(p.x, World.PlayerMinX, World.PlayerMaxX);
        p.y = Mathf.Clamp(p.y, World.PlayerMinY, World.PlayerMaxY);
        rb.MovePosition(p);

        if (cam != null)
        {
            Vector3 m = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mw = new Vector2(m.x, m.y);
            Vector2 f = mw - (Vector2)transform.position;
            if (f.sqrMagnitude > 0.01f) Facing = f.normalized;
        }

        if (carriedFish == null && rod != null && rod.state != RodState.Casting)
        {
            Fish f;
            if (FindDeadNear(out f, 1.3f)) PickUp(f);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            var shop = ShopPanel.Instance;
            if (shop != null)
            {
                if (InShopZone)
                {
                    if (shop.IsOpen && carriedFish != null)
                    {
                        shop.SellButtonClicked();
                    }
                    else
                    {
                        shop.Toggle();
                    }
                }
                else if (carriedFish != null)
                {
                    DropFish();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F) && carriedFish != null)
        {
            DropFish();
        }

        if (hurtCd > 0f) hurtCd -= Time.deltaTime;

        hpRegenTimer += Time.deltaTime;
        if (hpRegenTimer >= 2f && hp < maxHp)
        {
            hpRegenTimer = 0f;
            hp = Mathf.Min(maxHp, hp + 1f);
            if (hud != null) hud.SetHp(hp, maxHp);
        }
    }

    private bool FindDeadNear(out Fish fish, float radius)
    {
        fish = null;
        var cols = Physics2D.OverlapCircleAll(transform.position, radius);
        float best = float.MaxValue;
        for (int i = 0; i < cols.Length; i++)
        {
            var f = cols[i].GetComponent<Fish>();
            if (f == null || !f.sellable) continue;
            float d = ((Vector2)f.transform.position - (Vector2)transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                fish = f;
            }
        }
        return fish != null;
    }

    public void PickUp(Fish f)
    {
        carriedFish = f;
        f.BeginCarry(carrySlot);
    }

    public void DropFish()
    {
        if (carriedFish == null) return;
        var f = carriedFish;
        carriedFish = null;
        f.EndCarry(transform.position + (Vector3)Facing * 0.9f);
    }

    public void TakeDamage(float dmg)
    {
        if (hurtCd > 0f || hp <= 0f) return;
        hp -= dmg;
        if (hud != null) hud.SetHp(hp, maxHp);
        if (hud != null) hud.Flash();
        if (hp <= 0f) Die();
    }

    public void Heal(float amount)
    {
        hp = Mathf.Min(maxHp, hp + amount);
        if (hud != null) hud.SetHp(hp, maxHp);
    }

    public bool IsFullHp()
    {
        return hp >= maxHp;
    }

    private void Die()
    {
        hp = maxHp;
        int lost = Mathf.FloorToInt(GameState.Money * 0.2f);
        if (lost > 0) GameState.Spend(lost);
        if (carriedFish != null) DropFish();
        if (rod != null) rod.ReleaseRod();
        transform.position = startPos;
        if (hud != null)
        {
            hud.SetHp(hp, maxHp);
            hud.Hint("Вы потеряли сознание и $" + lost + ". Осмотритесь: WASD, Space — удочка.", 5f);
        }
    }
}