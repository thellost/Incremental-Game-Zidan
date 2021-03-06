
using System.Collections.Generic;

using UnityEngine;

using UnityEngine.UI;

using TMPro;
using System;

public class GameManager : MonoBehaviour

{


    // Fungsi [Range (min, max)] ialah menjaga value agar tetap berada di antara min dan max-nya 

    [Range(0f, 1f)]

    public float AutoCollectPercentage = 0.1f;

    public ResourceConfig[] ResourcesConfigs;

    public Sprite[] ResourcesSprites;



    public Transform ResourcesParent;

    public ResourceController ResourcePrefab;

    public TapText TapTextPrefab;



    public Transform CoinIcon;

    public TextMeshProUGUI GoldInfo;

    public TextMeshProUGUI GemInfo;

    public TextMeshProUGUI AutoCollectInfo;

    public Button Buymax;

    public Button Buy1;



    private List<TapText> _tapTextPool = new List<TapText>();
    private List<ResourceController> _activeResources = new List<ResourceController>();
    private float _collectSecond;
    private double totalGoldOvertime;

    [HideInInspector] public bool BuyIsMax;



    private static GameManager _instance = null;

    public static GameManager Instance

    {

        get

        {

            if (_instance == null)

            {

                _instance = FindObjectOfType<GameManager>();

            }



            return _instance;

        }

    }






    private void Start()

    {

        BuyIsMax = false;
        setup();
    }

    private void setup()
    {
        Buy1.onClick.AddListener(SetBuy1);
        Buymax.onClick.AddListener(SetBuyMax);
        AddAllResources();
        GoldInfo.text = $"Gold: { UserDataManager.Progress.Gold.ToString("0") }";
        GemInfo.text = $"Gold: { UserDataManager.Progress.Gems.ToString("0") }";
    }

 



    private void Update()

    {

        // Fungsi untuk selalu mengeksekusi CollectPerSecond setiap detik 

        _collectSecond += Time.unscaledDeltaTime;

        if (_collectSecond >= 1f)

        {

            CollectPerSecond();

            _collectSecond = 0f;

        }

        CoinIcon.transform.localScale = Vector3.LerpUnclamped(CoinIcon.transform.localScale, Vector3.one * 2f, 0.15f);

        CoinIcon.transform.Rotate(0f, 0f, Time.deltaTime * -100f);

        CheckResourceCost();

        UserDataManager.Save();

    }



    private void AddAllResources()

    {
        bool showResources = true;
        int index = 0;

        foreach (ResourceConfig config in ResourcesConfigs)

        {

            GameObject obj = Instantiate(ResourcePrefab.gameObject, ResourcesParent, false);

            ResourceController resource = obj.GetComponent<ResourceController>();

            resource.SetConfig(index, config);
            index++;


            _activeResources.Add(resource);


            obj.gameObject.SetActive(showResources);



            if (showResources && !resource.IsUnlocked)

            {

                showResources = false;

            }


        }

    }

    public void ShowNextResource()

    {

        foreach (ResourceController resource in _activeResources)

        {

            if (!resource.gameObject.activeSelf)

            {

                resource.gameObject.SetActive(true);

                break;

            }

        }

    }

    private void SetBuy1()
    {
        ColorBlock colors = Buy1.colors;
        colors.normalColor = new Color32(255, 197, 0, 255);
        colors.highlightedColor = new Color32(255, 197, 0, 255);
        colors.pressedColor = new Color32(231, 140, 0, 255);
        Buy1.colors = colors;

        colors = Buymax.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color32(231, 140, 0, 255);
        colors.pressedColor = Color.white;
        Buymax.colors = colors;

        BuyIsMax = false;
    }

    private void SetBuyMax()
    {
        ColorBlock colors = Buymax.colors;
        colors.normalColor = new Color32(255, 197, 0, 255);
        colors.highlightedColor = new Color32(255, 197, 0, 255);
        colors.pressedColor = new Color32(231, 140, 0, 255);
        Buymax.colors = colors;

        colors = Buy1.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color32(231, 140, 0, 255);
        colors.pressedColor = Color.white;
        Buy1.colors = colors;

        BuyIsMax = true;
    }
    private void CollectPerSecond()

    {

        double output = 0;

        foreach (ResourceController resource in _activeResources)

        {
            if (resource.IsUnlocked)
            {
                output += resource.GetOutput();
            }
        }

        output = output + (output * UserDataManager.Progress.Gems * 0.05);

        output *= AutoCollectPercentage;

        // Fungsi ToString("F1") ialah membulatkan angka menjadi desimal yang memiliki 1 angka di belakang koma 

        AutoCollectInfo.text = $"Auto Collect: { output.ToString("F1") } / second";



        AddGold(output);

    }

    public void Reincarnate()
    {
        while (totalGoldOvertime >= GetGemCost())
        {
            double upgradeCost = GetGemCost();

            totalGoldOvertime = totalGoldOvertime - upgradeCost;

            UserDataManager.Progress.Gems++;
        }

        GemInfo.text = $"Gems: { UserDataManager.Progress.Gems.ToString("0") }";
        UserDataManager.Progress.Gold = 0;
       
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Resource");
        foreach (GameObject enemy in enemies)
            GameObject.Destroy(enemy);

        _activeResources.Clear();
        resetResourceLevel();
        AddAllResources();


    }


    //bakal reset level resource dengan cara menghapus resource level dan membuat ulang resource di ui
    private void resetResourceLevel()
    {
        UserDataManager.Progress.ResourcesLevels.Clear();
    }

    private double GetGemCost()
    {
        return 100000 * (UserDataManager.Progress.Gems + 1);
    }

    public void AddGold(double value)

    {


        UserDataManager.Progress.Gold = UserDataManager.Progress.Gold + (value + value * (UserDataManager.Progress.Gems * 0.05));
        totalGoldOvertime += value;

        GoldInfo.text = $"Gold: { UserDataManager.Progress.Gold.ToString("0") }";

    }

    public void CollectByTap(Vector3 tapPosition, Transform parent)

    {

        double output = 0;

        foreach (ResourceController resource in _activeResources)

        {
            if (resource.IsUnlocked)
            {
                output += resource.GetOutput();
            }

        }



        TapText tapText = GetOrCreateTapText();

        tapText.transform.SetParent(parent, false);

        tapText.transform.position = tapPosition;



        tapText.Text.text = $"+{ output.ToString("0") }";

        tapText.gameObject.SetActive(true);

        CoinIcon.transform.localScale = Vector3.one * 1.75f;



        AddGold(output);

    }

    private TapText GetOrCreateTapText()

    {

        //mencari apakah ada tap text yang masih aktif (kesimpulan sendiri)
        TapText tapText = _tapTextPool.Find(t => !t.gameObject.activeSelf);

        if (tapText == null)

        {

            tapText = Instantiate(TapTextPrefab).GetComponent<TapText>();

            _tapTextPool.Add(tapText);

        }



        return tapText;

    }

    private void CheckResourceCost()

    {

        foreach (ResourceController resource in _activeResources)

        {

            bool isBuyable = false;

            if (resource.IsUnlocked)

            {

                isBuyable = UserDataManager.Progress.Gold >= resource.GetUpgradeCost();

            }

            else

            {

                isBuyable = UserDataManager.Progress.Gold >= resource.GetUnlockCost();

            }
            resource.ResourceImage.sprite = ResourcesSprites[isBuyable ? 1 : 0];

        }

    }



}

// Fungsi System.Serializable adalah agar object bisa di-serialize dan

// value dapat di-set dari inspector

[System.Serializable]

public struct ResourceConfig

{

    public string Name;

    public double UnlockCost;

    public double UpgradeCost;

    public double Output;

}
