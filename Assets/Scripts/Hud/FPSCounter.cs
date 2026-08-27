using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class FPSCounter : MonoBehaviour
{
[SerializeField] private TextMeshProUGUI _fpsText;
[SerializeField] private TextMeshProUGUI _bestfpsText;
[SerializeField] private TextMeshProUGUI _lowestfpsText;
private float updateInterval = 1.0f;
private int _bestFps;
private int _lowestFps;

private float _currentFPS;

private void Start()
{
    _fpsText.text = "FPS: 0";
    _lowestFps = 100;
}

private void Update()
{
    _currentFPS = 1f / Time.deltaTime;
    UpdateFPS();
}

private void UpdateFPS()
{
    if (_currentFPS >= _bestFps)
    {
        _bestFps = (int)_currentFPS;
        _bestfpsText.text = $"Best FPS: {_bestFps}";
    }

    if (_lowestFps >= _currentFPS)
    {
        _lowestFps = (int)_currentFPS;
        _lowestfpsText.text = $"Low FPS: {_lowestFps}";
    }

    _fpsText.text = "Curr FPS: " + Mathf.RoundToInt(_currentFPS);
}
}   