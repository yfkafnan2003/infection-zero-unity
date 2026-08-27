using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OneTimeButton : MonoBehaviour
{
    public string saveKey;

    void Start()
    {
        if (PlayerPrefs.GetInt(saveKey, 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }
        GetComponent<Button>().onClick.AddListener(ButtonClicked);
    }

    void ButtonClicked()
    {
        PlayerPrefs.SetInt(saveKey, 1);
        PlayerPrefs.Save();

        gameObject.SetActive(false);
    }
}