using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        GameState.Load();

        EnsureCamera();
        EnsureEventSystem();

        BuildWorld();
        BuildFishingSpot();
        BuildShop();

        Transform player = BuildPlayer();

        BuildHUD();
        BuildCameraFollow(player);

        gameObject.AddComponent<TutorialBrain>();
    }

    private void Start()
    {
        YandexBridge.Init();
        YandexBridge.GameReady();
        YandexBridge.GameplayStart();
    }

    private void EnsureCamera()
    {
        if (Camera.main != null) return;
        var go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6.3f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = World.WaterDeep;
        cam.transform.position = new Vector3(6f, 0f, -10f);
        go.AddComponent<AudioListener>();
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    private void BuildWorld()
    {
        var sea = MakeRect("Sea", new Vector2(0f, 0f), 34f, 22f, Palette.WaterDeep, -12);
        sea.transform.position = new Vector3(0f, 0f, 5f);

        var tint = MakeRect("FishingZone", new Vector2(0f, 0f), 12.5f, 14f, Palette.WaterShallow, -11);
        tint.transform.position = new Vector3(-6f, 0f, 4f);

        var island = MakeSprite("Island", Palette.Square(Palette.Sand), -10);
        island.transform.position = World.IslandCenter;
        island.transform.localScale = Vector3.one * (World.IslandRadius * 2f);

        var grass = MakeSprite("Grass", Palette.Square(Palette.Grass), -9);
        grass.transform.position = World.IslandCenter + new Vector3(1f, 0.5f, 0f);
        grass.transform.localScale = Vector3.one * (World.IslandRadius * 1.75f);

        MakeWall("WallLeft", new Vector2(-17f, 0f), new Vector2(2f, 24f));
        MakeWall("WallRight", new Vector2(17f, 0f), new Vector2(2f, 24f));
        MakeWall("WallTop", new Vector2(0f, 11f), new Vector2(36f, 2f));
        MakeWall("WallBottom", new Vector2(0f, -11f), new Vector2(36f, 2f));
    }

    private GameObject MakeRect(string name, Vector2 pos, float w, float h, Color c, int order)
    {
        var go = new GameObject(name);
        var sr = Palette.AddSprite(go, Palette.Square(c), order);
        sr.transform.localScale = new Vector3(w, h, 1f);
        go.transform.position = pos;
        return go;
    }

    private GameObject MakeSprite(string name, Sprite s, int order)
    {
        var go = new GameObject(name);
        Palette.AddSprite(go, s, order);
        return go;
    }

    private void MakeWall(string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        var bc = go.AddComponent<BoxCollider2D>();
        bc.size = size;
    }

    private void BuildFishingSpot()
    {
        var go = new GameObject("WaterZone");
        go.transform.position = new Vector3(-6f, 0f, 0f);
        go.AddComponent<FishingSpot>();
    }

    private void BuildShop()
    {
        var go = MakeSprite("Shopkeeper", Palette.Circle(Palette.Gold), 4);
        go.transform.position = World.ShopPos;
        go.transform.localScale = Vector3.one * 1.6f;
        go.AddComponent<Shopkeeper>();

        var sign = MakeSprite("ShopSign", Palette.Square(new Color32(0x4a, 0x35, 0x22, 255)), 5);
        sign.transform.position = World.ShopPos + new Vector3(0f, 1.15f, 0f);
        sign.transform.localScale = new Vector3(1.4f, 0.55f, 1f);

        var label = new GameObject("ShopLabel");
        label.transform.position = World.ShopPos + new Vector3(0f, 1.4f, -0.1f);
        var tm = label.AddComponent<TextMesh>();
        tm.font = Ui.Font;
        tm.text = "ЛАВКА";
        tm.fontSize = 42;
        tm.characterSize = 0.055f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = Palette.White;
        if (tm.font != null) label.GetComponent<MeshRenderer>().material = tm.font.material;
    }

    private Transform BuildPlayer()
    {
        var go = new GameObject("Player");
        go.transform.position = World.PlayerStart;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.linearDamping = 9f;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.55f;

        var visuals = new GameObject("Visuals");
        visuals.transform.SetParent(go.transform, false);

        var body = Palette.AddSprite(visuals, Palette.Circle(Palette.PlayerOrange), 5);
        body.name = "Body";
        body.transform.localScale = Vector3.one * 1.15f;

        var eye = new GameObject("Eye");
        eye.transform.SetParent(visuals.transform, false);
        eye.transform.localPosition = new Vector3(0.25f, 0.3f, -0.1f);
        var eyeSr = Palette.AddSprite(eye, Palette.Circle(new Color32(0x2b, 0x2b, 0x2b, 255)), 6);
        eyeSr.transform.localScale = Vector3.one * 0.22f;

        var carry = new GameObject("CarrySlot");
        carry.transform.SetParent(go.transform, false);
        carry.transform.localPosition = new Vector3(0f, 1.0f, -0.1f);

        var rod = new GameObject("Rod");
        rod.transform.SetParent(go.transform, false);
        rod.transform.localPosition = new Vector3(0f, 0.15f, -0.1f);
        var rodSr = Palette.AddSprite(rod, Palette.Square(new Color32(0x8a, 0x63, 0x33, 255)), 4);
        rodSr.transform.localScale = new Vector3(0.09f, 0.7f, 1f);

        var bobber = new GameObject("Bobber");
        bobber.transform.SetParent(go.transform, false);
        bobber.transform.localPosition = new Vector3(0f, 0.3f, -0.1f);
        var bobSr = Palette.AddSprite(bobber, Palette.Circle(new Color32(0xff, 0x3f, 0x3f, 255)), 8);
        bobSr.transform.localScale = Vector3.one * 0.18f;
        bobber.SetActive(false);

        var line = go.AddComponent<LineRenderer>();
        line.startWidth = 0.06f;
        line.endWidth = 0.06f;
        line.startColor = new Color(1f, 1f, 1f, 0.9f);
        line.endColor = new Color(1f, 1f, 1f, 0.9f);
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.sortingOrder = 7;
        line.enabled = false;

        go.AddComponent<PlayerController>();
        go.AddComponent<PlayerWeapons>();
        go.AddComponent<RodController>();

        return go.transform;
    }

    private void BuildHUD()
    {
        var canvasGo = new GameObject("HUD Canvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = true;

        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        var hud = canvasGo.AddComponent<HUD>();
        hud.Build(canvasGo.transform);

        var shopGo = new GameObject("ShopPanel");
        shopGo.transform.SetParent(canvasGo.transform, false);
        var shop = shopGo.AddComponent<ShopPanel>();
        shop.Build(canvasGo.transform);
    }

    private void BuildCameraFollow(Transform player)
    {
        var cf = gameObject.AddComponent<CameraFollow>();
        cf.target = player;
    }
}

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    private void Update()
    {
        var cam = Camera.main;
        if (cam == null || target == null) return;
        Vector3 p = target.position;
        p.x = Mathf.Clamp(p.x, -3f, 12f);
        p.y = Mathf.Clamp(p.y, -3.5f, 4f);
        p.z = -10f;
        cam.transform.position = Vector3.Lerp(cam.transform.position, p, Time.deltaTime * 7f);
    }
}

public class TutorialBrain : MonoBehaviour
{
    private HUD hud;
    private bool tipCast, tipHook, tipCatch, tipKill, tipSell;
    private float timer;

    private void Start()
    {
        hud = HUD.Instance;
        RodController.OnEvent += OnRod;
        Fish.AnyKilled += OnKilled;
        ShopPanel.OnSold += OnSold;

        timer = 0.4f;
    }

    private void OnDestroy()
    {
        RodController.OnEvent -= OnRod;
        Fish.AnyKilled -= OnKilled;
        ShopPanel.OnSold -= OnSold;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = 1.2f;
        if (hud != null && !tipCast && !tipHook && !tipCatch && !tipKill)
        {
            var rod = FindObjectOfType<RodController>();
            if (rod == null || rod.state == RodState.Idle)
            {
                hud.Hint("WASD — движение. SPACE (зажать) — зарядка заброса, отпусти — бросок. ЛКМ — атака.", 3.2f);
            }
        }
    }

    private void OnRod(string evt)
    {
        if (hud == null) return;
        switch (evt)
        {
            case "cast":
                tipCast = true;
                hud.Hint("Блесна в воде. Ждите клёв...");
                break;
            case "hook":
                tipHook = true;
                hud.Hint("КЛЁВ! Зажмите SPACE — тяните. Когда вспыхнет красное — ОТПУСТИ.");
                break;
            case "catch":
                if (!tipCatch)
                {
                    tipCatch = true;
                    hud.Hint("Рыба на берегу! Наведите курсор и жмите ЛКМ (кулаки/нож).");
                }
                break;
            case "escape":
                hud.Hint("Рыба сорвалась. Кидайте ещё раз.");
                break;
        }
    }

    private void OnKilled(Fish f)
    {
        if (hud == null) return;
        if (!tipKill)
        {
            tipKill = true;
            hud.Hint("Подойдите к рыбе — она в руках. Несите торговцу и жмите E. Бонусы: воздух, один выстрел, быстро — выше цена.");
        }
    }

    private void OnSold()
    {
        if (hud == null || tipSell) return;
        tipSell = true;
        hud.Hint("Отлично! Откройте лавку (E) и купите нож (30$) или пистолет (120$).");
    }
}