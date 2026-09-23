using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }

    private Text moneyText;
    private Image hpFill;
    private Text hpText;
    private Image chargeFill;
    private RectTransform chargeGroup;
    private RectTransform reelGroup;
    private Image reelFill;
    private Text reelLabel;
    private Text weaponText;
    private Text hintText;
    private Image flash;
    private float hintTimer;
    private Coroutine hintCR;
    private Coroutine flashCR;

    private void Awake()
    {
        Instance = this;
    }

    public void Build(Transform canvas)
    {
        moneyText = Ui.Txt(canvas, "Money", "Монеты: $0", 34, Palette.Gold,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24, -20), new Vector2(420, 30), TextAnchor.MiddleLeft);
        moneyText.fontStyle = FontStyle.Bold;

        hpText = Ui.Txt(canvas, "HpText", "HP 100/100", 18, Palette.White,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(26, -64), new Vector2(240, 26), TextAnchor.MiddleLeft);

        var hpBack = Ui.Img(canvas, "HpBack", new Color(0, 0, 0, 0.55f),
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24, -96), new Vector2(240, 18));
        hpFill = Ui.Img(hpBack.transform, "HpFill", new Color32(0x5c, 0xc9, 0x3f, 255),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Ui.SetupFilled(hpFill);

        chargeGroup = Ui.NewRect(canvas, "ChargeGroup");
        Ui.SetRect(chargeGroup, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120, 34), new Vector2(120, 58));
        var chargeBack = Ui.Img(chargeGroup, "ChargeBack", new Color(0, 0, 0, 0.5f),
            new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));
        var chargeObj = Ui.Img(chargeBack.transform, "ChargeFill", new Color32(0xff, 0xa5, 0x2f, 255),
            new Vector2(0, 0.5f), new Vector2(1, 1), new Vector2(0, 0), new Vector2(0, 0));
        chargeFill = chargeObj;
        Ui.SetupFilled(chargeFill);
        chargeFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        chargeFill.type = Image.Type.Filled;
        chargeFill.fillMethod = Image.FillMethod.Vertical;
        chargeFill.fillOrigin = (int)Image.OriginVertical.Bottom;
        Ui.Txt(chargeGroup, "ChargeLabel", "ЗАБРОС", 16, new Color(1, 1, 1, 0.8f),
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-80, -8), new Vector2(80, 14));
        chargeGroup.gameObject.SetActive(false);

        reelGroup = Ui.NewRect(canvas, "ReelGroup");
        Ui.SetRect(reelGroup, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-170, 70), new Vector2(170, 100));
        var reelBack = Ui.Img(reelGroup, "ReelBack", new Color(0, 0, 0, 0.6f),
            Vector2.zero, new Vector2(1f, 0.45f), new Vector2(0, 0), new Vector2(0, 0));
        reelFill = Ui.Img(reelBack.transform, "ReelFill", new Color32(0x3f, 0x9e, 0xff, 255),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Ui.SetupFilled(reelFill);
        reelLabel = Ui.Txt(reelGroup, "ReelLabel", "", 26, new Color32(0xff, 0x5f, 0x5f, 255),
            new Vector2(0, 0.45f), new Vector2(1, 0.45f), new Vector2(0, 8), new Vector2(0, 44));
        reelLabel.fontStyle = FontStyle.Bold;
        reelGroup.gameObject.SetActive(false);

        weaponText = Ui.Txt(canvas, "Weapon", "Оружие: Кулаки", 20, Palette.White,
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24, -72), new Vector2(360, 40), TextAnchor.MiddleLeft);
        Ui.Txt(canvas, "ControlsHint", "WASD — движение   Space — удочка   ЛКМ — атака   E — магазин", 15, new Color(1, 1, 1, 0.55f),
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24, -34), new Vector2(700, 30), TextAnchor.MiddleLeft);

        hintText = Ui.Txt(canvas, "Hint", "", 22, Palette.White,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-500, -70), new Vector2(500, 70), TextAnchor.MiddleCenter);

        flash = Ui.Img(canvas, "Flash", new Color(1, 0.2f, 0.2f, 0f),
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, false, 100);

        GameState.OnMoneyChanged += OnMoneyChanged;
        GameState.OnInventoryChanged += OnInventoryChanged;
        OnMoneyChanged();
        OnInventoryChanged();
    }

    private void OnDestroy()
    {
        if (GameState.OnMoneyChanged != null) GameState.OnMoneyChanged -= OnMoneyChanged;
        if (GameState.OnInventoryChanged != null) GameState.OnInventoryChanged -= OnInventoryChanged;
    }

    private void OnMoneyChanged()
    {
        if (moneyText != null) moneyText.text = "Монеты: $" + GameState.Money;
    }

    private void OnInventoryChanged()
    {
        var pw = FindObjectOfType<PlayerWeapons>();
        if (pw != null && weaponText != null) weaponText.text = "Оружие: " + pw.CurrentName + "   [1-3]";
    }

    public void SetWeapon(string name)
    {
        if (weaponText != null) weaponText.text = "Оружие: " + name + "   [1-3]";
    }

    public void SetHp(float current, float max)
    {
        if (hpFill == null) return;
        Ui.SetFill(hpFill, current / max);
        if (hpText != null) hpText.text = "HP " + Mathf.CeilToInt(current) + "/" + Mathf.CeilToInt(max);
    }

    public void SetCharge(float value, bool visible)
    {
        if (chargeGroup == null) return;
        chargeGroup.gameObject.SetActive(visible);
        if (visible && chargeFill != null) Ui.SetFill(chargeFill, value);
    }

    public void ShowReel(bool visible)
    {
        if (reelGroup != null) reelGroup.gameObject.SetActive(visible);
    }

    public void SetReel(float value, bool warning, bool furious)
    {
        if (reelGroup == null || !reelGroup.gameObject.activeSelf) return;
        Ui.SetFill(reelFill, value);
        if (reelLabel == null) return;
        if (furious)
        {
            reelLabel.text = "ОТПУСТИ!";
            reelFill.color = new Color32(0xff, 0x4f, 0x4f, 255);
        }
        else if (warning)
        {
            reelLabel.text = "РЫВОК!";
            reelFill.color = new Color32(0xff, 0xc8, 0x4f, 255);
        }
        else
        {
            reelLabel.text = "ТЯНИ!";
            reelFill.color = new Color32(0x3f, 0x9e, 0xff, 255);
        }
    }

    public void Hint(string msg, float seconds = 4f)
    {
        if (hintText == null) return;
        if (hintCR != null) StopCoroutine(hintCR);
        hintText.text = msg;
        hintTimer = seconds;
        hintCR = StartCoroutine(HintTimer());
    }

    private IEnumerator HintTimer()
    {
        while (hintTimer > 0)
        {
            hintTimer -= Time.deltaTime;
            hintText.color = new Color(1, 1, 1, Mathf.Clamp01(hintTimer));
            yield return null;
        }
        hintText.text = "";
    }

    public void Flash()
    {
        if (flash == null) return;
        if (flashCR != null) StopCoroutine(flashCR);
        flashCR = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        var c = flash.color;
        c.a = 0.55f;
        flash.color = c;
        while (flash.color.a > 0.01f)
        {
            var cc = flash.color;
            cc.a = Mathf.MoveTowards(cc.a, 0f, Time.deltaTime * 2.2f);
            flash.color = cc;
            yield return null;
        }
    }

    public void DestroyUI()
    {
        GameState.OnMoneyChanged -= OnMoneyChanged;
        GameState.OnInventoryChanged -= OnInventoryChanged;
        Destroy(gameObject);
    }
}