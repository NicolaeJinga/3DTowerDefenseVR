using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveManager : MonoBehaviour
{
    [System.Serializable]
    public class NrOfEnemy
    {
        public GameObject typeOfMonster;
        public int numberToSpawn;
        public float spawnInterval;
        [HideInInspector] public int numberSpawned = 0;
        [HideInInspector] public float untilNextSpawn;
        [HideInInspector] public bool currentSetOfEnemiesStarted = false;
        [HideInInspector] public bool isEnemyAlive = true;
    }

    [System.Serializable]
    public class Wave
    {
        public int waveInterval;
        //a wave contains one or more "group of enemies" as miniwaves
        public List<NrOfEnemy> enemies = new List<NrOfEnemy>();
        [HideInInspector] public float untilNextWave;
        [HideInInspector] public bool currentWaveStarted = false;
    }

    public TextMeshProUGUI WaveDisplay;

    public List<Wave> waves = new List<Wave>();

    //variables and delegate needed for win condition
    private int totalNumberOfEnemies = 0;
    private int currentNumberOfEnemiesSpawned = 0;
    private GameObject[] arrayOfAliveEnemies;

    public delegate void Win();
    public static event Win OnWin;

    void GetTotalNrOfEnemies()
    {
        for(int i = 0; i < waves.Count; ++i) //for each wave
        {
            for (int j = 0; j < waves[i].enemies.Count; ++j) //for each group of enemies
            {
                totalNumberOfEnemies += waves[i].enemies[j].numberToSpawn;
            }
        }
        Debug.Log("Nr total de inamici este: " + totalNumberOfEnemies);
    }

    private SoundManager sm;
    private AudioSource waveIncomingSource;
    bool soundedHornThisWave = false;

    bool firstTowerBuilt = false;

    private void OnEnable()
    {
        BuilderController.OnTowerBuilt += StartWaves;
    }

    private void OnDisable()
    {
        BuilderController.OnTowerBuilt -= StartWaves;
    }

    void StartWaves(string notUsed, bool notUsed2, int notUsed3)
    {
        firstTowerBuilt = true;
    }

    void Start()
    {
        waves[0].currentWaveStarted = true;
        GetTotalNrOfEnemies();
        sm = gameObject.GetComponent<SoundManager>();
        waveIncomingSource = gameObject.AddComponent<AudioSource>();
        bool firstTowerBuilt = false;
    }

    void Update()
    {
        if (firstTowerBuilt)
        {
            ParseWaves();
        }
        CheckForWinCondition();
    }

    void CheckForWinCondition()
    {
        if (currentNumberOfEnemiesSpawned == totalNumberOfEnemies)
        {
            arrayOfAliveEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            if (arrayOfAliveEnemies.Length == 0)
            {
                OnWin?.Invoke();
                Debug.Log("YOU WIN");
            }
            else
            {
                Debug.Log("Still left to fight!");
            }
        }
    }

    public int GetWaveNumber()
    {
        for (int i = 0; i < waves.Count; ++i) //for each wave
        {
            if (waves[i].currentWaveStarted == true) //if wave has started
            {
                return i+1; //return wave number, starting from 1.
            }
        }
        return -1; //error
    }

    void ParseWaves()
    {
        for (int i = 0; i < waves.Count; ++i) //for each wave
        {
            if (waves[i].currentWaveStarted == true) //if wave has started
            {
                WaveDisplay.text = "Wave " + (i + 1) + "\n in " + (waves[i].waveInterval - (int)waves[i].untilNextWave) + " sec"; //display in world space the time until next level
                if (waves[i].untilNextWave >= waves[i].waveInterval) //if delay until next wave has passed
                {
                    WaveDisplay.text = "Wave " + (i + 1); //show the current wave number in gui
                    if (!soundedHornThisWave) //if wave incoming sound was not played this wave
                    {
                        waveIncomingSource.PlayOneShot(sm.waveIncoming, sm.volume[sm.waveIncoming]); //play wave incoming sound
                        soundedHornThisWave = true; //set to true in order to not play it every time
                    }
                    waves[i].enemies[0].currentSetOfEnemiesStarted = true; //mark first "group of enemies" as started
                    for (int j = 0; j < waves[i].enemies.Count; ++j) //for each group of enemies
                    {
                        if (waves[i].enemies[j].untilNextSpawn >= waves[i].enemies[j].spawnInterval && //if interval between monster has passed
                            waves[i].enemies[j].numberSpawned < waves[i].enemies[j].numberToSpawn && //and if there are still monsters left to spawn
                            waves[i].enemies[j].currentSetOfEnemiesStarted == true) //and if current "group of enemies" has started
                        {
                            waves[i].enemies[j].numberSpawned++; //increase our monster counter
                            currentNumberOfEnemiesSpawned++; //increase monster counter (needed for win condition)
                            waves[i].enemies[j].untilNextSpawn = 0.0f; //reset spawn interval
                            Instantiate(waves[i].enemies[j].typeOfMonster,
                                        waves[i].enemies[j].typeOfMonster.transform.position,
                                        waves[i].enemies[j].typeOfMonster.transform.rotation); //spawn monster

                            if (waves[i].enemies[j].numberSpawned >= waves[i].enemies[j].numberToSpawn) //if we spawned all monsters in current "group of enemies"
                            {
                                waves[i].enemies[j].currentSetOfEnemiesStarted = false; //mark current "group of enemies" as stopped
                                if (j + 1 < waves[i].enemies.Count) //check if we are within bounds in the current wave
                                {
                                    waves[i].enemies[j + 1].currentSetOfEnemiesStarted = true;  //mark next "group of enemies" as started
                                }
                                else //if we finished the vector of "group of enemies" within wave
                                {
                                    waves[i].currentWaveStarted = false; //mark the current wave as stopped
                                    if (i + 1 < waves.Count) //check if we are within bounds in the waves
                                    {
                                        waves[i + 1].currentWaveStarted = true; //mark the next wave as started
                                        soundedHornThisWave = false; //new wave, so no wave incoming wave sound was played
                                    }
                                }
                            }
                        }
                        waves[i].enemies[j].untilNextSpawn += Time.deltaTime; //increase timer for enemy spawn.
                    }
                }
                waves[i].untilNextWave += Time.deltaTime; //increase timer for wave start
            }
        }
    }
}
