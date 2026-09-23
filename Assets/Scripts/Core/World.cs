using UnityEngine;

public static class World
{
    public const float BorderX = 16f;
    public const float BorderY = 10f;

    public const float WaterMinX = -11.5f;
    public const float WaterMaxX = -1.5f;
    public const float WaterMinY = -6f;
    public const float WaterMaxY = 6f;

    public const float ShoreX = -4.5f;

    public const float PlayerMinX = -8.5f;
    public const float PlayerMaxX = 14f;
    public const float PlayerMinY = -9f;
    public const float PlayerMaxY = 9f;

    public static readonly Vector2 IslandCenter = new Vector2(5f, 0f);
    public const float IslandRadius = 10f;

    public static readonly Vector3 PlayerStart = new Vector3(6f, -2f, 0f);
    public static readonly Vector3 ShopPos = new Vector3(7f, 4f, 0f);
}