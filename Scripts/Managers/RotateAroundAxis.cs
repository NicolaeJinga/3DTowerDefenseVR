using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAroundAxis : MonoBehaviour
{
    void Update()
    {
        transform.RotateAround(-transform.forward, Time.deltaTime * 5.0f);
    }
}
