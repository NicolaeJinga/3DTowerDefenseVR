using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemRotation : MonoBehaviour
{
    public float speed = 10.0f;

    void Update()
    {
        gameObject.transform.RotateAround(gameObject.transform.up, speed * Time.deltaTime);
    }
}
