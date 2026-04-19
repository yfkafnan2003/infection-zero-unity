using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CreditSceneManager : MonoBehaviour
{
    public float creditDuration = 10f;
    public string mapSceneName = "MapScene";
    
    void Start()
    {
        StartCoroutine(WaitAndLoadMap());
    }
    
    IEnumerator WaitAndLoadMap()
    {
        yield return new WaitForSeconds(creditDuration);
        
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene(mapSceneName);
        }
        else
        {
            SceneManager.LoadScene(mapSceneName);
        }
    }
}