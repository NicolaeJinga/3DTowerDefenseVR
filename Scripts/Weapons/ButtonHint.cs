using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonHint : MonoBehaviour
{
    public class SmartHint
    {
        public Transform hint = null;

        public void Enable()
        {
            hint.gameObject.SetActive(true);
        }
        public void Disable()
        {
            hint.gameObject.SetActive(false);
        }
        public bool IsEnabled()
        {
            return hint.gameObject.activeSelf;
        }
    }

    public List<SmartHint> smartHints = new List<SmartHint>();
    int towersBuilt = 0;

    enum HintType
    {
        Shoot = 0,
        Teleport = 1,
        Build_Upgrade = 2,
        SwitchTower = 3,
        SwitchTower2 = 4,
        Sell = 5,
        COUNT
    }

    private void OnEnable()
    {
        Nokobot.Assets.Crossbow.CrossbowController.OnArrowShots += DisableShootHint;
        Valve.VR.InteractionSystem.Teleport.onTP += DisableTeleportHint;
        BuilderController.OnTowerBuilt += DisableBuildHint;
        BuilderController.OnNoGold += EnableSellHint;
    }

    private void OnDisable()
    {
        Nokobot.Assets.Crossbow.CrossbowController.OnArrowShots -= DisableShootHint;
        Valve.VR.InteractionSystem.Teleport.onTP -= DisableTeleportHint;
        BuilderController.OnTowerBuilt -= DisableBuildHint;
        BuilderController.OnNoGold -= EnableSellHint;
    }

    void EnableSellHint()
    {
        smartHints[(int)HintType.Sell].Enable();
    }

    void DisableShootHint()
    {
        smartHints[(int)HintType.Shoot].Disable();
    }

    void DisableBuildHint(string not_used1, bool not_used2, int not_used3)
    {
        smartHints[(int)HintType.Build_Upgrade].Disable();
        smartHints[(int)HintType.SwitchTower].Enable();
        smartHints[(int)HintType.SwitchTower2].Enable();
        //smartHints[(int)HintType.Shoot].Enable();
        smartHints[(int)HintType.Sell].Disable();
        towersBuilt++;
        if(towersBuilt>=2)
        {
            smartHints[(int)HintType.SwitchTower].Disable();
            smartHints[(int)HintType.SwitchTower2].Disable();
        }
    }

    void DisableTeleportHint()
    {
        smartHints[(int)HintType.Teleport].Disable();
    }

    private void Start()
    {
        if(Options.twoControllers)
        {
            gameObject.SetActive(false);
        }
        for (int i = 0; i < transform.childCount; ++i)
        {
            SmartHint a = new SmartHint();
            a.hint = transform.GetChild(i);
            smartHints.Add(a);
            smartHints[i].Enable();
        }
        smartHints[(int)HintType.SwitchTower].Disable();
        smartHints[(int)HintType.SwitchTower2].Disable();
        smartHints[(int)HintType.Sell].Disable();
    }

    void Update()
    {
        foreach(SmartHint i in smartHints)
        {
            if(i.IsEnabled() == false)
            {
                continue;
            }
            else
            {
                Transform child = i.hint;
                LineRenderer lr = child.Find("Line").gameObject.GetComponent<LineRenderer>();
                lr.SetPosition(0, child.Find("Start").gameObject.transform.position);
                lr.SetPosition(1, child.Find("End").gameObject.transform.position);
            }
        }
    }
}
