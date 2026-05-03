using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;
    
    [Header("Audio")]
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    
    [Header("Sensitivity")]
    public Slider normalSensitivitySlider;
    public Slider aimSensitivitySlider;
    public float normalSensitivity = 0.2362331f;
    public float aimSensitivity = 0.2362331f;
    
    [Header("UI Sounds")]
    public AudioSource uiAudioSource;
    public AudioClip buttonClickSound;
    private static AudioMixer _globalAudioMixer; // Static reference to keep AudioMixer
    private float currentNormalSensitivity;
    private float currentAimSensitivity;
    
    [Header("UI Customization")]
    public Button customizeUIButton;
    public UICustomizationManager uiCustomizationManager;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Load AudioMixer from Resources if not assigned
            if (audioMixer == null)
            {
                audioMixer = Resources.Load<AudioMixer>("MasterMixer");
                if (audioMixer == null)
                {
                    Debug.LogError("MasterMixer not found in Resources folder! Create folder 'Resources' and put your AudioMixer there.");
                }
                else
                {
                    Debug.Log("AudioMixer loaded from Resources successfully!");
                    _globalAudioMixer = audioMixer;
                }
            }
            else
            {
                // Store AudioMixer globally if not already stored
                if (_globalAudioMixer == null)
                {
                    _globalAudioMixer = audioMixer;
                }
            }
            
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        ApplySettings();
        SetupSliders();
        SetupUICustomization();
    }
    void SetupUICustomization()
    {
        if (customizeUIButton != null)
        {
            customizeUIButton.onClick.AddListener(OnCustomizeUIClicked);
        }
    }

    void OnCustomizeUIClicked()
    {
        PlayButtonSound();
        
        // Find or create UICustomizationManager
        if (uiCustomizationManager == null)
        {
            uiCustomizationManager = FindObjectOfType<UICustomizationManager>();
            if (uiCustomizationManager == null)
            {
                GameObject uiManager = new GameObject("UICustomizationManager");
                uiCustomizationManager = uiManager.AddComponent<UICustomizationManager>();
                DontDestroyOnLoad(uiManager);
            }
        }
        
        // Find UI elements in current scene
        uiCustomizationManager.FindUIElementsInScene();
        uiCustomizationManager.StartCustomization();
    }
    void SetupSliders()
    {
        // Setup sensitivity sliders - INCREASE THE RANGE
        if (normalSensitivitySlider != null)
        {
            normalSensitivitySlider.minValue = 0.5f;
            normalSensitivitySlider.maxValue = 5f;
            normalSensitivitySlider.value = currentNormalSensitivity;
            normalSensitivitySlider.onValueChanged.AddListener(SetNormalSensitivity);
        }
        
        if (aimSensitivitySlider != null)
        {
            aimSensitivitySlider.minValue = 0.3f;
            aimSensitivitySlider.maxValue = 3f;
            aimSensitivitySlider.value = currentAimSensitivity;
            aimSensitivitySlider.onValueChanged.AddListener(SetAimSensitivity);
        }
        
        // Setup audio sliders
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
    }
    
    public void LoadSettings()
    {
        // Load sensitivity
        currentNormalSensitivity = PlayerPrefs.GetFloat("NormalSensitivity", 1.5f);
        currentAimSensitivity = PlayerPrefs.GetFloat("AimSensitivity", 1f);
        
        // Load audio (0-1 range) with safe defaults
        float masterVol = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.75f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        
        // Clamp values to safe range
        masterVol = Mathf.Clamp(masterVol, 0.01f, 1f);
        musicVol = Mathf.Clamp(musicVol, 0.01f, 1f);
        sfxVol = Mathf.Clamp(sfxVol, 0.01f, 1f);
        
        // Apply to audio mixer
        SetMasterVolume(masterVol);
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
        
        // Update UI sliders if they exist
        if (masterVolumeSlider != null) masterVolumeSlider.value = masterVol;
        if (musicVolumeSlider != null) musicVolumeSlider.value = musicVol;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = sfxVol;
        if (normalSensitivitySlider != null) normalSensitivitySlider.value = currentNormalSensitivity;
        if (aimSensitivitySlider != null) aimSensitivitySlider.value = currentAimSensitivity;
    }
    
    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("NormalSensitivity", currentNormalSensitivity);
        PlayerPrefs.SetFloat("AimSensitivity", currentAimSensitivity);
        PlayerPrefs.Save();
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(DelayedSceneSetup());
    }

    IEnumerator DelayedSceneSetup()
    {
        yield return null; // Wait one frame
        
        // Load AudioMixer from Resources if missing
        if (audioMixer == null)
        {
            audioMixer = Resources.Load<AudioMixer>("MasterMixer");
            if (audioMixer != null)
            {
                _globalAudioMixer = audioMixer;
                Debug.Log("AudioMixer restored from Resources in new scene");
            }
        }
        
        // Restore AudioMixer from global reference
        if (audioMixer == null && _globalAudioMixer != null)
        {
            audioMixer = _globalAudioMixer;
        }
        
        // Find and reconnect sliders in the new scene
        FindSlidersInScene();
        
        // Re-apply settings
        LoadSettings();
        ApplySettings();
    }
    void FindSlidersInScene()
    {
        // Option 1: Find by specific GameObject path (if you have a consistent UI structure)
        GameObject settingsPanel = GameObject.Find("SettingsPanel");
        
        if (settingsPanel == null)
        {
            // Option 2: Find by component type in active canvas
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                settingsPanel = canvas.gameObject;
            }
        }
        
        if (settingsPanel != null)
        {
            // Find all sliders in the panel
            Slider[] allSliders = settingsPanel.GetComponentsInChildren<Slider>(true);
            
            foreach (Slider slider in allSliders)
            {
                string sliderName = slider.name.ToLower();
                
                if (sliderName.Contains("master") || sliderName.Contains("volume"))
                {
                    if (sliderName.Contains("master") || sliderName == "MasterVolumeSlider")
                        masterVolumeSlider = slider;
                }
                else if (sliderName.Contains("music"))
                    musicVolumeSlider = slider;
                else if (sliderName.Contains("sfx") || sliderName.Contains("sound"))
                    sfxVolumeSlider = slider;
                else if (sliderName.Contains("normal") || sliderName.Contains("look"))
                    normalSensitivitySlider = slider;
                else if (sliderName.Contains("aim"))
                    aimSensitivitySlider = slider;
            }
            
            // Setup listeners for found sliders
            SetupSliderListeners();
            
            // Force UI update
            UpdateSliderUI();
        }
        else
        {
            Debug.LogWarning("Settings panel not found in scene - sliders won't auto-connect");
        }
    }

    public void SetupSliderListeners()
    {
        // Master Volume
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveAllListeners();
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }
        
        // Music Volume
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveAllListeners();
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
        
        // SFX Volume
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        }
        
        // Normal Sensitivity - REMOVE the min/max lines here
        if (normalSensitivitySlider != null)
        {
            normalSensitivitySlider.onValueChanged.RemoveAllListeners();
            normalSensitivitySlider.onValueChanged.AddListener(SetNormalSensitivity);
        }
        
        // Aim Sensitivity - REMOVE the min/max lines here
        if (aimSensitivitySlider != null)
        {
            aimSensitivitySlider.onValueChanged.RemoveAllListeners();
            aimSensitivitySlider.onValueChanged.AddListener(SetAimSensitivity);
        }
    }

    void UpdateSliderUI()
    {
        // Update slider visuals without triggering events
        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MasterVolume", 0.75f));
        
        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MusicVolume", 0.75f));
        
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("SFXVolume", 0.75f));
        
        if (normalSensitivitySlider != null)
            normalSensitivitySlider.SetValueWithoutNotify(currentNormalSensitivity);
        
        if (aimSensitivitySlider != null)
            aimSensitivitySlider.SetValueWithoutNotify(currentAimSensitivity);
    }
    public void ApplySettings()
    {
        // Apply sensitivity to all PlayerLook components
        PlayerLook[] playerLooks = FindObjectsOfType<PlayerLook>(true);
        Debug.Log($"Found {playerLooks.Length} PlayerLook components to update");
        
        foreach (PlayerLook look in playerLooks)
        {
            look.SetSensitivity(currentNormalSensitivity);
            Debug.Log($"Applied sensitivity {currentNormalSensitivity} to {look.name}");
        }
        
        // Apply aim sensitivity to all AimSystem components
        AimSystem[] aimSystems = FindObjectsOfType<AimSystem>(true);
        foreach (AimSystem aim in aimSystems)
        {
            aim.aimSpeed = currentAimSensitivity * 200f;
        }
    }

    public void SetNormalSensitivity(float value)
    {
        currentNormalSensitivity = value;
        PlayerPrefs.SetFloat("NormalSensitivity", value);
        PlayerPrefs.Save();
        
        Debug.Log($"=== SENSITIVITY CHANGED TO: {value} ===");
        
        // Update all active PlayerLook scripts
        PlayerLook[] playerLooks = FindObjectsOfType<PlayerLook>(true);
        Debug.Log($"Found {playerLooks.Length} PlayerLook components");
        
        foreach (PlayerLook look in playerLooks)
        {
            if (look != null)
            {
                look.SetSensitivity(value);
                Debug.Log($"Updated sensitivity on {look.gameObject.name} to {value}");
            }
        }
    }
    
    public void SetAimSensitivity(float value)
    {
        currentAimSensitivity = value;
        PlayerPrefs.SetFloat("AimSensitivity", value);
        
        // Update all active AimSystem scripts
        AimSystem[] aimSystems = FindObjectsOfType<AimSystem>(true);
        foreach (AimSystem aim in aimSystems)
        {
            aim.aimSpeed = value * 200f;
        }
    }
    
    public void SetMasterVolume(float value)
    {
        PlayerPrefs.SetFloat("MasterVolume", value);
        SetAudioMixerVolume("MasterVolume", value);
    }
    
    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        SetAudioMixerVolume("MusicVolume", value);
    }
    
    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        SetAudioMixerVolume("SFXVolume", value);
    }
    
    void SetAudioMixerVolume(string parameter, float value)
    {
        if (audioMixer != null)
        {
            float volume = Mathf.Log10(Mathf.Clamp(value, 0.01f, 1f)) * 20f;
            bool result = audioMixer.SetFloat(parameter, volume);
            Debug.Log($"Set {parameter} to {volume} dB (value: {value}) - Success: {result}");
        }
        else
        {
            Debug.LogError($"AudioMixer is NULL! Cannot set {parameter}");
        }
    }
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void PlayButtonSound()
    {
        if (uiAudioSource != null && buttonClickSound != null)
        {
            uiAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    public float GetNormalSensitivity() => currentNormalSensitivity;
    public float GetAimSensitivity() => currentAimSensitivity;
}