using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    GameManager gm;

    public int maxHealth = 100;
    [HideInInspector]
    public int currentHealth;
    public Slider healthSlider;
    public Canvas canvas;

    private GameObject playerCamera;

    public int playerArrowDamage  = 4;
    public int towerMissileDamage = 7;
    public int towerNovaDamage    = 25;

    [HideInInspector]
    public bool isDead = false;

    public delegate void AwardGold(string name);
    public static event AwardGold OnAwardGold;

    public SoundManager sm;
    public AudioSource enemyHitSource;
    public AudioSource enemyDieSource;

    public GameObject hitImpact;

    private int healthAmplificationFactor = 15;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Arrow")
        {
            TakeDamage(playerArrowDamage);
            Destroy(other.gameObject);
            enemyHitSource.PlayOneShot(sm.enemyHit, sm.volume[sm.enemyHit]);
            Instantiate(hitImpact, other.transform.position, other.transform.rotation);
        }
        else if (other.tag == "Missile")
        {
            TakeDamage(towerMissileDamage);
            Destroy(other.gameObject);
            enemyHitSource.PlayOneShot(sm.enemyHit, sm.volume[sm.enemyHit]);
        }
        else if (other.tag == "Nova")
        {
            TakeDamage(towerNovaDamage);
            enemyHitSource.PlayOneShot(sm.enemyHit, sm.volume[sm.enemyHit]);
        }

        //enemy take lightning damage from LightningTowerController script.
    }

    void Start()
    {
        switch(Options.difficulty)
        {
            case 0:
                healthAmplificationFactor = 0;
                break;
            case 1:
                healthAmplificationFactor = 8;
                break;
            case 2:
                healthAmplificationFactor = 15;
                break; 
        }
        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera");
        WaveManager wm = GameObject.Find("GameManager").GetComponent<WaveManager>();
        maxHealth = maxHealth + (wm.GetWaveNumber() * healthAmplificationFactor);
        Debug.Log("viata este = " + maxHealth);
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        isDead = false;
        sm = GameObject.Find("GameManager").GetComponent<SoundManager>();
        enemyHitSource = gameObject.AddComponent<AudioSource>();
        enemyDieSource = gameObject.AddComponent<AudioSource>();

        hitImpact = Resources.Load<GameObject>("FireImpact");
    }

    void Update()
    {
        canvas.transform.LookAt(playerCamera.transform);
        if(isDead)
        {
            Invoke("Sink", 2.0f);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthSlider.value = currentHealth;
        if(currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        gm.GetComponent<GameManager>().GoldPopup(transform.position, "+"+ gm.GPGetGoldByEnemyName(gameObject.name)); //show gold popup based on each enemy's gold award
        OnAwardGold?.Invoke(gameObject.name);
        GetComponentInChildren<Animator>().SetBool("hasDied", true);
        GetComponent<FollowPath>().enabled = false;
        canvas.enabled = false;
        PlayDeathSoundBasedOnEnemyType(gameObject.name);
    }

    void PlayDeathSoundBasedOnEnemyType(string enemyType)
    {
        switch(enemyType)
        {
            case "Imp(Clone)":
                {
                    enemyDieSource.PlayOneShot(sm.enemyDieImp);
                    break;
                }
            case "FireDemon(Clone)":
                {
                    enemyDieSource.PlayOneShot(sm.enemyDieFirelord);
                    break;
                }
            case "Guardian(Clone)":
                {
                    enemyDieSource.PlayOneShot(sm.enemyDieGuardian);
                    break;
                }
        }
    }

    void Sink()
    {
        transform.position += -transform.up * 3.0f * Time.deltaTime;
        Destroy(gameObject, 5.0f);
    }
}
