using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;



public class ResourceController : MonoBehaviour

{
    public Button ResourceButton;

    public Image ResourceImage;

    public TextMeshProUGUI ResourceDescription;

    public TextMeshProUGUI ResourceUpgradeCost;

    public TextMeshProUGUI ResourceUnlockCost;

    public bool IsUnlocked { get; private set; }


    private ResourceConfig _config;



    private int _level = 1;

    private void Start()

    {

        ResourceButton.onClick.AddListener(() =>

        {

            if (IsUnlocked)

            {
                UpgradeLevel();

            }

            else

            {

                UnlockResource();

            }

        });

    }

    public void UpgradeLevel()

    {
        do {
            double upgradeCost = GetUpgradeCost();

            if (GameManager.Instance.TotalGold <= upgradeCost)
            {

                break;

            }



            GameManager.Instance.AddGold(-upgradeCost);

            _level++;

        } while (GameManager.Instance.BuyIsMax);



        ResourceUpgradeCost.text = $"Upgrade Cost\n{ GetUpgradeCost() }";

        ResourceDescription.text = $"{ _config.Name } Lv. { _level }\n+{ GetOutput().ToString("0") }";

    }

    public void SetConfig(ResourceConfig config)

    {

        _config = config;


        // ToString("0") berfungsi untuk membuang angka di belakang koma

        ResourceDescription.text = $"{ _config.Name } Lv. { _level }\n+{ GetOutput().ToString("0") }";

        ResourceUnlockCost.text = $"Unlock Cost\n{ _config.UnlockCost }";

        ResourceUpgradeCost.text = $"Upgrade Cost\n{ GetUpgradeCost() }";

        SetUnlocked(_config.UnlockCost == 0);

    }

    public void UnlockResource()

    {

        double unlockCost = GetUnlockCost();

        if (GameManager.Instance.TotalGold <= unlockCost)

        {

            return;

        }



        SetUnlocked(true);

        GameManager.Instance.ShowNextResource();

    }



    public void SetUnlocked(bool unlocked)

    {

        IsUnlocked = unlocked;

        ResourceImage.color = IsUnlocked ? Color.white : Color.grey;

        ResourceUnlockCost.gameObject.SetActive(!unlocked);

        ResourceUpgradeCost.gameObject.SetActive(unlocked);

    }



    public double GetOutput()

    {

        return _config.Output * _level;

    }



    public double GetUpgradeCost()

    {

        return _config.UpgradeCost * _level;

    }



    public double GetUnlockCost()

    {

        return _config.UnlockCost;

    }

}
