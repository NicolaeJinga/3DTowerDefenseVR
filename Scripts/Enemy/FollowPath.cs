using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

//this class no longer does only follow path but also controls the attacks of enemies
public class FollowPath : MonoBehaviour 
{
    private GameObject path;
    private List<Transform> points = new List<Transform>();

    private float distanceFromPreviousPoint;
    private float distanceBetweenPreviousAndNextPoint;
    private int nextPointIndex;

    public float speed = 1.0f;
    public float rotationSpeed = 3.0f;

    public int damage = 1;
    public float inflictDamageEvery = 1.0f;
    private float elapsedDamageEvery = 0.0f;

    private GameObject gate;

    float waitBeforeWalk;
    float waitBeforeWalkElapsed = 0.0f;
    bool  gameOver = false;

    private void OnEnable()
    {
        GateHealth.OnDestroyed += GateDead;
    }

    private void OnDisable()
    {
        GateHealth.OnDestroyed -= GateDead;
    }

    void Start()
    {
        path = GameObject.FindGameObjectWithTag("Path");
        gate = GameObject.FindGameObjectWithTag("Gate");
        foreach(var child in path.transform.Cast<Transform>())
        {
            points.Add(child);
        }
        transform.position = points[0].position;
        nextPointIndex = 1;
        transform.LookAt(points[nextPointIndex].position);
        distanceFromPreviousPoint = (transform.position - points[nextPointIndex-1].position).magnitude;
        distanceBetweenPreviousAndNextPoint = (points[nextPointIndex].position - points[nextPointIndex - 1].position).magnitude;
    }
    
    void Update()
    {
        if(!ReachedEnd()) //follow path
        {
            if (distanceFromPreviousPoint > distanceBetweenPreviousAndNextPoint)
            {
                nextPointIndex++;
                if (nextPointIndex <= (points.Count - 1))
                    distanceBetweenPreviousAndNextPoint = (points[nextPointIndex].position - points[nextPointIndex - 1].position).magnitude;
            }
            
            if (nextPointIndex <= (points.Count - 1))
            {
                Vector3 destination = points[nextPointIndex].position - transform.position;
                Quaternion newRotation = Quaternion.LookRotation(destination);
                transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, rotationSpeed * Time.deltaTime);
                distanceFromPreviousPoint = (transform.position - points[nextPointIndex - 1].position).magnitude;
                Vector3 directionToWalk = (points[nextPointIndex].position - transform.position).normalized;
                transform.position += directionToWalk * speed * Time.deltaTime;
            }

        }
        else //starting inflicting damage
        {
            if(elapsedDamageEvery > inflictDamageEvery)
            {
                if (gate != null)
                {
                    gate.GetComponent<GateHealth>().TakeDamage(damage);
                    elapsedDamageEvery = 0.0f;
                }
            }
            elapsedDamageEvery += Time.deltaTime;
        }
        if(gameOver)
        {
            if(waitBeforeWalkElapsed >= waitBeforeWalk)
            {
                transform.position += transform.forward * speed * Time.deltaTime;
                GetComponent<Animator>().SetBool("reachedEnd", false);
            }
            waitBeforeWalkElapsed += Time.deltaTime;
        }
    }

    bool ReachedEnd()
    {
        if(nextPointIndex > (points.Count - 1))
        {
            GetComponent<Animator>().SetBool("reachedEnd", true);
            return true;
        }
        return false;
    }

    void GateDead(float gateFall)
    {
        waitBeforeWalk = gateFall;
        gameOver = true;
    }
}
