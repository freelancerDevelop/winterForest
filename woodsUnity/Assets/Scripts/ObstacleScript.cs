using UnityEngine;
using System.Collections;

/// <summary>
/// The Obstacle script handles the calculation of a radius and 
/// position for a given obstacle in the world coordinates, for the
/// collision avoidance algorithm.
/// </summary>
[RequireComponent (typeof (BoxCollider))]
public class ObstacleScript : MonoBehaviour {

    public float radius; //want to check on this from the inspector for debug purposes, will be changed back to private after 

    //properties
    public float Radius
    { get { return radius; } }
    

   
	void Start()
    {
        //calculate the radius of the model by the dimensions of its box collider.
        float lenX = this.GetComponent<BoxCollider>().bounds.size.x;
        float lenZ = this.GetComponent<BoxCollider>().bounds.size.z;
        if (lenX < lenZ)
            radius = lenZ / 2;
        else
            radius = lenZ / 2;

    }

    
}

	