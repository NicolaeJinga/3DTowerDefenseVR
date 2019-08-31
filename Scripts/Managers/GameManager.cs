using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [HideInInspector] public int health;
    [HideInInspector] public int gold;
    [HideInInspector] public int ammo;
    
    public TextMeshProUGUI UIHealth;
    public TextMeshProUGUI UIGold;
    public TextMeshProUGUI UIAmmo;

    public Dictionary<string, int> TowerGoldCost    = new Dictionary<string, int>();
    public Dictionary<string, int> TowerUpgradeCost = new Dictionary<string, int>();
    public Dictionary<string, int> EnemyGoldGive    = new Dictionary<string, int>();
    public Dictionary<string, int> TowerGoldSell    = new Dictionary<string, int>();
    public Dictionary<string, int> TowerUpgradeSell = new Dictionary<string, int>();

    public GameObject YouWin;
    public GameObject GameOver;

    public GameObject goldPopup;

    public delegate void EndMessage();
    public static event EndMessage OnEndMessage;

    private void OnEnable()
    {
        BuilderController.OnTowerBuilt += LoseGoldByTower;
        BuilderController.OnTowerSell += GiveGoldByTower;
        EnemyHealth.OnAwardGold += GiveGoldByEnemy;
        GateHealth.OnDamageTake += LoseHealth;
        WaveManager.OnWin += YouWon;
    }

    private void OnDisable()
    {
        BuilderController.OnTowerBuilt -= LoseGoldByTower;
        BuilderController.OnTowerSell -= GiveGoldByTower;
        EnemyHealth.OnAwardGold -= GiveGoldByEnemy;
        GateHealth.OnDamageTake -= LoseHealth;
        WaveManager.OnWin -= YouWon;
    }

    //these are not with (Clone) because they are not taken from the scene unlike the rest
    //they are received from the initial Tower List supplied in BuilderController's Inspector
    private void InitTowerGoldCost() 
    {
        TowerGoldCost.Add("MissileTower", 3);
        TowerGoldCost.Add("NovaTower", 7);
        TowerGoldCost.Add("LightningTower", 5);
    }

    private void InitTowerUpgradeCost()
    {
        TowerUpgradeCost.Add("MissileTower", 9);
        TowerUpgradeCost.Add("NovaTower", 11);
        TowerUpgradeCost.Add("LightningTower", 10);
    }

    private void InitEnemyGoldGive()
    {
        EnemyGoldGive.Add("Imp(Clone)", 1);
        EnemyGoldGive.Add("FireDemon(Clone)", 1);
        EnemyGoldGive.Add("Guardian(Clone)", 1);
    }

    private void InitTowerGoldSell()
    {
        TowerGoldSell.Add("MissileTower(Clone)", 1);
        TowerGoldSell.Add("NovaTower(Clone)", 3);
        TowerGoldSell.Add("LightningTower(Clone)", 2);
    }

    private void InitTowerUpgradeSell()
    {
        TowerUpgradeSell.Add("MissileTower(Clone)", 2);
        TowerUpgradeSell.Add("NovaTower(Clone)", 6);
        TowerUpgradeSell.Add("LightningTower(Clone)", 4);
    }

    private void InitDictionaries()
    {
        InitTowerGoldCost();
        InitTowerUpgradeCost();
        InitEnemyGoldGive();
        InitTowerGoldSell();
        InitTowerUpgradeSell();
    }

    private void InitStats()
    {
        health = 30;
        gold = 15;
        ammo = 20;
        InitDictionaries();
    }

    public int GPGetGoldByEnemyName(string enemyName)
    {
        return EnemyGoldGive[enemyName];
    }

    public int GPLoseGoldByTowerBuild(string towerName, bool upgrade)
    {
        if (upgrade)
        {
            return TowerUpgradeCost[towerName];
        }
        else
        {
            return TowerGoldCost[towerName];
        }
    }

    public int GPGetGoldByTowerSell(string towerName, bool upgraded)
    {
        if (upgraded)
        {
            return TowerUpgradeSell[towerName];
        }
        else
        {
            return TowerGoldSell[towerName];
        }
    }

    private Transform GetWorldCanvasTransform()
    {
        return transform.GetChild(0).transform;
    }

    public void GoldPopup(Vector3 pos, string value, bool append = true, bool nogoldtext = false)
    {
        GameObject go = Instantiate(goldPopup, GetWorldCanvasTransform());
        go.transform.position = pos;
        if (append)
            go.GetComponent<TextMeshProUGUI>().text = value + "G";
        else
            go.GetComponent<TextMeshProUGUI>().text = value;
        if(nogoldtext)
        {
            go.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    void Start()
    {
        Debug.Log("Difficulty = " + Options.difficulty);
        InitStats();
        UpdateUI();
    }
    
    void Update()
    {
        UpdateUI();
    }

    public void GiveGoldByEnemy(string enemyName)
    {
        gold += EnemyGoldGive[enemyName];
    }

    public void LoseGoldByTower(string towerName, bool upgrade, int id)
    {
        if(upgrade)
        {
            gold -= TowerUpgradeCost[towerName];
        }
        else
        {
            gold -= TowerGoldCost[towerName];
        }
    }

    public void GiveGoldByTower(string towerName, bool upgrade)
    {
        if (upgrade)
        {
            gold += TowerUpgradeSell[towerName];
        }
        else
        {
            gold += TowerGoldSell[towerName];
        }
    }

    public void LoseHealth(int lostHP)
    {
        health -= lostHP;
        if (health <= 0)
        {
            Debug.Log("GAME OVER! YOU LOSE!");
            GameOver.SetActive(true);
            OnEndMessage?.Invoke();
        }
    }

    public void YouWon()
    {
        if (health > 0)
        {
            YouWin.SetActive(true);
            OnEndMessage?.Invoke();
        }
    }

    void UpdateUI()
    {
        UIHealth.text = "HP: "     + health;
        UIGold.text   = "Gold: "   + gold;
        UIAmmo.text   = "Ammo: "   + ammo;
    }
}
