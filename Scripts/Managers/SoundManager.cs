using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioClip arrowShoot;            //set in CrossbowController
    public AudioClip enemyDieFirelord;      //set in EnemyHealth
    public AudioClip enemyDieGuardian;      //set in EnemyHealth
    public AudioClip enemyDieImp;           //set in EnemyHealth
    public AudioClip enemyHit;              //set in EnemyHealth
    public AudioClip gateAttacked;          //set in GateHealth
    public AudioClip gateDestroyed;         //set in GateHealth
    public AudioClip towerBuilt;            //set in BuilderController
    public AudioClip towerSwitch;           //set in BuilderController
    public AudioClip towerSold;             //set in BuilderController
    public AudioClip towerMissileShoot;     //set in MissileTowerController
    public AudioClip towerNovaShoot;        //set in NovaTowerController
    public AudioClip towerTeslaShoot;       //set in LightningTowerController
    public AudioClip waveIncoming;          //set in WaveManager

    public Dictionary<AudioClip, float> volume = new Dictionary<AudioClip, float>();

    void Start()
    {
        volume.Add(arrowShoot, 5.0f);
        volume.Add(enemyDieFirelord, 0.5f);
        volume.Add(enemyDieGuardian, 0.5f);
        volume.Add(enemyDieImp, 0.5f);
        volume.Add(enemyHit, 1.0f);
        volume.Add(gateAttacked, 0.5f);
        volume.Add(gateDestroyed, 0.5f);
        volume.Add(towerBuilt, 0.4f);
        volume.Add(towerSwitch, 0.05f);
        volume.Add(towerSold, 0.5f);
        volume.Add(towerMissileShoot, 0.05f);
        volume.Add(towerNovaShoot, 0.5f);
        volume.Add(towerTeslaShoot, 0.3f);
        volume.Add(waveIncoming, 0.05f);
    }

    void Update()
    {
        
    }
}
