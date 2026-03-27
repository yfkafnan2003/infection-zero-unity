using UnityEngine;

[CreateAssetMenu(fileName = "New POI", menuName = "Game/POI Data")]
public class POIData : ScriptableObject
{
    public string poiName;
    public string levelScene;

    public POIType poiType;

    [Header("Difficulty")]
    public int requiredPlayerLevel = 1;
    public int difficultyLevel = 1; // 1-10

    [Header("Mission Settings")]
    public int zombieAmount = 10;
    public int surviveTime = 60;
}