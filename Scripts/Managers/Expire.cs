using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expire : MonoBehaviour
{
    private float timeUntilDestroy = 1.2f;
    private float elapsedTimeUntil = 0.0f;

    void Start()
    {
        
    }

    void Update()
    {
        if(elapsedTimeUntil > timeUntilDestroy)
        {
            Destroy(gameObject);
        }
        elapsedTimeUntil += Time.deltaTime;
        transform.position += new Vector3(0, 3.0f * Time.deltaTime, 0.0f);
    }
}
