using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsPanelConnector : MonoBehaviour
{
    public AudioMixer targetAudioMixer;
    
    void Start()
    {
        SettingsManager settingsManager = SettingsManager.Instance;

        if (settingsManager != null)
        {
            // Assign AudioMixer if missing
            if (settingsManager.audioMixer == null && targetAudioMixer != null)
            {
                settingsManager.audioMixer = targetAudioMixer;
            }
            
            // Find all sliders in this panel
            Slider[] sliders = GetComponentsInChildren<Slider>();
            
            foreach (Slider slider in sliders)
            {
                // Connect based on slider name
                if (slider.name.Contains("Master"))
                    settingsManager.masterVolumeSlider = slider;
                else if (slider.name.Contains("Music"))
                    settingsManager.musicVolumeSlider = slider;
                else if (slider.name.Contains("SFX"))
                    settingsManager.sfxVolumeSlider = slider;
                else if (slider.name.Contains("Normal"))
                    settingsManager.normalSensitivitySlider = slider;
                else if (slider.name.Contains("Aim"))
                    settingsManager.aimSensitivitySlider = slider;
            }
            
            // CRITICAL: Re-setup the listeners after assigning sliders
            settingsManager.SetupSliderListeners();
            
            // Reload settings to update the sliders
            settingsManager.LoadSettings();
        }
    }
}