using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Purchasing;
using System.Collections.Generic;

public class IAPManager : MonoBehaviour
{
    public static IAPManager Instance;

    // =========================================================
    // MAIN PANELS
    // =========================================================

    [Header("MAIN PANELS")]
    public GameObject purchasePanel;

    [Header("INFINITE ENERGY PANEL")]
    public GameObject energyPanel;

    [Header("MONEY PANEL")]
    public GameObject moneyPanel;


    // =========================================================
    // PURCHASE CONFIRMATION
    // =========================================================

    [Header("PURCHASE CONFIRMATION")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI priceText;

    public Image productImage;

    public Button yesButton;
    public Button noButton;


    // =========================================================
    // INFINITE ENERGY
    // =========================================================

    [Header("INFINITE ENERGY")]
    public string infiniteEnergyProductID = "infinite_energy";

    public Sprite infiniteEnergyImage;

    public string infiniteEnergyTitle = "INFINITE ENERGY";

    [TextArea(2, 5)]
    public string infiniteEnergyDescription =
        "Never worry about energy again. Play missions without consuming normal stamina.";

    public string infiniteEnergyPrice = "$1.99";


    // =========================================================
    // MONEY PACK 1
    // =========================================================

    [Header("MONEY PACK 1")]
    public string moneyPack1ProductID = "money_pack_3000";

    public Sprite moneyPack1Image;

    public int moneyPack1Amount = 3000;

    public string moneyPack1Price = "$0.99";


    // =========================================================
    // MONEY PACK 2
    // =========================================================

    [Header("MONEY PACK 2")]
    public string moneyPack2ProductID = "money_pack_10000";

    public Sprite moneyPack2Image;

    public int moneyPack2Amount = 10000;

    public string moneyPack2Price = "$2.99";


    // =========================================================
    // MONEY PACK 3
    // =========================================================

    [Header("MONEY PACK 3")]
    public string moneyPack3ProductID = "money_pack_20000";

    public Sprite moneyPack3Image;

    public int moneyPack3Amount = 20000;

    public string moneyPack3Price = "$4.99";


    // =========================================================
    // IAP
    // =========================================================

    private string selectedProductID;

    private StoreController storeController;

    private bool iapInitialized = false;


    // =========================================================
    // AWAKE
    // =========================================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    // START
    // =========================================================

    async void Start()
    {
        CloseAllPanels();

        if (yesButton != null)
            yesButton.onClick.AddListener(ConfirmPurchase);

        if (noButton != null)
            noButton.onClick.AddListener(ClosePurchasePanel);

        await InitializeIAP();
    }


    // =========================================================
    // INITIALIZE IAP
    // =========================================================

    async System.Threading.Tasks.Task InitializeIAP()
    {
        try
        {
            Debug.Log("Initializing Unity IAP...");

            // Get Store Controller
            storeController = UnityIAPServices.StoreController();

            // -------------------------------------------------
            // EVENTS
            // -------------------------------------------------

            storeController.OnProductsFetched += OnProductsFetched;
            storeController.OnProductsFetchFailed += OnProductsFetchFailed;

            storeController.OnPurchasesFetched += OnPurchasesFetched;
            storeController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;

            storeController.OnPurchasePending += OnPurchasePending;


            // -------------------------------------------------
            // CONNECT
            // -------------------------------------------------

            await storeController.Connect();

            Debug.Log("IAP connected successfully.");


            // -------------------------------------------------
            // LOAD UNITY IAP CATALOG
            // -------------------------------------------------

            ProductCatalog catalog = ProductCatalog.LoadDefaultCatalog();

            CatalogProvider catalogProvider =
                CodelessCatalogProvider.PopulateCatalogProvider(catalog);


            // -------------------------------------------------
            // FETCH PRODUCTS
            // -------------------------------------------------

            catalogProvider.FetchProducts(
                products =>
                {
                    storeController.FetchProducts(products);
                }
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError(
                "IAP initialization failed: " + e
            );
        }
    }


    // =========================================================
    // PRODUCTS FETCHED
    // =========================================================

    void OnProductsFetched(List<Product> products)
    {
        Debug.Log(
            "IAP products fetched: " + products.Count
        );

        foreach (Product product in products)
        {
            Debug.Log(
                "Product available: " +
                product.definition.id
            );
        }

        iapInitialized = true;

        // Fetch previous purchases
        storeController.FetchPurchases();
    }


    // =========================================================
    // PRODUCTS FAILED
    // =========================================================

    void OnProductsFetchFailed(ProductFetchFailed failure)
    {
        Debug.LogError(
            "IAP product fetch failed: " + failure
        );
    }


    // =========================================================
    // PURCHASES FETCHED
    // =========================================================

    void OnPurchasesFetched(Orders orders)
    {
        Debug.Log("Previous purchases fetched.");

        // Restore non-consumable purchases
        foreach (ConfirmedOrder order in orders.ConfirmedOrders)
        {
            foreach (var item in order.CartOrdered.Items())
            {
                Product product = item.Product;

                if (product == null)
                    continue;

                Debug.Log(
                    "Existing purchase: " +
                    product.definition.id
                );

                // Infinite Energy is NON-CONSUMABLE
                if (product.definition.id == infiniteEnergyProductID)
                {
                    GiveInfiniteEnergy();
                }
            }
        }
    }
    public void GiveInfiniteEnergy()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("GameManager not found! Cannot give Infinite Energy.");
            return;
        }

        GameManager.instance.infiniteStamina = true;
        GameManager.instance.SaveAllData();

        Debug.Log("INFINITE ENERGY PURCHASE SUCCESSFUL!");

        ClosePurchasePanel();
    }

    // =========================================================
    // PURCHASE FETCH FAILED
    // =========================================================

    void OnPurchasesFetchFailed(
        PurchasesFetchFailureDescription failure)
    {
        Debug.LogError(
            "Failed to fetch previous purchases: " +
            failure
        );
    }


    // =========================================================
    // OPEN / CLOSE PANELS
    // =========================================================

    public void CloseAllPanels()
    {
        if (purchasePanel != null)
            purchasePanel.SetActive(false);

        if (energyPanel != null)
            energyPanel.SetActive(false);

        if (moneyPanel != null)
            moneyPanel.SetActive(false);
    }


    public void OpenEnergyPanel()
    {
        if (energyPanel != null)
            energyPanel.SetActive(true);

        if (moneyPanel != null)
            moneyPanel.SetActive(false);
    }


    public void OpenMoneyPanel()
    {
        if (moneyPanel != null)
            moneyPanel.SetActive(true);

        if (energyPanel != null)
            energyPanel.SetActive(false);
    }


    // =========================================================
    // INFINITE ENERGY UI
    // =========================================================

    public void ShowInfiniteEnergyPurchase()
    {
        selectedProductID = infiniteEnergyProductID;

        if (titleText != null)
            titleText.text = infiniteEnergyTitle;

        if (descriptionText != null)
            descriptionText.text =
                infiniteEnergyDescription;

        if (priceText != null)
            priceText.text =
                infiniteEnergyPrice;

        if (productImage != null)
            productImage.sprite =
                infiniteEnergyImage;

        OpenPurchasePanel();
    }


    // =========================================================
    // MONEY PACK 1
    // =========================================================

    public void ShowMoneyPack1Purchase()
    {
        selectedProductID =
            moneyPack1ProductID;

        if (titleText != null)
            titleText.text =
                moneyPack1Amount.ToString("N0") +
                " MONEY";

        if (descriptionText != null)
            descriptionText.text =
                "Get " +
                moneyPack1Amount.ToString("N0") +
                " in-game money to purchase weapons, upgrades and other items.";

        if (priceText != null)
            priceText.text =
                moneyPack1Price;

        if (productImage != null)
            productImage.sprite =
                moneyPack1Image;

        OpenPurchasePanel();
    }


    // =========================================================
    // MONEY PACK 2
    // =========================================================

    public void ShowMoneyPack2Purchase()
    {
        selectedProductID =
            moneyPack2ProductID;

        if (titleText != null)
            titleText.text =
                moneyPack2Amount.ToString("N0") +
                " MONEY";

        if (descriptionText != null)
            descriptionText.text =
                "Get " +
                moneyPack2Amount.ToString("N0") +
                " in-game money to purchase weapons, upgrades and other items.";

        if (priceText != null)
            priceText.text =
                moneyPack2Price;

        if (productImage != null)
            productImage.sprite =
                moneyPack2Image;

        OpenPurchasePanel();
    }


    // =========================================================
    // MONEY PACK 3
    // =========================================================

    public void ShowMoneyPack3Purchase()
    {
        selectedProductID =
            moneyPack3ProductID;

        if (titleText != null)
            titleText.text =
                moneyPack3Amount.ToString("N0") +
                " MONEY";

        if (descriptionText != null)
            descriptionText.text =
                "Get " +
                moneyPack3Amount.ToString("N0") +
                " in-game money to purchase weapons, upgrades and other items.";

        if (priceText != null)
            priceText.text =
                moneyPack3Price;

        if (productImage != null)
            productImage.sprite =
                moneyPack3Image;

        OpenPurchasePanel();
    }


    // =========================================================
    // OPEN PURCHASE PANEL
    // =========================================================

    void OpenPurchasePanel()
    {
        if (purchasePanel != null)
            purchasePanel.SetActive(true);
    }


    // =========================================================
    // CLOSE PURCHASE PANEL
    // =========================================================

    public void ClosePurchasePanel()
    {
        if (purchasePanel != null)
            purchasePanel.SetActive(false);

        selectedProductID = "";
    }


    // =========================================================
    // CONFIRM PURCHASE
    // =========================================================

    void ConfirmPurchase()
    {
        if (!iapInitialized)
        {
            Debug.LogWarning(
                "IAP is not ready yet."
            );

            return;
        }

        if (string.IsNullOrEmpty(selectedProductID))
        {
            Debug.LogWarning(
                "No product selected."
            );

            return;
        }


        // -------------------------------------------------
        // FIND PRODUCT
        // -------------------------------------------------

        Product selectedProduct = null;

        foreach (Product product in storeController.GetProducts())
        {
            if (product.definition.id ==
                selectedProductID)
            {
                selectedProduct = product;
                break;
            }
        }


        // -------------------------------------------------
        // PRODUCT NOT FOUND
        // -------------------------------------------------

        if (selectedProduct == null)
        {
            Debug.LogError(
                "Product not found: " +
                selectedProductID
            );

            return;
        }


        // -------------------------------------------------
        // START PURCHASE
        // -------------------------------------------------

        Debug.Log(
            "Starting purchase: " +
            selectedProductID
        );

        storeController.PurchaseProduct(
            selectedProduct
        );
    }


    // =========================================================
    // PURCHASE PENDING
    // =========================================================

    void OnPurchasePending(PendingOrder order)
    {
        Debug.Log("Purchase pending...");


        // -------------------------------------------------
        // PROCESS EVERY ITEM IN THE ORDER
        // -------------------------------------------------

        foreach (var item in order.CartOrdered.Items())
        {
            Product product = item.Product;

            if (product == null)
                continue;

            string productID =
                product.definition.id;


            Debug.Log(
                "Purchase received: " +
                productID
            );


            // -------------------------------------------------
            // INFINITE ENERGY
            // -------------------------------------------------

            if (productID ==
                infiniteEnergyProductID)
            {
                GiveInfiniteEnergy();
            }


            // -------------------------------------------------
            // MONEY PACK 1
            // -------------------------------------------------

            else if (productID ==
                     moneyPack1ProductID)
            {
                GiveMoney(
                    moneyPack1Amount
                );
            }


            // -------------------------------------------------
            // MONEY PACK 2
            // -------------------------------------------------

            else if (productID ==
                     moneyPack2ProductID)
            {
                GiveMoney(
                    moneyPack2Amount
                );
            }


            // -------------------------------------------------
            // MONEY PACK 3
            // -------------------------------------------------

            else if (productID ==
                     moneyPack3ProductID)
            {
                GiveMoney(
                    moneyPack3Amount
                );
            }


            // -------------------------------------------------
            // UNKNOWN
            // -------------------------------------------------

            else
            {
                Debug.LogWarning(
                    "Unknown product: " +
                    productID
                );
            }
        }


        // -------------------------------------------------
        // CONFIRM PURCHASE
        // -------------------------------------------------

        storeController.ConfirmPurchase(order);

        ClosePurchasePanel();

        Debug.Log(
            "Purchase successfully fulfilled."
        );
    }

    // =========================================================
    // GIVE MONEY
    // =========================================================

    void GiveMoney(int amount)
    {
        if (GameManager.instance == null)
        {
            Debug.LogError(
                "GameManager not found!"
            );

            return;
        }


        GameManager.instance.playerMoney += amount;


        PlayerPrefs.SetInt(
            "PlayerMoney",
            GameManager.instance.playerMoney
        );

        PlayerPrefs.Save();


        Debug.Log(
            "Added " +
            amount +
            " money. New balance: " +
            GameManager.instance.playerMoney
        );
    }


    // =========================================================
    // CLEANUP
    // =========================================================

    void OnDestroy()
    {
        if (yesButton != null)
            yesButton.onClick.RemoveListener(
                ConfirmPurchase
            );

        if (noButton != null)
            noButton.onClick.RemoveListener(
                ClosePurchasePanel
            );

        if (storeController != null)
        {
            storeController.OnProductsFetched -=
                OnProductsFetched;

            storeController.OnProductsFetchFailed -=
                OnProductsFetchFailed;

            storeController.OnPurchasesFetched -=
                OnPurchasesFetched;

            storeController.OnPurchasesFetchFailed -=
                OnPurchasesFetchFailed;

            storeController.OnPurchasePending -=
                OnPurchasePending;
        }

        if (Instance == this)
            Instance = null;
    }
}