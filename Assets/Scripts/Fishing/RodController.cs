using System;
using UnityEngine;

public enum RodState
{
    Idle,
    Charging,
    Casting,
    Waiting,
    Hooked
}

public class RodController : MonoBehaviour
{
    public static event Action<string> OnEvent;

    public RodState state = RodState.Idle;
    public float charge;
    public float reelProgress;
    public bool inTug;
    public bool tugWarning;

    private PlayerController player;
    private FishingSpot spot;
    private HUD hud;
    private Camera cam;
    private Transform bobber;
    private LineRenderer line;
    private Transform rodVisual;
    private Fish hooked;
    private Vector2 target;
    private Vector2 from;
    private Vector2 hookPoint;
    private float waitTimer;
    private float castT;
    private float tugWarnLeft;
    private float tugEndBy;
    private float nextTugIn;
    private float bobPhase;

    private void Start()
    {
        player = GetComponent<PlayerController>();
        spot = FishingSpot.Instance != null ? FishingSpot.Instance : FindObjectOfType<FishingSpot>();
        hud = HUD.Instance != null ? HUD.Instance : FindObjectOfType<HUD>();
        cam = Camera.main;
        bobber = transform.Find("Bobber");
        line = GetComponent<LineRenderer>();
        rodVisual = transform.Find("Rod");
        if (bobber != null) bobber.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;
        var shop = ShopPanel.Instance;
        if (shop != null && shop.IsOpen)
        {
            if (state == RodState.Charging) EnterIdle();
            return;
        }

        bool hold = Input.GetKey(KeyCode.Space);
        float dt = Time.deltaTime;

        switch (state)
        {
            case RodState.Idle:
                if (hold)
                {
                    state = RodState.Charging;
                    charge = 0f;
                    if (hud != null) hud.SetCharge(0f, true);
                }
                break;

            case RodState.Charging:
                if (hold)
                {
                    charge = Mathf.Min(1f, charge + dt / 0.6f);
                    if (hud != null) hud.SetCharge(charge, true);
                    if (rodVisual != null)
                    {
                        Vector2 aim = AimDir();
                        rodVisual.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(aim.y, aim.x) * Mathf.Rad2Deg);
                    }
                }
                else
                {
                    Cast();
                }
                break;

            case RodState.Casting:
                castT += dt / 0.55f;
                float t = Mathf.Clamp01(castT);
                float e = t * t * (3f - 2f * t);
                Vector2 pos = Vector2.Lerp(from, target, e);
                pos.y += Mathf.Sin(e * Mathf.PI) * 0.8f;
                if (bobber != null) bobber.position = pos;
                if (t >= 1f)
                {
                    state = RodState.Waiting;
                    waitTimer = 1.2f + UnityEngine.Random.value * 1.6f - GameState.rodLevel * 0.15f;
                    Emit("cast");
                }
                break;

            case RodState.Waiting:
                waitTimer -= dt;
                bobPhase += dt;
                if (bobber != null)
                {
                    Vector2 bp = target;
                    bp.y += Mathf.Sin(bobPhase * 3f) * 0.06f;
                    bobber.position = bp;
                }
                if (hold)
                {
                    EnterIdle();
                }
                else if (waitTimer <= 0f)
                {
                    TryBite();
                }
                break;

            case RodState.Hooked:
                ReelUpdate(dt);
                break;
        }

        DrawLine();
    }

    private Vector2 AimDir()
    {
        Vector2 dir = player != null ? player.Facing : Vector2.right;
        if (cam != null)
        {
            Vector3 m = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mw = new Vector2(m.x, m.y);
            Vector2 f = mw - (Vector2)transform.position;
            if (f.sqrMagnitude > 0.01f) dir = f.normalized;
        }
        return dir;
    }

    private void Cast()
    {
        state = RodState.Casting;
        castT = 0f;
        from = transform.position;

        Vector2 dir = AimDir();
        float baseRange = 3f + charge * (5f + GameState.rodLevel * 2f);
        target = from + dir * baseRange;
        target.x = Mathf.Clamp(target.x, World.WaterMinX + 0.6f, World.WaterMaxX - 0.6f);
        target.y = Mathf.Clamp(target.y, World.WaterMinY + 0.6f, World.WaterMaxY - 0.6f);

        if (bobber != null)
        {
            bobber.position = from;
            bobber.gameObject.SetActive(true);
        }
        if (hud != null) hud.SetCharge(0f, false);
        if (rodVisual != null)
        {
            rodVisual.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        }
        if (player != null) player.OnCast = true;
    }

    private void TryBite()
    {
        Fish f = spot != null ? spot.FindNearest(target, 9f) : null;
        if (f == null)
        {
            waitTimer = 2f;
            return;
        }
        hooked = f;
        f.BeginHooked();
        hookPoint = f.transform.position;
        reelProgress = 0f;
        inTug = false;
        tugWarning = false;
        nextTugIn = 0.9f;
        state = RodState.Hooked;
        if (hud != null)
        {
            hud.ShowReel(true);
            hud.SetReel(0f, false, false);
        }
        Emit("hook");
    }

    private void ReelUpdate(float dt)
    {
        if (!inTug)
        {
            nextTugIn -= dt;
            if (nextTugIn <= 0f)
            {
                inTug = true;
                tugWarning = true;
                tugWarnLeft = 0.45f;
                tugEndBy = 0.8f + UnityEngine.Random.value * 0.5f;
            }
        }
        else
        {
            if (tugWarnLeft > 0f)
            {
                tugWarnLeft -= dt;
                if (tugWarnLeft <= 0f)
                {
                    tugWarnLeft = 0f;
                    tugWarning = false;
                }
            }
            else
            {
                tugEndBy -= dt;
                if (tugEndBy <= 0f)
                {
                    inTug = false;
                    tugWarning = false;
                    nextTugIn = 1.2f + UnityEngine.Random.value * 1.4f;
                }
            }
        }

        bool furious = inTug && !tugWarning;
        bool hold = Input.GetKey(KeyCode.Space);

        if (furious)
        {
            reelProgress -= 1.9f * dt;
        }
        else if (hold)
        {
            reelProgress += 0.55f * dt;
        }
        else
        {
            reelProgress -= 0.3f * dt;
        }
        reelProgress = Mathf.Clamp01(reelProgress);

        if (hud != null) hud.SetReel(reelProgress, tugWarning, furious);

        Vector2 pull = Vector2.Lerp(hookPoint, (Vector2)player.transform.position, reelProgress);
        if (bobber != null) bobber.position = pull;
        if (hooked != null) hooked.FollowToward(pull);

        if (reelProgress >= 1f)
        {
            CatchFish();
        }
        else if (reelProgress <= 0.001f)
        {
            FishEscaped();
        }
    }

    private void CatchFish()
    {
        Vector2 land = (Vector2)player.transform.position + new Vector2(1.3f, 0.5f);
        land.x = Mathf.Max(land.x, World.ShoreX + 1f);
        state = RodState.Idle;
        if (hud != null) hud.ShowReel(false);
        if (bobber != null) bobber.gameObject.SetActive(false);
        if (hooked != null)
        {
            hooked.StartCaughtFly(land);
            hooked = null;
        }
        Emit("catch");
    }

    private void FishEscaped()
    {
        state = RodState.Idle;
        if (hud != null) hud.ShowReel(false);
        if (bobber != null) bobber.gameObject.SetActive(false);
        if (hooked != null)
        {
            hooked.EscapeToWater();
            hooked = null;
        }
        if (hud != null) hud.Hint("Рыба сорвалась. Кидайте снова.", 2.5f);
        Emit("escape");
    }

    private void EnterIdle()
    {
        state = RodState.Idle;
        if (bobber != null) bobber.gameObject.SetActive(false);
        if (hud != null) hud.SetCharge(0f, false);
    }

    public void ReleaseRod()
    {
        if (hooked != null)
        {
            hooked.EscapeToWater();
            hooked = null;
        }
        if (hud != null) hud.ShowReel(false);
        EnterIdle();
    }

    private void DrawLine()
    {
        if (line == null) return;
        if (bobber == null || !bobber.gameObject.activeSelf)
        {
            if (line.enabled) line.enabled = false;
            return;
        }
        Vector3 tip = transform.position;
        if (rodVisual != null) tip = rodVisual.position + rodVisual.up * 0.35f;
        line.positionCount = 2;
        line.SetPosition(0, tip);
        line.SetPosition(1, bobber.position);
        if (!line.enabled) line.enabled = true;
    }

    private static void Emit(string evt)
    {
        if (OnEvent != null) OnEvent(evt);
    }
}