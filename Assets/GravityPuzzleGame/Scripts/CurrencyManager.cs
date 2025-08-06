using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CurrencyManager
{
    private static string GOLD_KEY = "PlayerGold";
    public static int GetGold()
    {
        return PlayerPrefs.GetInt(GOLD_KEY, 0);
    }

    public static void AddGold(int amount)
    {
        int currentGold = GetGold();
        PlayerPrefs.SetInt(GOLD_KEY, currentGold + amount);
        PlayerPrefs.Save();
    }
    public static void SetGold(int value)
    {
        PlayerPrefs.SetInt("Gold", value);
    }

    public static bool SpendGold(int amount)
    {
        int currentGold = GetGold();
        if (currentGold >= amount)
        {
            PlayerPrefs.SetInt(GOLD_KEY, currentGold - amount);
            PlayerPrefs.Save();
            return true;
        }
        return false;
    }

    public static void ResetGold()
    {
        PlayerPrefs.SetInt(GOLD_KEY, 0);
        PlayerPrefs.Save();
    }
}
