using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonActivator : MonoBehaviour
{
    public GameObject buttonToActivate;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(ActivateButton);
        buttonToActivate.SetActive(false);
    }

    void ActivateButton()
    {
        if (buttonToActivate != null)
            buttonToActivate.SetActive(true);
    }
}