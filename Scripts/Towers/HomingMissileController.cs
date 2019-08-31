using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingMissileController : MonoBehaviour
{
    public float speed = 2.0f;
    [HideInInspector]
    public GameObject whatToFollow;

    void Update()
    {
        if (whatToFollow != null)
        {
            transform.LookAt(whatToFollow.transform);
            Vector3 vectorToTarget = whatToFollow.transform.position + new Vector3(0, whatToFollow.transform.localScale.y / 2.0f, 0) - transform.position;
            transform.position += vectorToTarget.normalized * speed * Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
