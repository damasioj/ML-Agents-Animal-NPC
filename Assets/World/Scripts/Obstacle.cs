using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float maxX, minX, maxZ, minZ;

    public void Reset()
    {
        float x = Random.Range(minX, maxX);
        float z = Random.Range(minZ, maxZ);

        transform.position = new Vector3(x, transform.position.y, z);
    }
}
