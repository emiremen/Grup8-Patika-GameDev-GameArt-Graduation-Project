using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoSingleton<GameManager>
{
    public GameObject GameUICanvas;
    public delegate void modeChangeDelegate(bool mode);
    //public static event modeChangeDelegate onModeChange;
    public bool _isStart = false;
    private int MeleeFighterV1Price = 10;
    private int WizardV1Price = 15;
    public int coins = 0;
    public int currentLevel = 1; //default 1
    [SerializeField] private TextMeshProUGUI levelText;
    public TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI MeleeFighterV1PriceText;
    [SerializeField] private TextMeshProUGUI WizardV1PriceText;
    //public List<GameObject> _HeroesList = new List<GameObject>();
    public GameObject[] HeroesList;


    public int defaultMeleeFighterPrice = 10;
    public int defaulWizardPrice = 15;

    public int meleeHitReward = 1;
    public int spellHitReward = 1;
    public int nextLevelReward = 15;
    public int raiseOnBuyMeleeFighter = 2;
    public int raiseOnBuyWizard = 2;
    public int raiseOnNextLevelMeleeFighter = 4;
    public int raiseOnNextLevelWizard = 4;
    public int successReward = 25;
    public int failReward = 15;

    private void Awake()
    {
        //PlayerPrefs.DeleteKey("heroes");
        //PlayerPrefs.DeleteKey("coins");
        //PlayerPrefs.DeleteKey("level");


        if (PlayerPrefs.HasKey("coins")) coins = PlayerPrefs.GetInt("coins");
        if (PlayerPrefs.HasKey("MeleeFighterV1Price")) MeleeFighterV1Price = PlayerPrefs.GetInt("MeleeFighterV1Price"); // ilgili player pref setlenmişmi kontrol et
        if (PlayerPrefs.HasKey("WizardV1Price")) WizardV1Price = PlayerPrefs.GetInt("WizardV1Price"); // ilgili player pref setlenmişmi kontrol et
        if (!PlayerPrefs.HasKey("heroes")) PlayerPrefs.SetString("heroes", "MeleeFighter_v1");
        if (PlayerPrefs.HasKey("level")) currentLevel = PlayerPrefs.GetInt("level");
    }
    private void Start()
    {
        coinsText.text = coins.ToString();
        levelText.text = "LEVEL " + currentLevel;
        MeleeFighterV1PriceText.text = MeleeFighterV1Price.ToString();
        WizardV1PriceText.text = WizardV1Price.ToString();
        SceneManager.LoadScene("Level_" + currentLevel, LoadSceneMode.Additive); //GameUI sahnesini silmeden üzerine Level sahnesinide yükledi
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene("GameUI", LoadSceneMode.Single); //Tüm sahneleri önce kapatır, ardından GameUI sahnesiin yeniden yükler, GameUI da zaten playerPrefsten currentLevel i alarak ona göre level sahnesini yüklediği için bu bize restart etkisi oluşturur. 
    }
    public void NextLevel()
    {
        if(currentLevel == 3) //son level i de kazandığımızı ifa eder
        {
            Debug.Log("You have defeated all enemies!");

            PlayerPrefs.DeleteAll(); // cihaz hafızasında tutulan tüm değerleri sıfırlar, amaç oyun bittiğinde oyunu en baştan başlatmak

            SceneManager.LoadScene("GameUI", LoadSceneMode.Single); //Ardından varolan tüm sahneleri silip GameUI sahnesini baştan yükledik, GameUI da level sahnesini playerprefsten güncel level i sorgulayarak yüklediği için her şey tıkır tıkır tıkır tıkır işledi, bkz: bu scriptin Start() ı.

            return;
        }

        //Level atladığında herolara enflasyon zammı uygula
        WizardV1Price += raiseOnNextLevelWizard;
        PlayerPrefs.SetInt("WizardV1Price", WizardV1Price);
        MeleeFighterV1Price += raiseOnNextLevelMeleeFighter;
        PlayerPrefs.SetInt("MeleeFighterV1Price", MeleeFighterV1Price);
        MeleeFighterV1PriceText.text = MeleeFighterV1Price.ToString();
        WizardV1PriceText.text = WizardV1Price.ToString();

        PlayerPrefs.SetInt("level", currentLevel + 1); //önce hafızadaki güncel level i sonrakine ayarladık

        SceneManager.LoadScene("GameUI", LoadSceneMode.Single); //Ardından varolan tüm sahneleri silip GameUI sahnesini baştan yükledik, GameUI da level sahnesini playerprefsten güncel level i sorgulayarak yüklediği için her şey tıkır tıkır tıkır tıkır işledi, bkz: bu scriptin Start() ı.
    }

    public void isPlayModeOn()
    {
        _isStart = true;
        HeroesList = GameObject.FindGameObjectsWithTag("Hero");
        //GameUICanvas.SetActive(false);
        //onModeChange(_isStart);
        //UIManager.Instance.StartCoroutine(C)
        UIManager.Instance.getTotalMaxHealth(HeroesList);
        UIManager.Instance.CurrentState = UIManager.UIStates.Fight;
    }
    public void isPlayModeOff()
    {
        _isStart = false;
        //GameUICanvas.SetActive(true);
        //onModeChange(_isStart);
        UIManager.Instance.CurrentState = UIManager.UIStates.Main;
    }

    public string[] getHeroes()
    {
        return PlayerPrefs.GetString("heroes").Split(',');
    }

    public void buyHero(string heroName)
    {
        int heroCost = 0;
        if (heroName == "Wizard_v1")
        {
            heroCost = WizardV1Price;
        }else if (heroName == "MeleeFighter_v1")
        {
            heroCost = MeleeFighterV1Price;
        }

        if (coins - heroCost >= 0)
        {
            string[] heroes = getHeroes();
            string newHeroes = "";
            for (int i = 0; i < heroes.Length; i++)
            {
                newHeroes += heroes[i] + ",";
            }
            newHeroes += heroName;

            if (LevelManager.Instance.placeIfAvailable(heroName))
            {
                PlayerPrefs.SetString("heroes", newHeroes);

                if (heroName == "Wizard_v1")
                {
                    calculateCoin(-WizardV1Price); //satın alınan karaktere göre coins i azalt

                    //Satın alımdan sonra hero nun fiyatını 2 coin artır
                    WizardV1Price += raiseOnBuyWizard;
                    PlayerPrefs.SetInt("WizardV1Price", WizardV1Price);
                    WizardV1PriceText.text = WizardV1Price.ToString();
                }
                else if (heroName == "MeleeFighter_v1")
                {
                    calculateCoin(-MeleeFighterV1Price); //satın alınan karaktere göre coins i azalt
                    
                    //Satın alımdan sonra hero nun fiyatını 2 coin artır
                    MeleeFighterV1Price += raiseOnBuyMeleeFighter;
                    PlayerPrefs.SetInt("MeleeFighterV1Price", MeleeFighterV1Price);
                    MeleeFighterV1PriceText.text = MeleeFighterV1Price.ToString();
                }
            }
        }
        
    }

    public void updateHeroListAfterMerge(string name, string mergedName)
    {
        string[] heroes = getHeroes();
        List<String> heroesList = heroes.ToList<String>();

        heroesList.Add(mergedName);

        string newHeroSet = "";
        string result = "";

        int counter = 0;
        foreach (string heroName in heroesList)
        {
            if(heroName == name && counter < 2)
            {
                counter += 1;
                continue;   
            }

            newHeroSet += heroName + ",";
        }
        string substr = newHeroSet.Substring(newHeroSet.Length - 1);
        if (substr == ",")
        {
            result = newHeroSet.Remove(newHeroSet.Length - 1);
        }

        Debug.Log("result:" + result);

        PlayerPrefs.SetString("heroes", result);
    }

    public void calculateCoin(int balance) //içine aldığı miktarı coinse ekler, oyun kapanıp açtığında kaybolmaması için playerprefs e setler
    {
        coins += balance;
        PlayerPrefs.SetInt("coins", coins);
        coinsText.text = coins.ToString();
    }
}

