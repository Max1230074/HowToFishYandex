using UnityEngine;

public class PlayerWeapons : MonoBehaviour
{
    public int dmgFist = 8;
    public int dmgKnife = 35;
    public int dmgPistol = 45;
    public float rangeFist = 1f;
    public float rangeKnife = 1.6f;
    public float rangePistol = 9f;
    public float cdFist = 0.22f;
    public float cdKnife = 0.3f;
    public float cdPistol = 0.55f;

    public int selected;
    public string CurrentName
    {
        get
        {
            switch (selected)
            {
                case 1: return "Нож";
                case 2: return "Пистолет";
                default: return "Кулаки";
            }
        }
    }

    private HUD hud;
    private float cooldown;

    private void Start()
    {
        hud = HUD.Instance;
        GameState.OnInventoryChanged += OnInventoryChanged;
    }

    private void OnDestroy()
    {
        if (GameState.OnInventoryChanged != null) GameState.OnInventoryChanged -= OnInventoryChanged;
    }

    private void OnInventoryChanged()
    {
        if (selected == 1 && !GameState.ownKnife) selected = 0;
        if (selected == 2 && !GameState.ownPistol) selected = 0;
        if (hud != null) hud.SetWeapon(CurrentName);
    }

    private void Update()
    {
        HandleSwitch();
        cooldown -= Time.deltaTime;

        var shop = ShopPanel.Instance;
        if (shop != null && shop.IsOpen) return;

        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    private void HandleSwitch()
    {
        bool changed = false;
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1)) { selected = 0; changed = true; }
        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2)) { if (GameState.ownKnife) { selected = 1; changed = true; } }
        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3)) { if (GameState.ownPistol) { selected = 2; changed = true; } }
        if (changed && hud != null) hud.SetWeapon(CurrentName);
    }

    private void Attack()
    {
        if (cooldown > 0f) return;
        Vector2 facing = (Vector2)transform.right;
        var pc = PlayerController.Instance;
        if (pc != null) facing = pc.Facing;

        switch (selected)
        {
            case 1:
                Melee(dmgKnife, rangeKnife, cdKnife, facing);
                break;
            case 2:
                Shoot(dmgPistol, rangePistol, cdPistol, facing);
                break;
            default:
                Melee(dmgFist, rangeFist, cdFist, facing);
                break;
        }
    }

    private void Melee(int dmg, float range, float cd, Vector2 facing)
    {
        cooldown += cd;
        var cols = Physics2D.OverlapCircleAll(transform.position, range * 1.5f);
        Fish target = null;
        float bestScore = float.MaxValue;
        for (int i = 0; i < cols.Length; i++)
        {
            var f = cols[i].GetComponent<Fish>();
            if (f == null || !Targetable(f)) continue;
            Vector2 to = (Vector2)f.transform.position - (Vector2)transform.position;
            float dist = to.magnitude;
            if (dist > range * 1.5f) continue;
            float ang = Vector2.Angle(facing, to.normalized);
            if (ang > 80f) continue;
            float score = dist + ang * 0.1f;
            if (score < bestScore)
            {
                bestScore = score;
                target = f;
            }
        }

        if (target != null)
        {
            target.Damage(dmg);
            FloatingText.Flash(target.transform.position, Palette.White, 0.5f);
        }
    }

    private void Shoot(int dmg, float range, float cd, Vector2 facing)
    {
        cooldown += cd;
        Vector2 origin = (Vector2)transform.position + facing * 0.6f;
        var hits = Physics2D.RaycastAll(origin, facing, range);
        Fish target = null;
        float bestDist = float.MaxValue;
        for (int i = 0; i < hits.Length; i++)
        {
            var f = hits[i].collider.GetComponent<Fish>();
            if (f == null || !Targetable(f)) continue;
            float d = hits[i].distance;
            if (d < bestDist)
            {
                bestDist = d;
                target = f;
            }
        }

        if (target != null)
        {
            Vector3 hitPos = target.transform.position;
            target.Damage(dmg);
            FloatingText.Flash(hitPos, new Color32(0xff, 0xd7, 0x4f, 255), 0.6f);
        }
        FloatingText.Flash(origin, new Color32(0xff, 0xff, 0x80, 200), 0.35f);
    }

    private bool Targetable(Fish f)
    {
        return f.state == FishState.Flopping || f.state == FishState.CaughtFly;
    }
}