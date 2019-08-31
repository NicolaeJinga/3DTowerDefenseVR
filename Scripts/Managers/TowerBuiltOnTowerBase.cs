using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerBuiltOnTowerBase : MonoBehaviour
{
    [HideInInspector]
    public bool towerBuilt = false;
    [HideInInspector]
    public bool towerUpgraded = false;

    void Start()
    {
        towerBuilt = false;
        towerUpgraded = false;
    }
}
