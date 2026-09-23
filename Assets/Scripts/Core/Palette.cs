using System.Collections.Generic;
using UnityEngine;

public static class Palette
{
    private static readonly Dictionary<int, Sprite> CircleCache = new Dictionary<int, Sprite>();
    private static readonly Dictionary<int, Sprite> SquareCache = new Dictionary<int, Sprite>();

    public static readonly Color WaterDeep = new Color32(0x14, 0x2c, 0x4f, 255);
    public static readonly Color WaterShallow = new Color32(0x1f, 0x4a, 0x7a, 255);
    public static readonly Color Sand = new Color32(0xd9, 0xb6, 0x6c, 255);
    public static readonly Color Grass = new Color32(0x6f, 0x9d, 0x4d, 255);
    public static readonly Color PlayerOrange = new Color32(0xf0, 0x8a, 0x2a, 255);
    public static readonly Color Gold = new Color32(0xff, 0xd7, 0x4f, 255);
    public static readonly Color White = new Color32(0xec, 0xef, 0xf1, 255);

    public static Sprite Circle(Color c)
    {
        int key = ShapeKey(c);
        Sprite s;
        if (CircleCache.TryGetValue(key, out s)) return s;
        s = MakeCircle(c);
        CircleCache[key] = s;
        return s;
    }

    public static Sprite Square(Color c)
    {
        int key = ShapeKey(c);
        Sprite s;
        if (SquareCache.TryGetValue(key, out s)) return s;
        s = MakeSquare(c);
        SquareCache[key] = s;
        return s;
    }

    private static int ShapeKey(Color c)
    {
        return c.r * 1000000 + c.g * 1000 + c.b;
    }

    private static Sprite MakeCircle(Color c)
    {
        const int res = 40;
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        float r = res * 0.5f;
        var px = new Color32[res * res];
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float dx = x + 0.5f - r;
                float dy = y + 0.5f - r;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float a = Mathf.Clamp01(r - d);
                px[y * res + x] = new Color(c.r, c.g, c.b, c.a * a);
            }
        }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), res);
    }

    private static Sprite MakeSquare(Color c)
    {
        var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        var px = new Color32[64];
        for (int i = 0; i < px.Length; i++) px[i] = c;
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 8f);
    }

    public static SpriteRenderer AddSprite(GameObject go, Sprite sprite, int order = 0, LayerMask layer = default)
    {
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        return sr;
    }
}