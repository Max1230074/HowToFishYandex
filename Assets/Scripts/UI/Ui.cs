using UnityEngine;
using UnityEngine.UI;

public static class Ui
{
    private static Font cachedFont;

    public static Font Font
    {
        get
        {
            if (cachedFont != null) return cachedFont;
            foreach (string name in new[] { "LegacyRuntime.ttf", "Arial.ttf" })
            {
                var f = Resources.GetBuiltinResource<Font>(name);
                if (f != null)
                {
                    cachedFont = f;
                    return f;
                }
            }
            return null;
        }
    }

    public static RectTransform NewRect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        return rt;
    }

    public static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
        rt.pivot = new Vector2(0.5f, 0.5f);
    }

    public static Image Img(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, bool raycast = true, int order = 0)
    {
        var rt = NewRect(parent, name);
        SetRect(rt, anchorMin, anchorMax, offsetMin, offsetMax);
        var img = rt.gameObject.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = raycast;
        return img;
    }

    public static Text Txt(Transform parent, string name, string text, int size, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, TextAnchor align = TextAnchor.MiddleCenter)
    {
        var rt = NewRect(parent, name);
        SetRect(rt, anchorMin, anchorMax, offsetMin, offsetMax);
        var t = rt.gameObject.AddComponent<Text>();
        t.text = text;
        t.font = Font;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    public static void SetFill(Image image, float value)
    {
        if (image == null) return;
        image.fillAmount = Mathf.Clamp01(value);
    }

    public static void SetupFilled(Image image)
    {
        image.type = Image.Type.Filled;
        image.fillMethod = Image.FillMethod.Horizontal;
        image.fillOrigin = (int)Image.OriginHorizontal.Left;
    }
}