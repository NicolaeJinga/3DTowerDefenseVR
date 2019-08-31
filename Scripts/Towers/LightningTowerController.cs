using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningTowerController : MonoBehaviour
{
    public GameObject lightning;

    public float attackRange = 30.0f;
    private float closestEnemyDistance = 999.0f;

    Vector3 lightStart;
    Vector3[] lightEnd = new Vector3[3];

    GameObject[] instantiatedLightning = new GameObject[3];

    [HideInInspector]
    public bool lightningActive = false;

    public float timeframeDamage = 0.1f;
    private float timeframeDamageElapsed = 0.0f;
    GameObject enemyToTarget;

    SoundManager sm;
    AudioSource towerTeslaShoot;

    private int damage = 2;

    [HideInInspector]
    public bool isGhost = false;

    private void OnEnable()
    {
        BuilderController.OnTowerBuilt += Upgrade;
    }

    private void OnDisable()
    {
        BuilderController.OnTowerBuilt -= Upgrade;
    }

    private void Upgrade(string notused, bool upgrade, int id)
    {
        if (id == gameObject.GetInstanceID())
        {
            if (upgrade)
            {
                timeframeDamage = 0.07f;
            }
        }
    }

    private IEnumerator GenerateLightning()
    {
        for (int i = 0; i < instantiatedLightning.Length; ++i)
        {
            yield return 0;
            instantiatedLightning[i] = Instantiate(lightning, transform.position, lightning.transform.rotation);
            instantiatedLightning[i].SetActive(false);
        }
    }

    private void Start()
    {
        StartCoroutine("GenerateLightning");
        lightStart = transform.Find("ShootPoint").transform.position;

        sm = GameObject.Find("GameManager").GetComponent<SoundManager>();
        towerTeslaShoot = gameObject.AddComponent<AudioSource>();
        towerTeslaShoot.loop = true;
        towerTeslaShoot.playOnAwake = false;
        towerTeslaShoot.clip = sm.towerTeslaShoot;
        towerTeslaShoot.volume = sm.volume[sm.towerTeslaShoot];
    }

    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length >= 1)
        {
            closestEnemyDistance = 999.0f;
            foreach (var enemy in enemies)
            {
                float distanceToEnemy = (enemy.transform.position - transform.position).magnitude;
                if (distanceToEnemy < closestEnemyDistance && !enemy.GetComponent<EnemyHealth>().isDead)
                {
                    closestEnemyDistance = distanceToEnemy;
                    for(int i = 0; i < lightEnd.Length; ++i)
                    {
                        lightEnd[i] = enemy.transform.position + new Vector3(0.0f, Random.Range(0,enemy.transform.localScale.y), 0.0f);
                    }
                    enemyToTarget = enemy;
                }
            }
            if (closestEnemyDistance <= attackRange)
            {
                LightningBeam(true);
                KeepLightningOnTarget();
                GiveDamageTo(enemyToTarget, damage);
                if (towerTeslaShoot.isPlaying == false)
                {
                    towerTeslaShoot.Play();
                }
            }
            else
            {
                LightningBeam(false);
                towerTeslaShoot.Stop();
            }
        }
    }

    private void OnDestroy()
    {
        LightningBeam(false);
    }

    void GiveDamageTo(GameObject receiver, int damage)
    {
        if(timeframeDamageElapsed >= timeframeDamage)
        {
            receiver.GetComponent<EnemyHealth>().TakeDamage(damage);
            timeframeDamageElapsed = 0.0f;
        }
        timeframeDamageElapsed += Time.deltaTime;
    }

    void KeepLightningOnTarget()
    {
        for (int i = 0; i < instantiatedLightning.Length; ++i)
        {
            if (instantiatedLightning[i] != null)
            {
                instantiatedLightning[i].transform.Find("LightningStart").transform.position = lightStart;
                instantiatedLightning[i].transform.Find("LightningEnd").transform.position = lightEnd[i];
            }
        }
    }

    private void LightningBeam(bool activate)
    {
        if (lightning != null)
        {
            lightningActive = activate;
            for (int i = 0; i < instantiatedLightning.Length; ++i)
            {
                if (instantiatedLightning[i] != null)
                {
                    instantiatedLightning[i].SetActive(activate);
                }
            }
        }
    }
}
