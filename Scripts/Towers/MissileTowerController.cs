using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileTowerController : MonoBehaviour
{
    private GameObject objectToAttack;
    public GameObject typeOfBullet;

    public float fireRate = 1.0f;
    private float timeUntilNextFire = 0.0f;

    private Transform bulletSpawnPoint;

    public GameObject rangeObject;
    private float attackRange;

    private float distanceToObject;

    private GameObject instantiatedBullet;

    private SoundManager sm;
    private AudioSource towerMissileShoot;

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
                fireRate = 0.65f;
            }
        }            
    }

    void Start()
    {
        bulletSpawnPoint = transform.Find("Tower_Top/BulletSpawn").gameObject.transform;
        if(bulletSpawnPoint!= null)
        {
            //distanceToObject = (objectToAttack.transform.position - bulletSpawnPoint.position).magnitude;
        }
        if (rangeObject != null)
        {
            attackRange = (rangeObject.transform.position - bulletSpawnPoint.position).magnitude;
        }
        sm = GameObject.Find("GameManager").GetComponent<SoundManager>();
        towerMissileShoot = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length >= 1)
        {
            foreach (var enemy in enemies)
            {
                objectToAttack = enemy;
                distanceToObject = (objectToAttack.transform.position - bulletSpawnPoint.position).magnitude;
                if (enemy.GetComponent<EnemyHealth>().isDead == false && TargetWithinRange())
                {
                    //objectToAttack.transform.position = new Vector3(enemy.transform.position.x, enemy.transform.position.y + enemy.transform.localScale.y / 2.0f, enemy.transform.position.z);
                    transform.Find("Tower_Top").LookAt(new Vector3(objectToAttack.transform.position.x,
                                                                   transform.Find("Tower_Top").position.y,
                                                                   objectToAttack.transform.position.z)
                                                     + new Vector3(0, objectToAttack.transform.localScale.y / 2.0f, 0)); //upper top of the tower looks at the enemy it is firing to.
                    Attack();
                    break;
                }
            }
        }
    }

    private bool TargetWithinRange()
    {
        if (distanceToObject <= attackRange)
            return true;
        return false;
    }

    private void Attack()
    {
        if(timeUntilNextFire >= fireRate)
        {
            Fire();
            timeUntilNextFire = 0.0f;
        }
        timeUntilNextFire += Time.deltaTime;
    }

    private void Fire()
    {
        if(typeOfBullet != null)
        {
            instantiatedBullet = Instantiate(typeOfBullet, bulletSpawnPoint.transform.position, typeOfBullet.transform.rotation);
            instantiatedBullet.GetComponent<HomingMissileController>().whatToFollow = objectToAttack;
            towerMissileShoot.PlayOneShot(sm.towerMissileShoot, sm.volume[sm.towerMissileShoot]);
        }
    }
}
