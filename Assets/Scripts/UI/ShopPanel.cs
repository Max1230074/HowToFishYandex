using System;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour
{
    public static event Action OnSold;

    public static ShopPanel Instance { get; private set; }

    private RectTransform panel;
    private RectTransform listRoot;
    private Text sellInfo;
    private PlayerController player;

    public bool IsOpen { get { return panel != null && panel.gameObject.activeSelf; } }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        player = PlayerController.Instance;
    }

    public void Build(Transform canvas)
    {
        panel = Ui.NewRect(canvas, "ShopPanel");
        Ui.SetRect(panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-400, -280), new Vector2(400, 280));
        var bg = Ui.Img(panel, "Bg", new Color(0.06f, 0.09f, 0.12f, 0.96f),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Ui.Txt(panel, "Title", "ЛАВКА", 34, Palette.Gold,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-180, -20), new Vector2(360, 44));

        UniText(panel, "Мимо лавки: E — продать /  закрыть", 15, new Color(1, 1, 1, 0.6f),
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-250, -66), new Vector2(500, 22));

        sellInfo = Ui.Txt(panel, "SellInfo", "", 18, Palette.White,
            new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24, -100), new Vector2(-24, -64), TextAnchor.MiddleLeft);

        listRoot = Ui.NewRect(panel, "List");
        Ui.SetRect(listRoot, new Vector2(0f, 0f), new Vector2(1f, 0.45f), new Vector2(24, 20), new Vector2(-24, -20));

        for (int i = 0; i < ShopCatalog.All.Length; i++)
        {
            int idx = i;
            float top = 1f - i * 0.26f;
            float bottom = top - 0.22f;
            var row = Ui.NewRect(listRoot, "Item" + i);
            Ui.SetRect(row, new Vector2(0f, bottom), new Vector2(1f, top), Vector2.zero, Vector2.zero);
            Ui.Img(row, "RowBg", new Color(0, 0, 0, 0.35f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Ui.Txt(row, "Info", "", 18, Palette.White,
                new Vector2(0f, 0f), new Vector2(0.6f, 1f), new Vector2(12, 6), new Vector2(0, -6), TextAnchor.MiddleLeft);

            var buy = Ui.NewRect(row, "Buy");
            Ui.SetRect(buy, new Vector2(0.62f, 0.1f), new Vector2(0.98f, 0.9f), Vector2.zero, Vector2.zero);
            var cross = buy.gameObject.AddComponent<Image>();
            cross.color = new Color32(0x2f, 0x7f, 0x4f, 255);
            Ui.Txt(buy, "Label", "", 16, Palette.White, Vector2.zero, Vector2.one, new Vector2(4, 0), new Vector2(-4, 0));
            var btn = buy.gameObject.AddComponent<Button>();
            btn.targetGraphic = cross;
            int capture = idx;
            btn.onClick.AddListener(() => BuyButtonClicked(capture));
        }

        var closeBtn = Ui.NewRect(panel, "Close");
        Ui.SetRect(closeBtn, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120, 12), new Vector2(120, 44));
        var cb = closeBtn.gameObject.AddComponent<Button>();
        cb.targetGraphic = Ui.Img(closeBtn, "Bg", new Color32(0x8a, 0x3a, 0x2f, 255), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Ui.Txt(closeBtn, "Label", "ЗАКРЫТЬ", 18, Palette.White, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        cb.onClick.AddListener(Close);

        panel.gameObject.SetActive(false);
    }

    private Text UniText(Transform parent, string msg, int size, Color c, Vector2 amn, Vector2 amx, Vector2 omin, Vector2 omax)
    {
        return Ui.Txt(parent, "t", msg, size, c, amn, amx, omin, omax);
    }

    private void Update()
    {
        if (!IsOpen) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (player != null && player.carriedFish != null)
            {
                SellCarried();
            }
            else
            {
                Close();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)) Close();
        RefreshList();
    }

    public void Open()
    {
        GameState.OnMoneyChanged += OnMoneyChanged;
        GameState.OnInventoryChanged += OnInventoryChanged;
        panel.gameObject.SetActive(true);
        RefreshList();
    }

    public void Close()
    {
        GameState.OnMoneyChanged -= OnMoneyChanged;
        GameState.OnInventoryChanged -= OnInventoryChanged;
        panel.gameObject.SetActive(false);
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    private void OnMoneyChanged()
    {
        RefreshList();
    }

    private void OnInventoryChanged()
    {
        RefreshList();
    }

    private void RefreshList()
    {
        if (listRoot == null || listRoot.childCount == 0) return;
        for (int i = 0; i < ShopCatalog.All.Length; i++)
        {
            var item = ShopCatalog.All[i];
            var rt = listRoot.GetChild(i);
            if (rt == null) continue;
            bool owned = GameState.IsOwned(item.id);
            bool maxed = item.id == "rod2" && owned;
            var text = rt.Find("Info").GetComponent<Text>();
            text.text = item.name + " — $" + item.cost + "\n" + item.desc;

            var btn = rt.Find("Buy").GetComponent<Button>();
            var label = rt.Find("Buy/Label").GetComponent<Text>();
            var cross = rt.Find("Buy").GetComponent<Image>();
            if (owned)
            {
                label.text = maxed ? "МАКС" : "ЕСТЬ";
                btn.interactable = false;
                cross.color = new Color32(0x2f, 0x5f, 0x3a, 255);
            }
            else
            {
                bool affordable = GameState.Money >= item.cost;
                label.text = "Купить $" + item.cost;
                btn.interactable = affordable;
                cross.color = affordable ? new Color32(0x2f, 0x7f, 0x4f, 255) : new Color32(0x5a, 0x5a, 0x5a, 255);
            }
        }

        if (player != null && player.carriedFish != null)
        {
            sellInfo.text = "На руках: " + player.carriedFish.displayName + "  (стоимость ~$" + player.carriedFish.computeValue + ")";
        }
        else
        {
            sellInfo.text = "Рыба не подобрана. Подойдите к добыче, чтобы взять её.";
        }
    }

    private void SellCarried()
    {
        if (player == null || player.carriedFish == null) return;
        var f = player.carriedFish;
        player.carriedFish = null;
        int value = f.computeValue;
        f.SellFromHand();
        GameState.AddMoney(value);
        YandexBridge.SetLeaderboard("h2f_coins", GameState.Money);
        FloatingText.Flash(player.transform.position, Palette.Gold, 1f);
        if (OnSold != null) OnSold();
        RefreshList();
    }

    public void SellButtonClicked()
    {
        SellCarried();
    }

    public void BuyButtonClicked(int index)
    {
        if (index < 0 || index >= ShopCatalog.All.Length) return;
        var item = ShopCatalog.All[index];
        if (item.kind == ItemKind.Consumable)
        {
            if (player == null) return;
            if (player.IsFullHp())
            {
                HUD.Instance.Hint("Здоровье уже полное");
                return;
            }
            if (!GameState.Spend(item.cost)) return;
            player.Heal(50);
            FloatingText.Create(player.transform.position + Vector3.up, "+50 HP", new Color32(0x5c, 0xc9, 0x3f, 255));
        }
        else
        {
            if (!GameState.BuyItem(item.id, item.cost))
            {
                HUD.Instance.Hint("Не хватает монет или уже куплено");
                return;
            }
            FloatingText.Create(player.transform.position + Vector3.up, item.name, Palette.Gold);
        }
        RefreshList();
    }
}