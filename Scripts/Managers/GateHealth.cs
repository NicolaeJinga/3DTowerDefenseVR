using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GateHealth : MonoBehaviour
{
    public GameManager gameManager;
    [HideInInspector]
    public int maxHealth;
    [HideInInspector]
    public int currentHealth;
    public Slider healthSlider;

    [HideInInspector]
    public bool isDestroyed = false;

    private float gateFallInSeconds = 5.0f;

    public delegate void DamageTake(int damage);
    public static event DamageTake OnDamageTake;

    public delegate void Destroyed(float gateFall);
    public static event Destroyed OnDestroyed;

    private SoundManager sm;
    private AudioSource gateAttackedSource;
    private AudioSource gateDestroyedSource;

    void Start()
    {
        maxHealth = gameManager.GetComponent<GameManager>().health;
        sm = GameObject.Find("GameManager").GetComponent<SoundManager>();
        gateAttackedSource = gameObject.AddComponent<AudioSource>();
        gateDestroyedSource = gameObject.AddComponent<AudioSource>();
        currentHealth = maxHealth;
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        isDestroyed = false;
    }

    void Update()
    {
        if(isDestroyed)
        {
            Invoke("Sink",1.0f);
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth > 0)
        {
            OnDamageTake?.Invoke(damage); //send notification to game manager to update UI on controller
            currentHealth -= damage;
            healthSlider.value = currentHealth;
            gateAttackedSource.PlayOneShot(sm.gateAttacked, sm.volume[sm.gateAttacked]);
        } 
        else if (currentHealth <= 0 && !isDestroyed)
        {
            Collapse();
        }
    }
    
    void Collapse()
    {
        Debug.Log("gate destroyed");
        gateDestroyedSource.PlayOneShot(sm.gateDestroyed, sm.volume[sm.gateDestroyed]);
        isDestroyed = true;
        GameObject go = gameObject.transform.Find("HealthCanvas").gameObject;
        Destroy(go);
    }

    void Sink()
    {
        transform.position += -transform.up * 5.5f * Time.deltaTime;
        Destroy(gameObject, gateFallInSeconds);
        OnDestroyed?.Invoke(gateFallInSeconds);
    }
}
