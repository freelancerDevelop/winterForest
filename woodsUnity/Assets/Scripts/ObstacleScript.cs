using UnityEngine;
using System.Collections;

[RequireComponent (typeof (BoxCollider))]
public class ObstacleScript : MonoBehaviour {

    private float radius = 3;
    private Vector3 position;
    public float Radius
    { get { return radius; } }
    public Vector3 Position
    { get { return this.transform.position; } set { position = value; } }

   
	//void Start()
    //{
        //float lenX = this.GetComponent<BoxCollider>().bounds.size.x;
        //float lenZ = this.GetComponent<BoxCollider>().bounds.size.z;
        //if (lenX < lenZ)
           // radius = lenZ / 2;
        //else
            //radius = lenZ / 2;

    //}
}

	