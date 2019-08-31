using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using TMPro;
using UnityEngine.SceneManagement;

public class BuilderController : MonoBehaviour
{
    public GameManager gameManager;
    private SoundManager sm;

    [System.Serializable]
    public class Tower
    {
        public GameObject tower;
        public GameObject ghost;
    }

    public GameObject HandDisplay;

    public List<Tower> towers = new List<Tower>();

    TowerEnum towerIndex = TowerEnum.Missile;

    public enum TowerEnum
    {
        Missile = 0,
        Nova = 1,
        Lightning = 2,
        COUNT
    }

    public string GetTowerNameByIndexForUI(int index)
    {
        switch(index)
        {
            case 0:
                {
                    return "Cannon";
                }
            case 1:
                {
                    return "Nova";
                }
            case 2:
                {
                    return "Tesla";
                }
            default: return "Error";
        }
    }

    public string GetTowerNameByIndexForDictionaries(int index, bool strippedOfClone)
    {
        switch(index)
        {
            case 0:
                {
                    if (strippedOfClone)
                        return "MissileTower";
                    else
                        return "MissileTower(Clone)";
                }
            case 1:
                {
                    if (strippedOfClone)
                        return "NovaTower";
                    else
                        return "NovaTower(Clone)";
                }
            case 2:
                {
                    if (strippedOfClone)
                        return "LightningTower";
                    else
                        return "LightningTower(Clone)";
                }
            default: return "Error";
        }
    }

    public int GetTowerIndexByStringForDictionaries(string towerName)
    {
        switch(towerName)
        {
            case "MissileTower":
            case "MissileTower(Clone)":
                return 0;
            case "NovaTower":
            case "NovaTower(Clone)":
                return 1;
            case "LightningTower":
            case "LightningTower(Clone)":
                return 2;
            default: return -1;
        }
    }

    public string GetTowerNameStrippedOfClone(string towerName)
    {
        string[] stripped = towerName.Split('(');
        return stripped[0];
    }

    public LineRenderer lineRenderer;

    private float rayRange = 10000.0f;

    public SteamVR_Action_Boolean triggerAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TriggerAction");
    public SteamVR_Action_Boolean cycleTowers   = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TowerCycle");
    public SteamVR_Action_Boolean towerLeft     = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TowerLeft");
    public SteamVR_Action_Boolean towerRight    = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TowerRight");
    public SteamVR_Action_Boolean towerUp       = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TowerUp");
    public SteamVR_Action_Boolean towerSell     = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TowerSell");

    private List<GameObject> towerGhosts = new List<GameObject>();
    private List<GameObject> towerHUD = new List<GameObject>();

    Quaternion initialRotation;

    public delegate void TowerBuilt(string tower, bool upgrade, int towerID);
    public static event TowerBuilt OnTowerBuilt;

    public delegate void TowerSell(string tower, bool upgrade);
    public static event TowerSell OnTowerSell;

    public delegate void NoGold();
    public static event NoGold OnNoGold;

    public GameObject green, yellow, red;
    private GameObject instantiatedGreen, instantiatedYellow, instantiatedRed;

    public TextMeshProUGUI UITowerName;
    public TextMeshProUGUI UITowerGoldCost;
    public TextMeshProUGUI UITowerUpgradeCost;

    [HideInInspector] public bool goToMenu = false;

    private AudioSource towerBuiltSource;
    private AudioSource towerSwitchSource;
    private AudioSource towerSoldSource;

    SteamVR_Input_Sources handByInput;

    public GameObject towerUpgrade;
    Vector3 upgradeIconOffset = new Vector3(0, 7.0f, 0);

    private bool sellingTower = false;

    GameObject buildEffect;
    GameObject sellEffect;
    GameObject buildIcon;
    //GameObject towerGoldForScrollDisplay;
    //public Canvas canvasFortowerGold;

    bool[] canUpgrade = new bool[3];

    private void OnEnable()
    {
        GameManager.OnEndMessage += ReadyToMenu;
    }

    private void OnDisable()
    {
        GameManager.OnEndMessage -= ReadyToMenu;
    }

    private void InitSounds()
    {
        sm = gameManager.GetComponent<SoundManager>();
        towerBuiltSource  = gameObject.AddComponent<AudioSource>();
        towerSwitchSource = gameObject.AddComponent<AudioSource>();
        towerSoldSource   = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        if (gameManager==null)
        {
            gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        }
        ResolveInitOptions();
        LineRendererInit();
        //ParticleSystemsInit();
        InitGhostAndHUDTowers();
        towerIndex = TowerEnum.Missile;
        goToMenu = false;
        initialRotation = HandDisplay.transform.rotation;
        InitSounds();
        buildEffect = Resources.Load<GameObject>("UpgradeEffect");
        sellEffect = Resources.Load<GameObject>("SellEffect");
        for (int i = 0; i < towerHUD.Count; ++i)
        {
            canUpgrade[i] = false;
        }
    }

    void ResolveInitOptions()
    {
        if (Options.twoControllers)
        {
            handByInput = SteamVR_Input_Sources.LeftHand;
        }
        else
        {
            handByInput = SteamVR_Input_Sources.RightHand;
        }

        if (Options.twoControllers == false)
        {
            //move builder module from left hand to right hand.
            transform.parent = GameObject.Find("RightHand").transform;
            transform.position = transform.parent.position;
            GameObject.Find("LeftHand").SetActive(false);
        }

        //this is for displaying the semi-transparent gray panel
        if (Options.newUI)
        {
            HandDisplay = gameObject.transform.Find("ScrollDisplay").gameObject;
            GameObject.Find("DisplayTower").SetActive(false);
        }
        else //oldUI
        {
            HandDisplay = gameObject.transform.Find("DisplayTower").gameObject;
            GameObject.Find("ScrollDisplay").SetActive(false);
        }
    }

    void InitGhostAndHUDTowers()
    {
        buildIcon = Resources.Load<GameObject>("BuildIcon");
        //towerGoldForScrollDisplay = Resources.Load<GameObject>("TowerGoldForScrollDisplay");

        for (int i = 0; i < towers.Count; ++i)
        {
            GameObject go = Instantiate(towers[i].ghost, transform.position, transform.rotation);
            towerGhosts.Add(go);
            towerGhosts[i].SetActive(false);

            go = Instantiate(towers[i].tower, transform.position, transform.rotation);
            towerHUD.Add(go);

            //the answer to the universe of not scaling children based on parent
            //keep the original scale, instantiate without parent, set the rotation as the parent, set the original scale, and then set the parent.
            Vector3 originalScale = buildIcon.transform.localScale;
            go = Instantiate(buildIcon, transform.position, transform.rotation);
            go.transform.rotation = towerHUD[i].transform.rotation;
            go.transform.localScale = originalScale;
            go.transform.parent = towerHUD[i].transform;
            go.transform.position += new Vector3(0, 6.0f, 0);
            //above was instantiating and parenting buildicon, below it is instantiating and parenting towergold for scroll display
            //Canvas canvas = Instantiate(canvasFortowerGold, towerHUD[i].transform);
            //originalScale = towerGoldForScrollDisplay.transform.localScale;
            //go = Instantiate(towerGoldForScrollDisplay, transform.position, transform.rotation);
            //go.transform.rotation = towerHUD[i].transform.rotation;
            //go.transform.localScale = originalScale/10.0f;
            //go.transform.parent = canvas.transform;
            //go.transform.position = HandDisplay.transform.position + new Vector3(0, -6.0f, 0);

            if (Options.newUI == true)
            {
                towerHUD[i].transform.localScale = new Vector3(towerHUD[i].transform.localScale.x / 150.0f,
                                                               towerHUD[i].transform.localScale.y / 150.0f,
                                                               towerHUD[i].transform.localScale.z / 150.0f);
            }
            else
            {
                towerHUD[i].transform.localScale = new Vector3(towerHUD[i].transform.localScale.x / 50.0f,
                                                               towerHUD[i].transform.localScale.y / 50.0f,
                                                               towerHUD[i].transform.localScale.z / 50.0f);
                towerHUD[i].SetActive(false);
            }
            Destroy(towerHUD[i].GetComponent<MonoBehaviour>()); //clear scripts from tower instances displayed on HUD
        }
        Destroy(towerHUD[GetTowerIndexByStringForDictionaries("LightningTower")].transform.Find("Particle System").gameObject);
    }

    void UpdateGhostTowers()
    {
        for (int i = 0; i < towers.Count; ++i)
        {
            towerGhosts[i].SetActive(false);
        }
        towerSwitchSource.PlayOneShot(sm.towerSwitch, sm.volume[sm.towerSwitch]); //play sound for switching tower0.1 0.039858 0.01
    }

    bool IsAnyGhostTowerActive()
    {
        for (int i = 0; i < towers.Count; ++i)
        {
            if(towerGhosts[i].activeSelf)
            {
                return true;
            }
        }
        return false;
    }

    void LineRendererInit()
    {
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.02f;
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.forward * rayRange);
        lineRenderer.SetColors(new Color(255, 0, 0, 50), new Color(255, 0, 0, 50));
        lineRenderer.enabled = false;
    }

    void ParticleSystemsInit()
    {
        instantiatedGreen = Instantiate(green, transform);
        instantiatedGreen.SetActive(false);
        instantiatedYellow = Instantiate(yellow, transform);
        instantiatedYellow.SetActive(false);
        instantiatedRed = Instantiate(red, transform);
        instantiatedRed.SetActive(false);
    }

    void Update()
    {
        ShowWhatCanBeBuiltOrUpgraded();
        if (!goToMenu)
        {
            Raycasting();
            Input();
        }
        TowerHUDUpdate();
        UpdateUI();
        ReturnToMenu();
    }

    void UpdateUI()
    {
        if (Options.newUI == true)
        {
            //Debug.Log("UpdateUI: new ui");
        }
        else //newUI == false
        {
            UITowerName.text = GetTowerNameByIndexForUI((int)towerIndex) + " Tower";
            UITowerGoldCost.text = "Build Cost: " + gameManager.GetComponent<GameManager>().TowerGoldCost[towers[(int)towerIndex].tower.name];
            UITowerUpgradeCost.text = "Upgrade Cost: " + gameManager.GetComponent<GameManager>().TowerUpgradeCost[towers[(int)towerIndex].tower.name];
        }
    }

    private void TowerHUDUpdate()
    {
        if(Options.newUI == true)
        {
            for (int i = 0; i < towerHUD.Count; ++i)
            {
                Vector3 anchorPointPos = HandDisplay.transform.GetChild(0).GetChild(i).position;
                towerHUD[((int)towerIndex + i) % towerHUD.Count].transform.position = new Vector3(anchorPointPos.x, anchorPointPos.y - HandDisplay.transform.localScale.y / 2.0f, anchorPointPos.z);
            }              
            towerHUD[(int)towerIndex].transform.RotateAround(towerHUD[(int)towerIndex].transform.up, Time.deltaTime * 5.0f);
        }
        else //newUI == false
        {
            for(int i = 0; i < towerHUD.Count; ++i)
            {
                towerHUD[i].SetActive(false);
            }
            towerHUD[(int)towerIndex].SetActive(true);
            towerHUD[(int)towerIndex].transform.RotateAround(towerHUD[(int)towerIndex].transform.up, Time.deltaTime * 5.0f);
            towerHUD[(int)towerIndex].transform.position = new Vector3(HandDisplay.transform.position.x,
                                                         HandDisplay.transform.position.y - HandDisplay.transform.localScale.y/4.0f,
                                                         HandDisplay.transform.position.z);
        }
    }

    private void Input()
    {
        bool buttonPressed = false;
        if (towerUp.GetStateDown(handByInput) && buttonPressed == false) //if spawn tower button is held down
        {
            lineRenderer.enabled = true; //enable line render
            ToggleArrowsForBuild();
            sellingTower = false;
            buttonPressed = true;
        }
        else if (towerUp.GetStateUp(handByInput)) //else if spawn tower button is released
        {
            lineRenderer.enabled = false; //disable line render
            ToggleArrowsForBuild();
            buttonPressed = false;
        }

        if (towerSell.GetStateDown(handByInput) && buttonPressed == false) //if sell tower button is held down
        {
            lineRenderer.enabled = true; //enable line render
            sellingTower = true;
            buttonPressed = true;
        }
        else if (towerSell.GetStateUp(handByInput)) //else if sell tower button is released
        {
            lineRenderer.enabled = false; //disable line render
            buttonPressed = false;
        }

        if ((towerRight.GetStateDown(handByInput) ||
            cycleTowers.GetStateDown(handByInput)) && buttonPressed == false) //cycle through towers
        {
            towerIndex = (TowerEnum)(((int)towerIndex + 1) % (int)TowerEnum.COUNT);
            UpdateGhostTowers();
            buttonPressed = true;
        }
        else if (towerLeft.GetStateDown(handByInput) && buttonPressed == false)
        {
            if (towerIndex == 0)
            {
                towerIndex = TowerEnum.COUNT - 1;
            }
            else
            {
                towerIndex = (TowerEnum)(((int)towerIndex - 1) % (int)TowerEnum.COUNT);
            }
            UpdateGhostTowers();
            buttonPressed = true;
        }

        if(towerLeft.GetStateUp(handByInput) || cycleTowers.GetStateUp(handByInput) || towerRight.GetStateUp(handByInput))
        {
            buttonPressed = false;
        }
    }

    public void ShowWhatCanBeBuiltOrUpgraded()
    {
        CheckGoldForUpgrade();
        DisplayIconForUpgrade();
        if (Options.newUI == true)
        {
            DisplayIconForBuild();
        }
    }

    public void CheckGoldForUpgrade()
    {
        for(int i = 0; i < 3; ++i)
        {
            if (gameManager.GetComponent<GameManager>().TowerUpgradeCost[towers[i].tower.name] <= gameManager.GetComponent<GameManager>().gold)
            {
                canUpgrade[i] = true;
            }
            else
            {
                canUpgrade[i] = false;
            }
        }
    }

    public void DisplayIconForUpgrade()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("TowerBase")) //for all towerbases
        {
            TowerBuiltOnTowerBase TBOTB = go.GetComponentInChildren<TowerBuiltOnTowerBase>(); //extract TSL script
            //GameObject go2 = go.GetComponentInChildren<LookAtPlayer>().gameObject;
            LookAtPlayer[] LAP = go.GetComponentsInChildren<LookAtPlayer>();
            if (TBOTB.towerBuilt == true && TBOTB.towerUpgraded == false) //if it is built on but not upgraded
            {
                foreach (LookAtPlayer go2 in LAP)
                {
                    if (go2.gameObject.name == "BuildIcon")
                    {
                        if (canUpgrade[GetTowerIndexByStringForDictionaries(TBOTB.transform.GetChild(0).name)]) //if existing tower matches the gold requirement
                        {
                            go2.GetComponentInChildren<MeshRenderer>().enabled = true; //display upgrade icon
                        }
                        else
                        {
                            go2.GetComponentInChildren<MeshRenderer>().enabled = false;
                        }
                    }
                }
            }
            else
            {
                foreach (LookAtPlayer go2 in LAP)
                {
                    if (go2.gameObject.name == "BuildIcon")
                    {
                        go2.GetComponentInChildren<MeshRenderer>().enabled = false;
                    }
                }
            }
        }
    }

    int previousTowerIndex = 0;

    public void DisplayIconForBuild()
    {
        for(int i = 0; i < towerHUD.Count; ++i)
        {
            GameObject buildIconChild = towerHUD[i].transform.Find("BuildIcon(Clone)").gameObject;
            if (gameManager.GetComponent<GameManager>().TowerGoldCost[towers[i].tower.name] <= gameManager.GetComponent<GameManager>().gold)
            {
                buildIconChild.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                buildIconChild.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    private void UpdateParticleSystem(string waveName)
    {
        Vector3 lookAtRot = lineRenderer.GetPosition(1) - lineRenderer.GetPosition(0);
        switch (waveName)
        {
            case "green":
                {
                    instantiatedGreen.SetActive(true);
                    instantiatedYellow.SetActive(false);
                    instantiatedRed.SetActive(false);
                    break;
                }
            case "yellow":
                {
                    instantiatedGreen.SetActive(false);
                    instantiatedYellow.SetActive(true);
                    instantiatedRed.SetActive(false);
                    break;
                }
            case "red":
                {
                    instantiatedGreen.SetActive(false);
                    instantiatedYellow.SetActive(false);
                    instantiatedRed.SetActive(true);
                    break;
                }
            case "none":
                {
                    instantiatedGreen.SetActive(false);
                    instantiatedYellow.SetActive(false);
                    instantiatedRed.SetActive(false);
                    break;
                }
            default: break;
        }
    }

    public void ReadyToMenu()
    {
        goToMenu = true;
    }

    private void ReturnToMenu()
    {
        if (goToMenu)
        {
            Debug.Log("goToMenu = true");
            RaycastHit hit;
            Ray pointingRay = new Ray(transform.position, transform.forward);

            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.forward * rayRange);

            if (Physics.Raycast(pointingRay, out hit) && lineRenderer.enabled)
            {
                Debug.Log("tag = " + hit.collider.transform.tag);
                if (hit.collider.transform.CompareTag("EndMessage"))
                {
                    lineRenderer.startColor = new Color(224, 255, 255, 50);
                    if (triggerAction.GetStateUp(handByInput) || towerUp.GetStateUp(handByInput))
                    {
                        Debug.Log("Menu OPENED");
                        //from Builder -> LeftHand -> SteamVRObjects -> Player
                        Destroy(transform.parent.parent.parent.gameObject);
                        SceneManager.LoadScene("Menu");
                    }
                }
            }
        }
    }

    private void Raycasting()
    {
        RaycastHit hit;
        Ray pointingRay = new Ray(transform.position, transform.forward);

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, transform.forward * rayRange);

        lineRenderer.startColor = new Color(255, 0, 0, 50);

        towerGhosts[(int)towerIndex].SetActive(false); //disable ghost tower

        if (Physics.Raycast(pointingRay, out hit) && lineRenderer.enabled) //if spawn tower button is held pressed
        {
            if (hit.collider.transform.CompareTag("TowerBase")) //if raycast hits hitbox of TowerBase
            {
                //TSL == TowerBase Spawn Location, a Game object inside TowerBase tree structure
                GameObject TSL = hit.collider.transform.Find("TowerSpawnLocation").gameObject;
                if (TSL.GetComponent<TowerBuiltOnTowerBase>().towerBuilt == false  //if tower has not been built yet in this TSL
                                                         && sellingTower == false) // and if not selling tower
                {
                    lineRenderer.startColor = new Color(0, 255, 0, 50); //make line renderer GREEN.
                    //UpdateParticleSystem("green");
                    towerGhosts[(int)towerIndex].transform.position = TSL.transform.position; //set position of ghost to TSL
                    towerGhosts[(int)towerIndex].SetActive(true); //enable ghost tower

                    if (towerUp.GetStateUp(handByInput) && //if spawn tower button is released 
                        (gameManager.GetComponent<GameManager>().TowerGoldCost[towers[(int)towerIndex].tower.name] 
                            <= gameManager.GetComponent<GameManager>().gold)) //and if player has enough gold
                    {
                        GameObject instantiatedTower = Instantiate(towers[(int)towerIndex].tower, TSL.transform.position, towers[(int)towerIndex].tower.transform.rotation); //spawn tower
                        OnTowerBuilt?.Invoke(towers[(int)towerIndex].tower.name, false, instantiatedTower.GetInstanceID()); //send notification of tower built in order to lose gold
                        gameManager.GoldPopup(TSL.transform.position + upgradeIconOffset, "-"
                            + gameManager.GPLoseGoldByTowerBuild(GetTowerNameByIndexForDictionaries((int)towerIndex,true), false)); //gold popup based on each tower's build cost
                        TSL.GetComponent<TowerBuiltOnTowerBase>().towerBuilt = true; //mark this TSL as built
                        instantiatedTower.transform.parent = TSL.transform; //set the parent of the instantiated tower to the towerbase spawn location
                        towerBuiltSource.PlayOneShot(sm.towerBuilt, sm.volume[sm.towerBuilt]); //play sound for building tower
                        Instantiate(buildEffect, TSL.transform.position, buildEffect.transform.rotation); //smoke effect for building tower
                    }
                    else if (gameManager.GetComponent<GameManager>().TowerGoldCost[towers[(int)towerIndex].tower.name] //not enough money for tower build
                            >= gameManager.GetComponent<GameManager>().gold && towerUp.GetStateUp(handByInput))
                    {
                        gameManager.GoldPopup(TSL.transform.position + upgradeIconOffset,
                            (gameManager.GetComponent<GameManager>().TowerGoldCost[towers[(int)towerIndex].tower.name] -
                            gameManager.GetComponent<GameManager>().gold).ToString(), false, true); //display no gold
                        OnNoGold?.Invoke(); //notify subscribers of no gold
                    }
                }
                else //we are raycasting on an already built tower
                {
                    if (sellingTower) //if button for sell tower was pressed
                    {
                        lineRenderer.startColor = new Color(255, 255, 0, 50); //make line renderer YELLOW as tower is already built and will be sold
                        //UpdateParticleSystem("yellow");
                        if (towerSell.GetStateUp(handByInput)) //if sell tower button is released
                        {
                            if (TSL.GetComponent<TowerBuiltOnTowerBase>().towerUpgraded == true) //here we sell upgraded tower
                            {
                                if (TSL.transform.childCount > 1) //error check
                                {
                                    string towerName = TSL.transform.GetChild(0).name; //get tower name in order to award gold based on it
                                    Destroy(TSL.transform.GetChild(1).gameObject); //destroy upgrade icon
                                    Destroy(TSL.transform.GetChild(0).gameObject); //destroy tower
                                    TSL.GetComponent<TowerBuiltOnTowerBase>().towerBuilt = false; //mark this TSL as free
                                    TSL.GetComponent<TowerBuiltOnTowerBase>().towerUpgraded = false; //mark this TSL as free
                                    OnTowerSell?.Invoke(towerName, true); //send notification of tower sold.
                                    gameManager.GoldPopup(TSL.transform.position + upgradeIconOffset, "+"
                                        + gameManager.GPGetGoldByTowerSell(GetTowerNameByIndexForDictionaries((int)towerIndex, false), true)); //gold popup based on each tower's upgrade sell amount
                                    towerSoldSource.PlayOneShot(sm.towerSold, sm.volume[sm.towerSold]); //play sound for selling tower
                                    Instantiate(sellEffect, TSL.transform.position + upgradeIconOffset, sellEffect.transform.rotation); //coin effect for selling tower
                                }
                            }
                            else if (TSL.GetComponent<TowerBuiltOnTowerBase>().towerBuilt == true) //here we sell non-upgraded tower
                            {
                                if (TSL.transform.childCount > 0) //error check
                                {
                                    string towerName = TSL.transform.GetChild(0).name; //get tower name in order to award gold based on it
                                    Destroy(TSL.transform.GetChild(0).gameObject); //destroy tower
                                    TSL.GetComponent<TowerBuiltOnTowerBase>().towerBuilt = false; //mark this TSL as free
                                    OnTowerSell?.Invoke(towerName, false); //send notification of tower sold.
                                    gameManager.GoldPopup(TSL.transform.position + upgradeIconOffset, "+"
                                        + gameManager.GPGetGoldByTowerSell(GetTowerNameByIndexForDictionaries((int)towerIndex, false), false)); //gold popup based on each tower's sell amount
                                    towerSoldSource.PlayOneShot(sm.towerSold, sm.volume[sm.towerSold]); //play sound for selling tower
                                    Instantiate(sellEffect, TSL.transform.position + upgradeIconOffset, sellEffect.transform.rotation); //coin effect for selling tower
                                }
                            }
                            
                        }
                    }
                    else //button for upgrade was pressed and we proceed with upgrade
                    {
                        lineRenderer.startColor = new Color32(255,140,0,255); //make line renderer ORANGE as tower is already built and will be upgraded
                        string selectedTowerName = GetTowerNameStrippedOfClone(TSL.transform.GetChild(0).name);

                        if (towerUp.GetStateUp(handByInput) && //if tower up button is released, we have to upgrade tower since it is already built
                            (gameManager.GetComponent<GameManager>().TowerUpgradeCost[selectedTowerName] 
                                <= gameManager.GetComponent<GameManager>().gold) && //and if player has enough gold
                            TSL.GetComponent<TowerBuiltOnTowerBase>().towerUpgraded == false) //and if tower was not already upgraded
                        {
                            GameObject instantiatedUpgrade = Instantiate(towerUpgrade,
                                TSL.transform.position + upgradeIconOffset, transform.rotation); //spawn upgrade icon
                            OnTowerBuilt?.Invoke(selectedTowerName, true, TSL.transform.GetChild(0).gameObject.GetInstanceID()); //send notification of tower upgrade in order to lose gold
                            gameManager.GoldPopup(TSL.transform.position + upgradeIconOffset, "-"
                                + gameManager.GPLoseGoldByTowerBuild(selectedTowerName, true)); //gold popup based on each tower's upgrade build cost
                            TSL.GetComponent<TowerBuiltOnTowerBase>().towerUpgraded = true;//mark this TSL as upgraded
                            instantiatedUpgrade.transform.parent = TSL.transform; //set the parent of the upgrade icon to the towerbase spawn location
                            towerBuiltSource.PlayOneShot(sm.towerBuilt, sm.volume[sm.towerBuilt]); //play sound for upgrading tower
                            Instantiate(buildEffect, TSL.transform.position, buildEffect.transform.rotation); //smoke effect for upgrading tower
                        }
                        else if(towerUp.GetStateUp(handByInput) && TSL.GetComponent<TowerBuiltOnTowerBase>().towerUpgraded == true) //if tower is fully upgraded
                        {
                            gameManager.GoldPopup(TSL.transform.position + upgradeIconOffset, "ALREADY UPGRADED", false); //display fully upgraded message
                        }
                        else if(gameManager.GetComponent<GameManager>().TowerUpgradeCost[selectedTowerName] //if not enough money for upgrade
                                >= gameManager.GetComponent<GameManager>().gold &&towerUp.GetStateUp(handByInput))
                        {
                            gameManager.GoldPopup(TSL.transform.position + upgradeIconOffset,
                                (gameManager.GetComponent<GameManager>().TowerUpgradeCost[selectedTowerName] -
                                gameManager.GetComponent<GameManager>().gold).ToString(), false, true); //display no gold
                            OnNoGold?.Invoke(); //notify subscribers of no gold
                        }
                    }
                }
            }
            else if (!hit.collider.transform.CompareTag("TowerBase")) //else if raycast does not hit any TowerBase hitboxes
            {
                lineRenderer.startColor = new Color(255, 0, 0, 50); //make line renderer RED
                //UpdateParticleSystem("red");
            }
        }
        else
        {
            //UpdateParticleSystem("none");
        }
    }

    void ToggleArrowsForBuild()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("TowerBase"))
        {
            bool showArrow = go.GetComponentInChildren<LookAtPlayer>().gameObject.GetComponentInChildren<MeshRenderer>().enabled;
            GameObject go2 = go.GetComponentInChildren<LookAtPlayer>().gameObject;

            if (go2.name == "ArrowForBuild")
            {
                go2.GetComponentInChildren<MeshRenderer>().enabled = !showArrow;
            }
            if (go.GetComponentInChildren<TowerBuiltOnTowerBase>().towerBuilt) //if tower is built on a TSL
            {
                if (go2.name == "ArrowForBuild")
                {
                    go2.GetComponentInChildren<MeshRenderer>().enabled = false; //disabled arrowForBuild for that TSL
                }
            }
        }
    }
}
