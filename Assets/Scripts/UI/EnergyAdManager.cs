using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using GoogleMobileAds.Api;

public class EnergyAdManager : MonoBehaviour
{
    public static EnergyAdManager Instance;
    
    [Header("UI Panels")]
    public GameObject confirmationPanel;
    public TextMeshProUGUI confirmationText;
    public Button yesButton;
    public Button noButton;
    public Button watchAdButton;
    
    [Header("Ad Settings")]
    public int energyReward = 1;
    
    private RewardedAd rewardedAd;
    private string rewardedAdUnitId = "ca-app-pub-3535982579807808/4825436948";
    
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    void Start()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
        
        if (watchAdButton != null)
            watchAdButton.onClick.AddListener(ShowConfirmationPanel);
        
        if (yesButton != null)
            yesButton.onClick.AddListener(OnYesClicked);
        
        if (noButton != null)
            noButton.onClick.AddListener(OnNoClicked);
        
        // Initialize AdMob
        MobileAds.Initialize(initStatus => {
            Debug.Log("AdMob initialized");
            LoadRewardedAd();
        });
    }
    
    public void LoadRewardedAd()
    {
        AdRequest request = new AdRequest();
        
        RewardedAd.Load(rewardedAdUnitId, request, (ad, error) =>
        {
            if (error != null)
            {
                Debug.LogError($"Failed to load ad: {error}");
                return;
            }
            rewardedAd = ad;
            Debug.Log("Rewarded ad ready");
        });
    }
    
    public void ShowConfirmationPanel()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(true);
        if (confirmationText != null)
            confirmationText.text = $"Watch ad for +{energyReward} Energy?";
    }
    
    public void ShowConfirmationPanelForEnergy()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(true);
        if (confirmationText != null)
            confirmationText.text = $"No energy!\nWatch ad for +{energyReward} Energy?";
    }
    
    public bool IsPanelActive() => confirmationPanel != null && confirmationPanel.activeSelf;
    
    void OnYesClicked()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
        ShowAd();
    }
    
    void OnNoClicked()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }
    
    void ShowAd()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Show((reward) =>
            {
                Debug.Log($"Reward earned: {reward.Amount} {reward.Type}");
                if (GameManager.instance != null)
                    GameManager.instance.AddEnergy(energyReward);
                StartCoroutine(ShowRewardMessage());
                LoadRewardedAd();
            });
        }
        else
        {
            Debug.Log("Ad not ready, loading...");
            LoadRewardedAd();
            StartCoroutine(RetryShowAd());
        }
    }
    
    IEnumerator RetryShowAd()
    {
        yield return new WaitForSeconds(2f);
        ShowAd();
    }
    
    IEnumerator ShowRewardMessage()
    {
        if (confirmationText != null)
        {
            string original = confirmationText.text;
            confirmationText.text = $"+{energyReward} ENERGY!";
            confirmationPanel.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            confirmationPanel.SetActive(false);
            confirmationText.text = original;
        }
    }
    
    public void OnAdFailed()
    {
        Debug.LogWarning("Ad failed to show");
    }
    
    void OnDestroy()
    {
        if (watchAdButton != null)
            watchAdButton.onClick.RemoveListener(ShowConfirmationPanel);
        if (yesButton != null)
            yesButton.onClick.RemoveListener(OnYesClicked);
        if (noButton != null)
            noButton.onClick.RemoveListener(OnNoClicked);
        if (Instance == this) Instance = null;
    }
}