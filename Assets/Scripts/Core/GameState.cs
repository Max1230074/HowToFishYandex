using System;
using UnityEngine;

public static class GameState
{
    public static event Action OnMoneyChanged;
    public static event Action OnInventoryChanged;

    private static int money;
    public static int Money { get { return money; } }

    public static int rodLevel = 1;
    public static bool ownKnife;
    public static bool ownPistol;

    [Serializable]
    public class SaveData
    {
        public int money;
        public int rodLevel;
        public bool ownKnife;
        public bool ownPistol;
    }

    private const string SaveKey = "htf_save_v1";

    public static void Load()
    {
        try
        {
            string cloud = YandexBridge.LoadCloud();
            string json = string.IsNullOrEmpty(cloud) ? PlayerPrefs.GetString(SaveKey, "") : cloud;
            if (string.IsNullOrEmpty(json)) return;
            var d = JsonUtility.FromJson<SaveData>(json);
            if (d == null) return;
            money = Mathf.Max(0, d.money);
            rodLevel = Mathf.Clamp(d.rodLevel, 1, 2);
            ownKnife = d.ownKnife;
            ownPistol = d.ownPistol;
            FireChanged();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("GameState.Load error: " + e.Message);
        }
    }

    public static void Save()
    {
        try
        {
            var d = new SaveData();
            d.money = money;
            d.rodLevel = rodLevel;
            d.ownKnife = ownKnife;
            d.ownPistol = ownPistol;
            string json = JsonUtility.ToJson(d);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
            YandexBridge.SaveCloud(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("GameState.Save error: " + e.Message);
        }
    }

    public static void AddMoney(int v)
    {
        money += v;
        OnMoneyChanged?.Invoke();
        Save();
    }

    public static bool Spend(int v)
    {
        if (money < v) return false;
        money -= v;
        OnMoneyChanged?.Invoke();
        Save();
        return true;
    }

    public static bool BuyItem(string id, int cost)
    {
        switch (id)
        {
            case "knife":
                if (ownKnife) return false;
                if (!Spend(cost)) return false;
                ownKnife = true;
                break;
            case "pistol":
                if (ownPistol) return false;
                if (!Spend(cost)) return false;
                ownPistol = true;
                break;
            case "rod2":
                if (rodLevel >= 2) return false;
                if (!Spend(cost)) return false;
                rodLevel = 2;
                break;
            default:
                return false;
        }
        OnInventoryChanged?.Invoke();
        return true;
    }

    public static bool IsOwned(string id)
    {
        switch (id)
        {
            case "knife": return ownKnife;
            case "pistol": return ownPistol;
            case "rod2": return rodLevel >= 2;
            default: return false;
        }
    }

    private static void FireChanged()
    {
        if (OnMoneyChanged != null) OnMoneyChanged();
        if (OnInventoryChanged != null) OnInventoryChanged();
    }
}