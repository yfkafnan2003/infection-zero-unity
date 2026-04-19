using UnityEngine;

[System.Serializable]
public class UtilityData
{
    public string utilityName;
    public UtilityType utilityType;
    public int price;
    public int requiredLevel = 1; // Add level requirement
    public int maxCount;
    public Sprite utilityIcon;
    public GameObject utilityPrefab;
    public bool isUnlocked;
    public int currentCount;
}

public enum UtilityType
{
    FirstAid,
    Grenade
}