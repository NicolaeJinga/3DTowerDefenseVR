using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    void Update()
    {
        gameObject.transform.LookAt(2*transform.position - Camera.main.transform.position);
    }
}
