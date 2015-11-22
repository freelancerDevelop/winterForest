using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider))]
public class ObstacleScript : MonoBehaviour {

    private float radius;
    private Vector3 position;
    public float Radius
    { get { return radius; } }
    public Vector3 Position
    { get { return this.transform.position; } }

	void Start()
    {
        Vector3 size = this.GetComponent<BoxCollider>().size;
        if (size.x < size.z)
            radius = size.z / 2;
        else
            radius = size.x / 2;
    }
}

	