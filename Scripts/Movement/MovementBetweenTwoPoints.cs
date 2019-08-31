using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementBetweenTwoPoints : MonoBehaviour
{
    public GameObject pointA;
    public GameObject pointB;

    public float speed = 2.0f;

    private float distanceBetweenAandB;
    private Vector3 AminusB;

    private bool reachedA;
    private bool reachedB;

    private void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject.transform.parent.gameObject);
    }

    void Start()
    {
        AminusB = pointA.transform.position - pointB.transform.position;
        distanceBetweenAandB = AminusB.magnitude;
        transform.position = pointA.transform.position;
        reachedA = true;
        reachedB = false;
    }

    void Update()
    {
        float distanceFromA = (transform.position - pointA.transform.position).magnitude;
        float distanceFromB = (transform.position - pointB.transform.position).magnitude;

        if(distanceFromA > distanceBetweenAandB)
        {
            reachedA = false;
            reachedB = true;
        }
        else if (distanceFromB > distanceBetweenAandB)
        {
            reachedA = true;
            reachedB = false;
        }

        if (reachedA)
        {
            transform.position += -AminusB.normalized * speed * Time.deltaTime;
        }
        else if (reachedB)
        {
            transform.position += AminusB.normalized * speed * Time.deltaTime;
        }
    }

}
