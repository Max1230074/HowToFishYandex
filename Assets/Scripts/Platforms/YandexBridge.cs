using System;

public static class YandexBridge
{
    public const bool IS_SDK_ACTIVE = false;

    public static void Init()
    {
        Log("Init");
    }

    public static void GameReady()
    {
        Log("GameReady");
    }

    public static void GameplayStart()
    {
        Log("GameplayStart");
    }

    public static void GameplayStop()
    {
        Log("GameplayStop");
    }

    public static void ShowInterstitial(Action onClose)
    {
        Log("ShowInterstitial (заглушка)");
        if (onClose != null) onClose();
    }

    public static void ShowRewarded(Action onReward, Action onFail)
    {
        Log("ShowRewarded (заглушка, награда выдаётся)");
        if (onReward != null) onReward();
    }

    public static void SaveCloud(string json)
    {
        Log("SaveCloud len=" + (json == null ? 0 : json.Length));
    }

    public static string LoadCloud()
    {
        Log("LoadCloud (пусто)");
        return "";
    }

    public static void SetLeaderboard(string leaderboard, long score)
    {
        Log("SetLeaderboard " + leaderboard + " = " + score);
    }

    private static void Log(string msg)
    {
        UnityEngine.Debug.Log("[YandexBridge] " + msg);
    }
}