using UnityEngine;

public static class KillScore
{
    public static float Compute(Fish f)
    {
        float m = 1f;
        if (f.wasAirborne) m += 0.75f;
        if (f.oneShot) m += 0.5f;
        if (f.quick) m += 0.5f;
        return Mathf.Min(m, 3f);
    }
}