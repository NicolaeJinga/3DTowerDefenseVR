using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class Enemy
    {
        public GameObject type;
        public float spawnFrequency;
        [HideInInspector]
        public float nextSpawn;
        [HideInInspector]
        public int nrSpawned = 0;
        public int nrToSpawn;
    }

    public List<Enemy> enemies = new List<Enemy>();

    void Start()
    {

    }

    
    void Update()
    {
        for(int i = 0; i < enemies.Count; ++i)
        {
            if (enemies[i].nextSpawn >= enemies[i].spawnFrequency &&
                enemies[i].nrSpawned < enemies[i].nrToSpawn) 
            {
                enemies[i].nrSpawned++;
                enemies[i].nextSpawn = 0.0f;
                Instantiate(enemies[i].type, enemies[i].type.transform.position, enemies[i].type.transform.rotation);
            }
            enemies[i].nextSpawn += Time.deltaTime;
        }
    }
}
