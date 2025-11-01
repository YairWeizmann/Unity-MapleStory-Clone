using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnClouds : MonoBehaviour
{
    public float CloudsSpeed;
    public void Update()
    {
        this.gameObject.transform.position = new Vector3(transform.position.x+CloudsSpeed, transform.position.y,transform.position.z);
    }
}


