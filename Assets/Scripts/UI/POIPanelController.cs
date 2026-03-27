using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class POIPanelController : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI difficultyText;

    public Slider difficultySlider;

    POIData poi;

    public void SetupPOI(POIData data)
    {
        poi = data;

        nameText.text = data.poiName;

        typeText.text = GetTypeText(data.poiType);

        difficultySlider.maxValue = 10;
        difficultySlider.value = data.difficultyLevel;

        difficultyText.text = GetDifficultyText(data.difficultyLevel);
    }

    string GetTypeText(POIType type)
    {
        switch(type)
        {
            case POIType.CountdownSurvive:
                return "Countdown Survive";

            case POIType.KillZombies:
                return "Kill Zombies";

            case POIType.RetrieveBox:
                return "Retrieve Supply Box";

            case POIType.ReachDestination:
                return "Reach Destination";

            case POIType.BossFight:
                return "Boss Fight";
        }

        return "Unknown";
    }

    string GetDifficultyText(int level)
    {
        if(level <= 1) return "Easy";
        if(level <= 2) return "Normal";
        if(level <= 3) return "Hard";
        if(level <= 4) return "Extreme";

        return "Nightmare";
    }
}