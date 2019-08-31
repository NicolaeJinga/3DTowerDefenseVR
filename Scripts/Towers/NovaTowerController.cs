using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NovaTowerController : MonoBehaviour
{
    public GameObject novaObject;

    public float fireRate = 4.0f;
    private float timeUntilNextFire = 3.5f;

    private float attackRange = 20.0f; // == radius of nova
    private float closestEnemyDistance = 999.0f;

    SoundManager sm;
    AudioSource towerNovaShoot;

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
                fireRate = 2.5f;
            }
        }
    }

    private void Start()
    {
        sm = GameObject.Find("GameManager").GetComponent<SoundManager>();
        towerNovaShoot = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        transform.Find("Crystal").RotateAround(transform.up, Time.deltaTime * 5.0f);
        if (enemies.Length >= 1)
        {
            closestEnemyDistance = 999.0f;
            foreach (var enemy in enemies)
            {
                float distanceToEnemy = (enemy.transform.position - transform.position).magnitude;
                if(distanceToEnemy < closestEnemyDistance)
                {
                    closestEnemyDistance = distanceToEnemy;
                }
            }
            //Debug.Log("Closest enemy DISTANCE: " + closestEnemyDistance);
            if (closestEnemyDistance <= attackRange)
            {
                Attack();
            }
        }
    }
    
    private void Attack()
    {
        if (timeUntilNextFire >= fireRate)
        {
            Fire();
            timeUntilNextFire = 0.0f;
        }
        timeUntilNextFire += Time.deltaTime;
    }

    private void Fire()
    {
        if (novaObject != null)
        {
            Instantiate(novaObject, transform.position, novaObject.transform.rotation);
            towerNovaShoot.PlayOneShot(sm.towerNovaShoot, sm.volume[sm.towerNovaShoot]);
        }
    }
}
