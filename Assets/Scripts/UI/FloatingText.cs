using System.Collections;
using UnityEngine;

public static class FloatingText
{
    public static void Create(Vector3 pos, string msg, Color c)
    {
        var go = new GameObject("FloatText");
        go.transform.position = pos;
        var tm = go.AddComponent<TextMesh>();
        tm.font = Ui.Font;
        tm.text = msg;
        tm.fontSize = 46;
        tm.color = c;
        tm.characterSize = 0.06f;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        var mr = go.GetComponent<MeshRenderer>();
        if (tm.font != null) mr.material = tm.font.material;
        go.AddComponent<FloatBehavior>();
    }

    public static void Flash(Vector3 pos, Color c, float scale = 0.7f)
    {
        var go = new GameObject("FlashFx");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Palette.Circle(c);
        sr.sortingOrder = 10;
        go.AddComponent<FlashFx>();
    }

    private class FloatBehavior : MonoBehaviour
    {
        private float age;

        private void Update()
        {
            age += Time.deltaTime;
            transform.position += Vector3.up * (0.7f * Time.deltaTime);
            var tm = GetComponent<TextMesh>();
            var a = 1f - age / 1.3f;
            if (tm != null)
            {
                var c = tm.color;
                tm.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(a));
            }
            if (age >= 1.3f) Destroy(gameObject);
        }
    }

    private class FlashFx : MonoBehaviour
    {
        private SpriteRenderer sr;
        private float age;

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = age / 0.28f;
            transform.localScale = Vector3.one * (0.6f + t * 0.9f);
            if (sr != null)
            {
                var c = sr.color;
                sr.color = new Color(c.r, c.g, c.b, Mathf.Clamp01(1f - t));
            }
            if (age >= 0.28f) Destroy(gameObject);
        }
    }
}